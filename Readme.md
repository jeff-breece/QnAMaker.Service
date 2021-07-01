# Introduction
This is to demo the following
- Programmatically calling QnA Maker and the newer Custom Question Answer service in Azure from an Azure Function
- The business requirement was to have the ability to route different client requests, as part of a quasi-conversational flow, to specific QnA Maker resources globally
- To initiate the service, create a POST request with a JSON body as described in the section titled Incoming request structure below
- The application then looks up the required QnA Maker instance variable required to send the question to the knowledge base
- Lastly, the application returns a string of whatever matched the user question

**Note,** as seen in the documentation in the References section below, precise answering, a new feature of the new Custom Question Answer service, is only availble from a Bot Service application, not through the API which could be used in something like an Azure Function API call. 

## Routing lookup data
While we used phone number as the id for routing a client to a given QnA Maker instance, a more unique organizational level vs client-member, would be a better pattern. It bears mention that in this scenario, each client will need to have a unique QnA Maker instance with a set of Question/Answer pairs that are intended for that organizations members.

The implementation for this is seen in the json file, mentioned below in the Setup section and then from the request perspective in the Incoming request structure below.
# Setup
 - Create a container in Azure blob Storage and save the name to the local.settings.json AZURE_BLOB_CONTAINER_NAME
 - Create a routing lookup json file and save the name to the local.settings.json in the CLIENT_LOOKUP_DATA variable

 ## Routing lookup json file format
 ```json
 [
	{
		"id": "phone-number-or-organizational-identifier",
		"endpointKeyVar": "your-endPoint-key-from-azure",
		"kbId": "your-knowledgebase-id-from-azure-qnamaker",
		"endPoint": "your-url-from-qnamaker-in-azure"
	}
]
```

# Incoming request structure
 ```json
{
	"id": "4696001111",
	"question": "Luis?"
}
```
# References
- [QnA Maker v5 Generate Answer Reference](https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker5.0preview1/knowledgebase/generate-answer)
- [Custom Question Answer](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/custom-question-answering)
- [Precise Answering](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/reference-precise-answering)
- [Get precise answers with GenerateAnswer API](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/how-to/metadata-generateanswer-usage?tabs=v2#get-precise-answers-with-generateanswer-api)