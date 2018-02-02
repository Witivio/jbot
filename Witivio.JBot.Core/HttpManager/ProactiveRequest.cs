using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.HttpManager
{
    public interface IProactiveRequest
    {
        String getQueue();
    }

    class ProactiveRequest : IProactiveRequest
    {
        // authorization
        // check token or genereate and store

        IHttpClientFactory _httpManager;
        //OAuth _authToken;
        private readonly static String url = "http://google.com";
        public String getQueue()
        {
            return (null);
        }

        private async Task PeriodicRunProactive(TimeSpan interval)
        {
            while (true)
            {
                
                await Task.Delay(interval);
            }
        }

        public ProactiveRequest(IHttpClientFactory httpManager)
        {
            _httpManager = httpManager;

        }
    }
}
