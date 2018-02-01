using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models.ProActive.Listener;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class RelEtagBase : RelBase
    {
        [JsonProperty("etag")]
        public string Etag { get; set; }
    }
}
