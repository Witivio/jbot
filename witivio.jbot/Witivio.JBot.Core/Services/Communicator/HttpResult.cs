using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services.Communicator
{
    public class HttpResult<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Result { get; set; }
    }
}
