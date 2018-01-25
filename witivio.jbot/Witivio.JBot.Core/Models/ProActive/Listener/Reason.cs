using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class Reason
    {

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("subcode")]
        public string Subcode { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }
    }
}
