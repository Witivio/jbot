using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Witivio.JBot.Core.Services.Communicator.Botndo.S4Bot.Core.UCWA;
namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleHttpClientFactory : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>().SingleInstance();
            base.Load(builder);
        }
    }
}
