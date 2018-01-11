using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.Ioc
{
    class JBotModule : Module
    {
        private readonly RuntimeMode _runtimeMode;

        public JBotModule(RuntimeMode runtimeMode)
        {
            _runtimeMode = runtimeMode;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(this.ThisAssembly)
                .AsImplementedInterfaces()
                .SingleInstance();

           
            base.Load(builder);
        }
    }
}
