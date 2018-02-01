using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class ApplicationLinks
    {

        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("policies")]
        public Self Policies { get; set; }

        [JsonProperty("batch")]
        public Self Batch { get; set; }

        [JsonProperty("events")]
        public Self Events { get; set; }
    }
}
