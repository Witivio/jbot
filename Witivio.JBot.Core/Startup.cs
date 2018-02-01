using System;
using System.Reflection;
using System.Text;
using Autofac;
//    using Autofac.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Serilog;
using System.Diagnostics;
using Witivio.JBot.Core.Infrastructure;



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
using Microsoft.Bot.Connector.DirectLine;

namespace Witivio.JBot.Core
{
    public class Startup
    {
        internal JBotSettings Settings { get; set; }
        private IContainer _container;

        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }

        public RuntimeMode RuntimeMode { get; set; }

        public Startup(IHostingEnvironment env)
        {
            // TODO module
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            RuntimeMode = (RuntimeMode)Enum.Parse(typeof(RuntimeMode), env.EnvironmentName, true);










            ContainerBuilder builder2 = new ContainerBuilder();

            builder2.Register<Witivio.JBot.Core.Configuration.IConfiguration>((c, p) =>
            {
                return (new Witivio.JBot.Core.Configuration.Configuration(builder.Build()));
            }).As<Witivio.JBot.Core.Configuration.IConfiguration>().SingleInstance();

            builder2.RegisterType<BotIdProvider>().As<IBotIdProvider>().SingleInstance();
            builder2.Register<IDirectLineClient>((c, p) =>
            {
                var config = c.Resolve<Witivio.JBot.Core.Configuration.IConfiguration>();
                var directLineKey = config.Get<string>(ConfigurationKeys.Credentials.DirectLine);
                /*if (string.IsNullOrWhiteSpace(directLineKey))
                    return new BotndoConnectorClient(config);
                else*/
                return (new DirectLineClient(directLineKey));

            }).As<IDirectLineClient>().SingleInstance();

            builder2.Register<IConversationDataStore>((c, p) =>
            {
                return (new InMemoryDataStore());
            }).As<IConversationDataStore>().SingleInstance();

            builder2.Register<IScheduler>((c, p) =>
            {
                return (new Scheduler());
            }).As<IScheduler>().SingleInstance();

            builder2.Register<IMessageFormater>((c, p) =>
            {
                return (new MessageFormater());
            }).As<IMessageFormater>().SingleInstance();


            builder2.Register<Witivio.JBot.Core.Services.Configuration.IXmppProvider>((c, p) =>
            {
                var config = c.Resolve<Witivio.JBot.Core.Configuration.IConfiguration>();
                return (new Witivio.JBot.Core.Services.Configuration.XmppProvider(config));
            }).As<Witivio.JBot.Core.Services.Configuration.IXmppProvider>().SingleInstance();


            builder2.Register(c =>
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
            _container = builder2.Build();
            _container.Resolve<ICommunicationLinker>().StartAsync();
        }
        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, ICommunicationLinker communicationLinker, IHostingEnvironment env)
        {

        }
    }
}
