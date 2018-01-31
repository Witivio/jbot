using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Witivio.JBot.Core;

namespace Witivio.JBot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var jbotService = new Jbot();
            jbotService.Configure(Core.Infrastructure.RuntimeMode.OnPremise);
            jbotService.Start();

            var cancelToken = new CancellationToken();
            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    jbotService.Dispose();
                    return;
                }
                Thread.Sleep(500);
            }
        }
    }
}
