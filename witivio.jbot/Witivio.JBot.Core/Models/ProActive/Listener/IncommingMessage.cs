using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Services.EventArgs;

namespace Witivio.JBot.Core.Models.ProActive.Listener
{
    public class IncommingMessage
    {
        public string Message { get; set; }
        public string ConversationId { get; set; }
        public User From { get; set; }
        public MessageFormat Format { get; set; }
    }
}
