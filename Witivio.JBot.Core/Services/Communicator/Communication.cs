using Newtonsoft.Json;
using System.Collections.Generic;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class Communication : RelEtagBase
    {
        //[JsonProperty("eb4f588e-3c33-426a-bb89-b81c435c0384")]
        //public string Eb4f588e3c33426aBb89B81c435c0384 { get; set; }

        [JsonProperty("supportedModalities")]
        public IList<object> SupportedModalities { get; set; }

        [JsonProperty("supportedMessageFormats")]
        public IList<string> SupportedMessageFormats { get; set; }

        [JsonProperty("_links")]
        public ApplicationLinks Links { get; set; }
    }
}