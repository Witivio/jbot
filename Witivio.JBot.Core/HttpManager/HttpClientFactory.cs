using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Polly;
using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.HttpManager
{
    public interface IHttpClientFactory
    {
        ResilientHttpClient CreateWithAutorization(OAuthToken token);
        ResilientHttpClient Create();
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        public ResilientHttpClient CreateWithAutorization(OAuthToken token)
        {
            var client = Create();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token.TokenType + " " + token.AccessToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");

            return client;
        }

        public ResilientHttpClient Create()
        {
            return new ResilientHttpClient();
        }
    }
}
