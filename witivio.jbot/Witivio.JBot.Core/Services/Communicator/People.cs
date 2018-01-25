using Newtonsoft.Json;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class People : RelBase
    {
        [JsonProperty("_links")]
        public PeopleLinks Links { get; set; }
    }
}