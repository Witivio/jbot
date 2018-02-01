using System;
using System.Collections.Generic;
using System.Text;

namespace Witivio.JBot.Core.Services
{
    public interface ITelemetryClient
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
    }
}
