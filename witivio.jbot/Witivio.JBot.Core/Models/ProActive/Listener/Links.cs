using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class Links
    {

        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("applications")]
        public Self Applications { get; set; }

        [JsonProperty("xframe")]
        public Self Xframe { get; set; }
    }
}
