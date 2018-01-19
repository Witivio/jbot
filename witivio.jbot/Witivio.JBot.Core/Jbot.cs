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
            var file = File.ReadAllText("appSettings.json");
            Settings = JsonConvert.DeserializeObject<JBotSettings>(file);
            if (string.IsNullOrEmpty(Settings.Credentials.DirectLineKey))
                throw new ArgumentNullException("missing directline key in configuration file");
            if (Settings.Credentials.DirectLineKey == null)
                throw new ArgumentNullException("missing XMPP credentials in configuration file");

            _communicationLinker = new CommunicationLinker(new DirectLineClient(Settings.Credentials.DirectLineKey),
                new JabberClient(Settings.Credentials.XmppCredentials));
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
