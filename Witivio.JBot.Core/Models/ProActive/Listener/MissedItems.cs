using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class MissedItems : RelBase
    {

        [JsonProperty("missedConversationsCount")]
        public int MissedConversationsCount { get; set; }

        [JsonProperty("unreadMissedConversationsCount")]
        public int UnreadMissedConversationsCount { get; set; }

        [JsonProperty("voiceMailsCount")]
        public int VoiceMailsCount { get; set; }

        [JsonProperty("unreadVoicemailsCount")]
        public int UnreadVoicemailsCount { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

    }
}
