using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Witivio.JBot.Core.Infrastructure
{
    public class RetryPolicies
    {

        public static Policy Retry = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(0.5));

        public static Policy RetryForStats = Policy.Handle<Exception>()
            .WaitAndRetryAsync(5, i => TimeSpan.FromMinutes(1));
    }
}
