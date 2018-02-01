using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class MessageLinks
    {

        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("contact")]
        public Self Contact { get; set; }

        [JsonProperty("participant")]
        public Participant Participant { get; set; }

        [JsonProperty("messaging")]
        public Self Messaging { get; set; }

        [JsonProperty("plainMessage")]
        public Self PlainMessage { get; set; }

        [JsonProperty("htmlMessage")]
        public Self HtmlMessage { get; set; }
    }
}
