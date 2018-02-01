using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleIAuth : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<XmppServerCredential>().As<IAuth>().SingleInstance();
            base.Load(builder);
        }
    }
}
