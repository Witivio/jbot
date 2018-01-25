using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public static class MediaTypeNames
    {
        public static class Application
        {
            public const string Json = "application/json";
            public const string XForm = "application/x-www-form-urlencoded";
        }

        public static class Text
        {
            public const string Html = "text/html";
            public const string Plain = "text/plain";
        }
    }
}
