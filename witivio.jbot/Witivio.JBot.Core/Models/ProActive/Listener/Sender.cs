using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class Sender : RelBase
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("events")]
        public IList<Event> Events { get; set; }
    }
}
