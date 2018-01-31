using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Configuration;

namespace Witivio.JBot.Core.Configuration
{
    public interface IBotIdProvider
    {
        string BotId { get; }
    }
    public class BotIdProvider : IBotIdProvider
    {
        public string BotId { get; set; }

        public BotIdProvider(IConfiguration configuration)
        {
            BotId = configuration.Get<string>(ConfigurationKeys.Credentials.BotId);
            if (string.IsNullOrWhiteSpace(BotId))
                BotId = Guid.NewGuid().ToString();
        }
    }
}