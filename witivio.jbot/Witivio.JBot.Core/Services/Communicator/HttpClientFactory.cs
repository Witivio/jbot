using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.Services.Communicator
{
    namespace Botndo.S4Bot.Core.UCWA
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
}
