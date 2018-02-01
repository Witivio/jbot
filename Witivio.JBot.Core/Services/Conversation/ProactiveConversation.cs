using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Bot.Connector.DirectLine;

namespace Witivio.JBot.Core.Services
{
    public class ProactiveConversation
    {
        public Activity Activity { get; set; }
        public Conversation DirectLineConversation { get; internal set; }
    }
}
