using System;
using System.Collections.Generic;
using System.Text;

namespace QnAMaker.Service.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Context
    {
        public bool isContextOnly { get; set; }
        public List<object> prompts { get; set; }
    }

    public class Answer
    {
        public List<string> questions { get; set; }
        public string answer { get; set; }
        public double score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public bool isDocumentText { get; set; }
        public List<object> metadata { get; set; }
        public Context context { get; set; }
    }

    public class ResponsePayload
    {
        public List<Answer> answers { get; set; }
        public bool activeLearningEnabled { get; set; }
    }
}
