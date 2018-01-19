using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.Services
{
    public class ConversationState
    {
        public string Watermark { get; set; }
        public Microsoft.Bot.Connector.DirectLine.Conversation Conversation { get; set; }
        public string From { get; set; }
        public MessageFormat SupportedFormat { get; set; }
    }

    public class ConversationTaskState
    {
        public Task Task { get; set; }
        public ConversationState ConversationState { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
