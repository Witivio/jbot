using Newtonsoft.Json;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class PeopleLinks
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("myContacts")]
        public Self MyContacts { get; set; }
    }
}