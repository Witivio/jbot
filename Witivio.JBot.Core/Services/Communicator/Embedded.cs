using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class Embedded
    {
        [JsonProperty("me")]
        public Me Me { get; set; }

        [JsonProperty("people")]
        public People People { get; set; }

        [JsonProperty("onlineMeetings")]
        public OnlineMeetings OnlineMeetings { get; set; }

        [JsonProperty("communication")]
        public Communication Communication { get; set; }
    }
}
