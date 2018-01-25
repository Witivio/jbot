using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

using Microsoft.Bot.Connector.DirectLine;

namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleDirectLine : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DirectLineClient>().As<IDirectLineClient>().SingleInstance();
            base.Load(builder);
        }
    }
}