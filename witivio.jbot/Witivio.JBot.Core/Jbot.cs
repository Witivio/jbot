using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core
{
    public class Jbot : IDisposable
    {
        internal JBotSettings Settings { get; set; }
        private ICommunicationLinker _communicationLinker;

        public void Configure(RuntimeMode mode)
        {
            var file = File.ReadAllText("appsettings.json");
            Settings = JsonConvert.DeserializeObject<JBotSettings>(file);

            if (string.IsNullOrEmpty(Settings.Credentials.DirectLineKey))
                throw new ArgumentNullException("missing directline key in configuration file");
            if (string.IsNullOrEmpty(Settings.Credentials.BotId))
                throw new ArgumentNullException("missing bot id in configuration file");
            if (Settings.Credentials.XmppCredentials == null
                || string.IsNullOrEmpty(Settings.Credentials.XmppCredentials.Host)
                || string.IsNullOrEmpty(Settings.Credentials.XmppCredentials.User)
                || string.IsNullOrEmpty(Settings.Credentials.XmppCredentials.Password)
                || string.IsNullOrEmpty(Settings.Credentials.XmppCredentials.Port))
                throw new ArgumentNullException("missing XMPP credentials in configuration file");
            _communicationLinker = new CommunicationLinker(new DirectLineClient(Settings.Credentials.DirectLineKey),
               new InMemoryDataStore(), new MessageFormater(),
               new JabberClient(Settings.Credentials.XmppCredentials, new InMemoryDataStore()), Settings.Credentials.BotId);
            //public CommunicationLinker(IDirectLineClient directLineClient, IConversationDataStore conversationsStates, IMessageFormater messageFormater, JabberClient jabberClient)
        }
        public void Start()
        {
            //Ajouter les logs
            _communicationLinker.Start();
        }


        public void Dispose()
        {
            _communicationLinker.Dispose();
        }

    }

    
}
