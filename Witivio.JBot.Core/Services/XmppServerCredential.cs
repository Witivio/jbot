using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core.Models
{
    public class XmppServerCredential : IAuth
    {
        [JsonProperty("Account")]
        public String Account { get; set; } = "admin@127.0.0.1";
        [JsonProperty("Host")]
        public String Host { get; set; } = "127.0.0.1";
        [JsonProperty("User")]
        public String User { get; set; } = "admin";
        [JsonProperty("Password")]
        public String Password { get; set; } = "password";
        [JsonProperty("Port")]
        public String Port { get; set; } = "5222";
        [JsonProperty("Tls")]
        public String Tls { get; set; } = "true";

        public String getCredential()
        {
            return (JsonConvert.SerializeObject(this));
        }

        public XmppServerCredential(XmppServerCredential toCpy)
        {
            if (toCpy != null)
            {
                this.Account = toCpy?.Account ?? this.Account;
                this.Host = toCpy?.Host ?? this.Host;
                this.User = toCpy?.User ?? this.User;
                this.Password = toCpy?.Password ?? this.Password;
                this.Port = toCpy?.Port ?? this.Port;
                this.Tls = toCpy?.Tls ?? this.Tls;
            }
        }
        public XmppServerCredential()
        {}
    }
}
