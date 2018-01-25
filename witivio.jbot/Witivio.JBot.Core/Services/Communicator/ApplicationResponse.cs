using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class ApplicationResponse : RelEtagBase
    {

        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        [JsonProperty("_links")]
        public ApplicationLinks Links { get; set; }

        [JsonProperty("_embedded")]
        public Embedded Embedded { get; set; }
    }
}
