using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class EventLinks
    {

        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("next")]
        public Self Next { get; set; }
    }
}
