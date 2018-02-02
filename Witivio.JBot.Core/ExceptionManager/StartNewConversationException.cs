using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.ExceptionManager
{
    [Serializable]
    public class StartNewConversationException : Exception
    {
        public UserPresence Presence { get; set; }

        public StartNewConversationException(UserPresence presence)
        {
            this.Presence = presence;
        }
    }
}
