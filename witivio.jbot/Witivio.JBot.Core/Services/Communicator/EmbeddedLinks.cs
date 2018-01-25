using Newtonsoft.Json;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class EmbeddedLinks
    {

        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("makeMeAvailable")]
        public Self MakeMeAvailable { get; set; }

        [JsonProperty("presence")]
        public Self Presence { get; set; }

        [JsonProperty("reportMyActivity")]
        public Self ReportMyActivity { get; set; }

        [JsonProperty("location")]
        public Self Location { get; set; }
    }
}