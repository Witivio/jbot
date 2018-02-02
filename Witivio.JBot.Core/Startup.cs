using System;
using System.Text;
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



using Witivio.JBot.Core.Models;

using Autofac;
using Witivio.JBot.Core.DependencyModule;
using Witivio.JBot.Core.Services.HttpManager;
using Witivio.JBot.Core.Configuration;
using Witivio.JBot.Core.Infrastructure;
//using Witivio.JBot.Core.Services.Configuration;
using Microsoft.Bot.Connector.DirectLine;
using Autofac.Extensions.DependencyInjection;

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
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            RuntimeMode = (RuntimeMode)Enum.Parse(typeof(RuntimeMode), env.EnvironmentName, true);
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(Configuration);

            var builder = new ContainerBuilder();
            builder.RegisterModule(new JabberModule(RuntimeMode));
            builder.Populate(services);
            this.ApplicationContainer = builder.Build();
            return (ApplicationContainer.Resolve<IServiceProvider>());
        }
        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, ICommunicationLinker communicationLinker, IHostingEnvironment env)
        {
            if (RuntimeMode == RuntimeMode.Azure || RuntimeMode == RuntimeMode.Hybrid)
                app.ApplicationServices.GetService<TelemetryClient>().Context.Properties["Environment"] = env.EnvironmentName;
            app.UseMvc();
            communicationLinker.StartAsync();
            app.Run(context =>
            {
                var welcome = new StringBuilder("Welcome to jabber Connector by Witivio.");
                return context.Response.WriteAsync(welcome.ToString());
            });
            appLifetime.ApplicationStopped.Register(() =>
            {
                this.ApplicationContainer.Dispose();
                communicationLinker.Dispose();
            });
        }
    }
}