using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Witivio.JBot.Core.Services.Botndo.S4Bot.Core.UCWA;
namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleUCWACommunicator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UCWACommunicator>().As<IUCWACommunicator>().SingleInstance();
            base.Load(builder);
        }
    }
}