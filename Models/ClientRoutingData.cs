using System;
using System.Collections.Generic;
using System.Text;

namespace QnAMaker.Service.Models
{
    class ClientRoutingData
    {
        public string id { get; set; }
        public string endpointKeyVar { get; set; }
        public string kbId { get; set; }

        public string endPoint { get; set; }
    }
}
