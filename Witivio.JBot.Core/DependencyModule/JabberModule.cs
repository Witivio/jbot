using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Autofac;
using Microsoft.Bot.Connector.DirectLine;
using Witivio.JBot.Core.Configuration;
using Witivio.JBot.Core.Infrastructure;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core.DependencyModule
{
    public class JabberModule : Module
    {
        private readonly RuntimeMode _runtimeMode;

        public JabberModule(RuntimeMode runtimeMode)
        {
            _runtimeMode = runtimeMode;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(this.ThisAssembly)
                .AsImplementedInterfaces()
                .SingleInstance();

            if (_runtimeMode == RuntimeMode.OnPremise)
            {
                builder.RegisterType<InMemoryDataStore>().As<IConversationDataStore>().SingleInstance();
                builder.RegisterType<InMemoryDataStore>().As<IPersistantDataStore>().SingleInstance();
            }
            else
            {/*
                builder.RegisterType<InMemoryDataStore>().As<IConversationDataStore>().SingleInstance();
                builder.RegisterType<TableDataStore>().As<IPersistantDataStore>().WithParameter("tableName", "s4bStore").SingleInstance();
                builder.RegisterType<TableDataStore>().As<IStatisticsDataStore>().WithParameter("tableName", "s4bStatistics").SingleInstance();
                builder.RegisterType<ApplicationInsightTelemetryClient>().As<ITelemetryClient>().SingleInstance();
                */
            }

            builder.RegisterType<Scheduler>().As<IScheduler>().InstancePerDependency();
            builder.RegisterType<BotIdProvider>().As<IBotIdProvider>().SingleInstance();

            /*
            builder.Register((c, p) =>
            {
                var managementService = c.Resolve<IAzureManagementService>();
                return new KeyVaultClient(managementService.AuthenticateAsync);

            }).As<IKeyVaultClient>().SingleInstance();
            */

            builder.Register<IDirectLineClient>((c, p) =>
            {
                var config = c.Resolve<IConfiguration>();
                var directLineKey = config.Get<string>(ConfigurationKeys.Credentials.DirectLine);
                /*
                if (string.IsNullOrWhiteSpace(directLineKey))
                    return new BotndoConnectorClient(config);
                else*/
                    return new DirectLineClient(directLineKey);

            }).As<IDirectLineClient>().SingleInstance();

            base.Load(builder);
        }
    }
}