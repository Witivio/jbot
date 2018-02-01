using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class Event
    {

        [JsonProperty("link")]
        public EventLink Link { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("_embedded")]
        public EmbeddedEvent Embedded { get; set; }

        [JsonProperty("reason")]
        public Reason Reason { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
