using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class MessagingLinks
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("conversation")]
        public Self Conversation { get; set; }

        [JsonProperty("contact")]
        public Self Contact { get; set; }

        [JsonProperty("contactPresence")]
        public Self ContactPresence { get; set; }

        [JsonProperty("contactPhoto")]
        public Self ContactPhoto { get; set; }

        [JsonProperty("accept")]
        public Self Accept { get; set; }

        [JsonProperty("decline")]
        public Self Decline { get; set; }
        [JsonProperty("messaging")]
        public Self Messaging { get; set; }
        [JsonProperty("message")]
        public Self Message { get; set; }

        [JsonProperty("sendMessage")]
        public Self SendMessage { get; set; }

        [JsonProperty("setIsTyping")]
        public Self SetIsTyping { get; set; }

        [JsonProperty("stopMessaging")]
        public Self StopMessaging { get; set; }

        [JsonProperty("to")]
        public Self To { get; set; }
    }
}
