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


using Autofac;
using Witivio.JBot.Core.DependencyModule;
using System.Reflection;
using Witivio.JBot.Core.Services.HttpManager;
using Witivio.JBot.Core.Configuration;
using Witivio.JBot.Core.Infrastructure;
//using Witivio.JBot.Core.Services.Configuration;
using Microsoft.Extensions.Configuration;

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
            ContainerBuilder builder = new ContainerBuilder();
            registerModule(builder);

            builder.Register<Witivio.JBot.Core.Configuration.IConfiguration>((c, p) =>
            {
                var builder2 = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                return (new Witivio.JBot.Core.Configuration.Configuration(builder2.Build()));
            }).As<Witivio.JBot.Core.Configuration.IConfiguration>().SingleInstance();

            builder.RegisterType<BotIdProvider>().As<IBotIdProvider>().SingleInstance();
            builder.Register<IDirectLineClient>((c, p) =>
            {
                var config = c.Resolve<Witivio.JBot.Core.Configuration.IConfiguration>();
                var directLineKey = config.Get<string>(ConfigurationKeys.Credentials.DirectLine);
                /*if (string.IsNullOrWhiteSpace(directLineKey))
                    return new BotndoConnectorClient(config);
                else*/
                    return new DirectLineClient(directLineKey);

            }).As<IDirectLineClient>().SingleInstance();
            //IConfiguration config = new Configuration();

            builder.Register(c => 
            {
                var config = c.Resolve<Witivio.JBot.Core.Configuration.IConfiguration>();
                var directline = c.Resolve<IDirectLineClient>();
                return new CommunicationLinker(
                    directline,
                    new InMemoryDataStore(),
                    new MessageFormater(),
                    new JabberClient(
                        new Witivio.JBot.Core.Services.Configuration.XmppProvider(config),
                        config,
                        new InMemoryDataStore(),
                        new ProactiveRequest(new HttpClientFactory())),
                        config);
            }).As<ICommunicationLinker>();
            _container = builder.Build();

            //public JabberClient(IXmppProvider xmppProvider, IConfiguration config, IPersistantDataStore applicationDataStore, IProactiveRequest proactiveRequest)
            //public CommunicationLinker(IDirectLineClient directLineClient, IConversationDataStore conversationsStates, IMessageFormater messageFormater, JabberClient jabberClient)
            //public UCWACommunicator(IPersistantDataStore applicationDataStore, IHttpClientFactory httpClientFactory)
        }

        public void Start()
        {
            //Ajouter les logs
            _container.Resolve<ICommunicationLinker>().StartAsync();
        }

        public void Dispose()
        {
            _container.Resolve<ICommunicationLinker>().Dispose();
        }
    }
}
