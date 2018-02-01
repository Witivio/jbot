using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleMessageFormater : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageFormater>().As<IMessageFormater>().SingleInstance();
            base.Load(builder);
        }
    }
}
