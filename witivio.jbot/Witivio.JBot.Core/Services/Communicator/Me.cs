using Newtonsoft.Json;
using System.Collections.Generic;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class Me : RelBase
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("emailAddresses")]
        public IList<string> EmailAddresses { get; set; }

        [JsonProperty("_links")]
        public EmbeddedLinks Links { get; set; }
    }
}