using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Infrastructure
{
    public interface IRuntimeModeEnvironment
    {
        RuntimeMode Mode { get; }
        bool IsOnPremise { get; }
        bool IsAzure { get; }
        bool IsHybrid { get; }
    }
}
