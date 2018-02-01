using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Botndo.S4Bot.Core.UCWA;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    using Witivio.JBot.Core.Models;
    using Witivio.JBot.Core.Models.ProActive.Listener;
    using Witivio.JBot.Core.Services.Communicator;
    using Witivio.JBot.Core.Services.Communicator.Botndo.S4Bot.Core.UCWA;

    namespace Botndo.S4Bot.Core.UCWA
    {
        public interface IUCWACommunicator
        {
            Task AcceptIncomingMessaging(string acceptUri);
            Task<EventResponse> GetEvents(CancellationToken cancellationToken, string url = null);
            Task StartMessaging(string to, string operationId);
            string ApplicationId { get; }
        }

        public class UCWACommunicator : IUCWACommunicator
        {
            private OAuthToken _token;
            private Uri _applicationUri;
            private IPersistantDataStore _applicationDataStore;

            private MakeMeAvailableResponse _makeMeAvailable;
            //private readonly IBotIdProvider _botIdProvider;
            //private readonly ILogger<UCWACommunicator> _logger;
            private readonly IHttpClientFactory _httpClientFactory;
            private string _applicationId;
            public string ApplicationId
            {
                get
                {

                    return _applicationId;
                }
                private set
                {
                    _applicationId = value;
                }
            }

            public UCWACommunicator(IPersistantDataStore applicationDataStore, IHttpClientFactory httpClientFactory)
            {
                _applicationDataStore = applicationDataStore;
                _httpClientFactory = httpClientFactory;

                //_botIdProvider = botIdProvider;
                //_logger = logger;
            }

            public async Task<EventResponse> GetEvents(CancellationToken cancellationToken, string url = null)
            {
                if (string.IsNullOrEmpty(url))
                    url = _makeMeAvailable.Links.Events.Href;
                using (var client = _httpClientFactory.CreateWithAutorization(_token))
                {
                    var response = await client.GetJsonAsyncWithRetry<EventResponse>(_applicationUri.Scheme + "://" + _applicationUri.Host + url, cancellationToken).ConfigureAwait(false);
                    if (response == null)
                        return null;
                    return response.Result;
                }
            }

            public async Task AcceptIncomingMessaging(string acceptUri)
            {
                using (var client = _httpClientFactory.CreateWithAutorization(_token))
                {
                    var responseMessage = await client.PostAsyncWithRetry(_applicationUri.Scheme + "://" + _applicationUri.Host + acceptUri, "");
                    if (responseMessage.StatusCode != HttpStatusCode.NoContent)
                        throw new Exception("204 is expected for AcceptIncomingMessaging");
                }
            }

            public async Task StartMessaging(string to, string operationId)
            {
                dynamic json = new JObject();
                json.importance = "Normal";
                json.sessionContext = Guid.NewGuid().ToString();
                json.telemetryId = null;
                json.to = "sip:" + to;
                json.operationId = operationId;

                using (var client = _httpClientFactory.CreateWithAutorization(_token))
                {
                    var communicationJson = await client.GetStringAsyncWithRetry(_applicationUri.Scheme + "://" + _applicationUri.Host + _makeMeAvailable.Embedded.Communication.Links.Self.Href);
                    dynamic dynamicCommunicationJson = JObject.Parse(communicationJson);
                    string jsonToSend = JsonConvert.SerializeObject(json);
                    var responseMessage = await client.PostAsyncWithRetry(_applicationUri.Scheme + "://" + _applicationUri.Host + dynamicCommunicationJson._links.startMessaging.href, new StringContent(jsonToSend, Encoding.UTF8, MediaTypeNames.Application.Json));
                    if (responseMessage.StatusCode != HttpStatusCode.Created)
                        throw new Exception("201 is expected for PostAsync");
                }
            }
        }
    }
}
