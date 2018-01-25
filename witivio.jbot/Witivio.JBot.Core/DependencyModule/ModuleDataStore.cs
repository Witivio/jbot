using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core.DependencyModule
{
    class ModuleDataStore : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryDataStore>().As<IDataStore>().SingleInstance();
            base.Load(builder);
        }
    }
}
