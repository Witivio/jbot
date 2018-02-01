using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services.EventArgs
{
    public class MessageEventArgs : System.EventArgs
    {
        public string Message { get; set; }
        public string ConversationId { get; set; }
        public User From { get; set; }

        public User Bot { get; set; }
    }

    public class User
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}
