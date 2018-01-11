using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Witivio.JBot.Core.Ioc;
using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core
{
    public class Startup
    {

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

            if (Debugger.IsAttached)
            {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(Configuration);

            if (RuntimeMode == RuntimeMode.Azure || RuntimeMode == RuntimeMode.Hybrid)
            {
                var aiOptions = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
                aiOptions.EnableQuickPulseMetricStream = true;
                aiOptions.InstrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");

                services.AddApplicationInsightsTelemetry(aiOptions);
            }

            var builder = new ContainerBuilder();
            builder.RegisterModule(new JBotModule(RuntimeMode));
            builder.Populate(services);
            this.ApplicationContainer = builder.Build();

            if (RuntimeMode == RuntimeMode.Azure || RuntimeMode == RuntimeMode.Hybrid)
                TelemetryConfiguration.Active.TelemetryInitializers.Add(ApplicationContainer.Resolve<ITelemetryInitializer>());


            return ApplicationContainer.Resolve<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime, ILoggerFactory loggerFactory)
        {

            if (RuntimeMode == RuntimeMode.Azure || RuntimeMode == RuntimeMode.Hybrid)
                app.ApplicationServices.GetService<TelemetryClient>().Context.Properties["Environment"] = env.EnvironmentName;

            loggerFactory
                    .AddDebug(LogLevel.Error)
                    .AddConsole(Configuration.GetSection("Logging"))
                    .AddApplicationInsights(app.ApplicationServices, LogLevel.Information);

            if (RuntimeMode == RuntimeMode.OnPremise)
            {
                var log = new LoggerConfiguration()
                    .WriteTo.RollingFile(Configuration.GetValue<string>("Logging:Path") + "\\log-{Date}.txt", retainedFileCountLimit: 15, fileSizeLimitBytes: 2621440)
                    .CreateLogger();

                loggerFactory.AddSerilog(log);
            }

            app.UseMvc();

            app.Run((context) =>
            {
                var welcome = new StringBuilder("Welcome to S4Bot Connector by Witivio.");
                welcome.AppendLine();
                welcome.AppendLine($"Version: {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");

                return context.Response.WriteAsync(welcome.ToString());
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                this.ApplicationContainer.Dispose();
            });
        }
    }
}
