using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.Services.EventArgs
{
    public class ConversationEventArgs : System.EventArgs
    {
        public string ConversationId { get; set; }
        public User From { get; set; }
        public User Bot { get; set; }
    }

    public class NewConversationEventArgs : ConversationEventArgs
    {
        public string Message { get; set; }

        public NewConversationEventArgs(User from, User bot, string message, string convid)
        {
            this.Bot = bot;
            this.From = from;
            this.Message = message;
            this.ConversationId = convid;
        }
    }

    public class ProActiveConversationEventArgs : System.EventArgs
    {
        public string ConversationId { get; set; }
        public User From { get; set; }
        public Microsoft.Bot.Connector.DirectLine.Conversation DirecLineConversation { get; set; }

        public MessageFormat SupportedFormat { get; set; }
    }
}