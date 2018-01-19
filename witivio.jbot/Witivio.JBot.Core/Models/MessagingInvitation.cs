using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
namespace Witivio.JBot.Core.Models
{
    public class MessagingInvitation
    {

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("importance")]
        public string Importance { get; set; }

        [JsonProperty("threadId")]
        public string ThreadId { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("rel")]
        public string Rel { get; set; }

        public string Message { get; set; }
    }
}