using S22.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Infrastructure
{
    public static class Extensions
    {
        public static string ToEmail(this Jid jid)
        {
            return jid.Node + "@" + jid.Domain;
        }
    }
}
