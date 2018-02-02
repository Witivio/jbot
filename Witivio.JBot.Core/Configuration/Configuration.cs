using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Witivio.JBot.Core.Configuration
{
    public interface IConfiguration
    {
        T Get<T>(string key);
    }

    public class Configuration : IConfiguration
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public Configuration(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T Get<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }
    }
}
