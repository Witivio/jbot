using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Models
{
    class ProActiveMessageResponse
    {
        public ProActiveMessageResult Result { get; set; }
        public string BotId { get; set; }
        public string ActivityId { get; set; }
    }
}
