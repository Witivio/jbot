using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class EventResponse
    {
        [JsonProperty("_links")]
        public EventLinks Links { get; set; }
        [JsonProperty("sender")]
        public IList<Sender> Sender { get; set; }
    }
}
