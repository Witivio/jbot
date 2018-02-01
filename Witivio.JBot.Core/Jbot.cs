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
using System.Diagnostics;

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
                    return (new DirectLineClient(directLineKey));

            }).As<IDirectLineClient>().SingleInstance();

            builder.Register<IConversationDataStore>((c, p) =>
            {
                return (new InMemoryDataStore());
            }).As<IConversationDataStore>().SingleInstance();

            builder.Register<IScheduler>((c, p) =>
            {
                return (new Scheduler());
            }).As<IScheduler>().SingleInstance();

            builder.Register<IMessageFormater>((c, p) =>
            {
                return (new MessageFormater());
            }).As<IMessageFormater>().SingleInstance();


            builder.Register<Witivio.JBot.Core.Services.Configuration.IXmppProvider>((c, p) =>
            {
                var config = c.Resolve<Witivio.JBot.Core.Configuration.IConfiguration>();
                return (new Witivio.JBot.Core.Services.Configuration.XmppProvider(config));
            }).As<Witivio.JBot.Core.Services.Configuration.IXmppProvider>().SingleInstance();


            builder.Register(c => 
            {
                var config = c.Resolve<Witivio.JBot.Core.Configuration.IConfiguration>();
                var directline = c.Resolve<IDirectLineClient>();
                var scheduler = c.Resolve<IScheduler>();
                var datastore = c.Resolve<IConversationDataStore>();
                var formater = c.Resolve<IMessageFormater>();
                var auth = c.Resolve<Witivio.JBot.Core.Services.Configuration.IXmppProvider>();

                return new CommunicationLinker(
                    directline,
                    datastore,
                    formater,
                    new JabberClient(
                        auth,
                        config,
                        new ProactiveRequest(new HttpClientFactory())),
                        config, scheduler);
            }).As<ICommunicationLinker>();
            _container = builder.Build();

            Debug.WriteLine("config Bot");
            /*
            public CommunicationLinker(IDirectLineClient directLineClient, ISkypeCredentialProvider skypeCredentialProvider, IUCWAClient ucwaClient,
                IConversationDataStore conversationsStates, IMessageFormater messageFormater, ILogger<CommunicationLinker> logger,
                IStatisticsService statisticsService, IScheduler unActiveConversationScheduler, ITelemetryClient telemetryClient, IBotIdProvider botIdProvider)
            */
            //public JabberClient(IXmppProvider xmppProvider, IConfiguration config, IPersistantDataStore applicationDataStore, IProactiveRequest proactiveRequest)
            //public UCWACommunicator(IPersistantDataStore applicationDataStore, IHttpClientFactory httpClientFactory)
        }

        public void Start()
        {
            //Ajouter les logs
            Debug.WriteLine("Start Bot");
            _container.Resolve<ICommunicationLinker>().StartAsync();
        }

        public void Dispose()
        {
            _container.Resolve<ICommunicationLinker>().Dispose();
        }
    }
}
