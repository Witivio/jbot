using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class EmbeddedEvent
    {

        [JsonProperty("messagingInvitation")]
        public MessagingInvitation MessagingInvitation { get; set; }

        [JsonProperty("messaging")]
        public Messaging Messaging { get; set; }

        [JsonProperty("missedItems")]
        public MissedItems MissedItems { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}
