using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models
{
    internal class JBotSettings
    {
        public LoggingSettings Logging { get; set; }
        public Credentials Credentials { get; set; }
    }

    public class LoggingSettings
    {
        public bool IncludeScopes { get; set; }
        public string LogLevel { get; set; }
    }

    public class Credentials
    {
        public string DirectLineKey { get; set; }
        [JsonProperty("BotIdProvider")]
        public string BotId { get; set; }
        [JsonProperty("Xmpp")]
        public XmppServerCredential XmppCredentials { get; set; }
    }
}
