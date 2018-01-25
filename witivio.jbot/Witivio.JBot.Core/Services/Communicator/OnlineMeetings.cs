using Newtonsoft.Json;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class OnlineMeetings : RelBase
    {
        [JsonProperty("_links")]
        public ApplicationLinks Links { get; set; }
    }
}