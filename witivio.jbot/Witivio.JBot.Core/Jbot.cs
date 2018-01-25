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


using Witivio.JBot.Core.Services.Botndo.S4Bot.Core.UCWA;
using Witivio.JBot.Core.Services.Communicator.Botndo.S4Bot.Core.UCWA;
using Autofac;
using Witivio.JBot.Core.DependencyModule;
using System.Reflection;

namespace Witivio.JBot.Core
{
    public class Jbot : IDisposable
    {
        internal JBotSettings Settings { get; set; }
        private IContainer _container;

        private void registerModule(ContainerBuilder builder)
        {
            builder.RegisterModule<ModuleIAuth>();
            builder.RegisterModule<ModuleDataStore>();
            builder.RegisterModule<ModuleDirectLine>();
            builder.RegisterModule<ModuleHttpClientFactory>();
            builder.RegisterModule<ModuleJabberClient>();
            builder.RegisterModule<ModuleMessageFormater>();
            builder.RegisterModule<ModuleUCWACommunicator>();
        }

        public void Configure(RuntimeMode mode)
        {
            var file = File.ReadAllText("appsettings.json");
            Settings = JsonConvert.DeserializeObject<JBotSettings>(file);
            if (string.IsNullOrEmpty(Settings.Credentials.DirectLineKey))
                throw new ArgumentNullException("missing directline key in configuration file");
            if (string.IsNullOrEmpty(Settings.Credentials.BotId))
                throw new ArgumentNullException("missing bot id in configuration file");

            ContainerBuilder builder = new ContainerBuilder();

            registerModule(builder);
            builder.Register(c => new CommunicationLinker(
                new DirectLineClient(Settings.Credentials.DirectLineKey),
                new InMemoryDataStore(),
                new MessageFormater(),
                new JabberClient(
                    new XmppServerCredential(Settings.Credentials.XmppCredentials),
                    new InMemoryDataStore(),
                    Settings.Credentials.BotId,
                    new UCWACommunicator(new InMemoryDataStore(), new HttpClientFactory())),
                Settings.Credentials.BotId))
                .As<ICommunicationLinker>();
            _container = builder.Build();


            //public CommunicationLinker(IDirectLineClient directLineClient, IConversationDataStore conversationsStates, IMessageFormater messageFormater, JabberClient jabberClient)
            //public UCWACommunicator(IPersistantDataStore applicationDataStore, IHttpClientFactory httpClientFactory)
        }

        public void Start()
        {
            //Ajouter les logs
            _container.Resolve<ICommunicationLinker>().Start();
        }

        public void Dispose()
        {
            _container.Resolve<ICommunicationLinker>().Dispose();
        }
    }
}
