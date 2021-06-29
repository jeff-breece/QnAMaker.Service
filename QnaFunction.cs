using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QnAMaker.Service.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace QnAMaker.Service
{
    public static class QnaFunction
    {
        [FunctionName("RequestAnswer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            QuestionPackage questionPayload;
            using (var reader = new StreamReader(req.Body))
            {
                var body = await reader.ReadToEndAsync();
                questionPayload = JsonConvert.DeserializeObject<QuestionPackage>(body);
            }
            log.LogInformation($"Processing incoming question '{questionPayload.question}' for id '{questionPayload.id}'.");
            log.LogInformation("Looking up which QnA instance to route the question to by perfroming a Mock lookup for the demo");
            var customerLookupData = GetClientRoutingData();
            ResponsePayload qnaResponse = new ResponsePayload();

            if (customerLookupData.Count > 0)
            {
                var clientReferenceObject = customerLookupData.Where(p => p.id == questionPayload.id).First();
                string endpointKeyVar = clientReferenceObject.endpointKeyVar;
                string kbIdVar = clientReferenceObject.kbId;
                string uri = string.Empty;
                
                string endpointVar = "https://sibte-textanalytics.cognitiveservices.azure.com/qnamaker";
                // https://sibte-textanalytics.cognitiveservices.azure.com/qnamaker/v5.0-preview.2/knowledgebases/bf3d01c6-0f2c-40ff-8824-5e702bee8adf/generateAnswer
                if (questionPayload.question.Equals("Luis?"))
                {
                    endpointVar = "https://qnamakerproof.azurewebsites.net/qnamaker";
                    uri = endpointVar + "/knowledgebases/" + kbIdVar + "/generateAnswer";
                }
                else
                {
                    endpointVar = "https://sibte-textanalytics.cognitiveservices.azure.com/qnamaker";
                    uri = endpointVar + "/v5.0-preview.2/knowledgebases/" + kbIdVar + "/generateAnswer";
                }
                string question = $"{{'question': '{questionPayload.question}','top': 1}}";
                try
                {
                    using (var client = new HttpClient())
                    using (var request = new HttpRequestMessage())
                    {
                        request.Method = HttpMethod.Post;
                        request.RequestUri = new Uri(uri);
                        request.Content = new StringContent(question, Encoding.UTF8, "application/json");
                        if (questionPayload.question.Equals("Luis?"))
                        {
                            request.Headers.Add("Authorization", "EndpointKey " + endpointKeyVar);
                        }
                        else
                            request.Headers.Add("Ocp-Apim-Subscription-Key", endpointKeyVar);
                        var response = client.SendAsync(request).Result;
                        var jsonResponse = response.Content.ReadAsStringAsync().Result;
                        qnaResponse = JsonConvert.DeserializeObject<ResponsePayload>(jsonResponse); 
                        log.LogInformation($"QnA Maker provided response: '{qnaResponse.answers.First().answer}.'");

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new BadRequestResult();
                }
            }
            return new OkObjectResult(qnaResponse.answers.First().answer);
        }

        private static List<ClientRoutingData> GetClientRoutingData()
        {
            string blobStoragConnStr = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobStoragConnStr);
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = serviceClient.GetContainerReference(Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER_NAME"));
            CloudBlockBlob blob = container.GetBlockBlobReference(Environment.GetEnvironmentVariable("CLIENT_LOOKUP_DATA"));
            var blobContents = blob.DownloadTextAsync().Result;
            List<ClientRoutingData> clientLookUpData = JsonConvert.DeserializeObject<List<ClientRoutingData>>(blobContents);
            return clientLookUpData;
        }
    }
}
