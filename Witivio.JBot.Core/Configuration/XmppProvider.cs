using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Configuration;
using Witivio.JBot.Core.Models;

using Microsoft.Azure.KeyVault;
using Microsoft.Bot.Connector.DirectLine;
using Witivio.JBot.Core.Infrastructure;

namespace Witivio.JBot.Core.Services.Configuration
{
    public interface IXmppProvider
    {
        XmppServerCredential GetLog();
    }

    public class XmppProvider : IXmppProvider
    {
        private readonly IConfiguration _configuration;
        //private readonly IKeyVaultClient _keyVaultClient; TODO ADD KEYVAULT
        //private readonly IRuntimeModeEnvironment _runtimeEnvironment;

        public XmppProvider(IConfiguration configuration/*, IKeyVaultClient keyVaultClient, IRuntimeModeEnvironment runtimeEnvironment*/)
        {
            _configuration = configuration;
            //_keyVaultClient = keyVaultClient;
            //_runtimeEnvironment = runtimeEnvironment;
        }

        public XmppServerCredential GetLog()
        {
            return new XmppServerCredential
            {
                Account = _configuration.Get<string>(ConfigurationKeys.Credentials.Account),
                Host = _configuration.Get<string>(ConfigurationKeys.Credentials.Host),
                Port = _configuration.Get<string>(ConfigurationKeys.Credentials.Port),
                User = _configuration.Get<string>(ConfigurationKeys.Credentials.User),
                Password = _configuration.Get<string>(ConfigurationKeys.Credentials.Password),
                Tls = _configuration.Get<string>(ConfigurationKeys.Credentials.Tls),
            };
        }
    }
}

/*

 namespace Witivio.JBot.Core.Models
{
    public class XmppServerCredential : IAuth
    {
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
*/
