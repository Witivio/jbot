using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleJabberClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<JabberClient>().As<IJabberClient>().SingleInstance().WithParameter(TypedParameter.From());
            base.Load(builder);
        }
    }
}