using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class Messaging : RelBase
    {

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("_links")]
        public MessagingLinks Links { get; set; }
    }
}
