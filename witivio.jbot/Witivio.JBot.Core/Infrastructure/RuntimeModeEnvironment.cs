using System;
//using Microsoft.AspNetCore.Hosting;

namespace Witivio.JBot.Core.Infrastructure
{
    public class RuntimeModeEnvironment : IRuntimeModeEnvironment
    {
        public RuntimeModeEnvironment(/*IHostingEnvironment env*/)
        {
            //Mode = (RuntimeMode)Enum.Parse(typeof(RuntimeMode), env.EnvironmentName, true);
        }

        public RuntimeMode Mode { get; set; }

        public bool IsOnPremise => Mode == RuntimeMode.OnPremise;
        public bool IsAzure => Mode == RuntimeMode.Azure;
        public bool IsHybrid => Mode == RuntimeMode.Hybrid;
    }
}
