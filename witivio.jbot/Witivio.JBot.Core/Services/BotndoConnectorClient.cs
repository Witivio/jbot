using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public class BotndoConnectorClient : ServiceClient<BotndoConnectorClient>, IDirectLineClient
    {
        public Uri BaseUri { get; set; }

        public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public ServiceClientCredentials Credentials => throw new NotImplementedException();

        public IConversations Conversations { get; protected set; }

        public ITokens Tokens => throw new NotImplementedException();

        public BotndoConnectorClient(string baseUri, string botId)
        {
            this.BaseUri = new Uri(baseUri);
            this.Conversations = new BotndoConversations(this, botId);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class BotndoConversations : IConversations
    {
        private BotndoConnectorClient _client;
        private string _baseUri;
        private string _botId;

        public BotndoConversations(BotndoConnectorClient client, string botId)
        {
            _client = client;
            _baseUri = client.BaseUri.AbsoluteUri;
            _botId = botId;
        }

        public async Task<HttpOperationResponse<ActivitySet>> GetActivitiesWithHttpMessagesAsync(string conversationId, string watermark = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string uri = _baseUri + (_baseUri.EndsWith("/") ? "" : "/") + _botId + "/v3/conversations/" + conversationId + "/activities?watermark=" + watermark;

            var request = new HttpRequestMessage();
            request.Method = new HttpMethod("GET");
            request.RequestUri = new Uri(uri);


            var response = await _client.HttpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                HttpOperationResponse<ActivitySet> httpOperationResponse = new HttpOperationResponse<ActivitySet>();
                httpOperationResponse.Request = request;
                httpOperationResponse.Response = response;

                httpOperationResponse.Body = JsonConvert.DeserializeObject<ActivitySet>(json);
                return httpOperationResponse;
            }
            else
                throw new HttpOperationException();
        }

        public async Task<HttpOperationResponse<ResourceResponse>> PostActivityWithHttpMessagesAsync(string conversationId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string uri = _baseUri + (_baseUri.EndsWith("/") ? "" : "/") + _botId + "/v3/conversations/" + conversationId + "/activities";

            var request = new HttpRequestMessage();
            request.Method = new HttpMethod("POST");
            request.RequestUri = new Uri(uri);
            request.Content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");

            var response = await _client.HttpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                HttpOperationResponse<ResourceResponse> httpOperationResponse = new HttpOperationResponse<ResourceResponse>();
                httpOperationResponse.Request = request;
                httpOperationResponse.Response = response;

                httpOperationResponse.Body = JsonConvert.DeserializeObject<ResourceResponse>(json);
                return httpOperationResponse;
            }
            else
                throw new HttpOperationException();
        }

        public Task<HttpOperationResponse<Conversation>> ReconnectToConversationWithHttpMessagesAsync(string conversationId, string watermark = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<HttpOperationResponse<Conversation>>(null);
            // throw new NotImplementedException();
        }

        public async Task<HttpOperationResponse<Conversation>> StartConversationWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string uri = _baseUri + (_baseUri.EndsWith("/") ? "" : "/") + _botId + "/v3/conversations/";

            var request = new HttpRequestMessage();
            request.Method = new HttpMethod("POST");
            request.RequestUri = new Uri(uri);

            var response = await _client.HttpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                HttpOperationResponse<Conversation> httpOperationResponse = new HttpOperationResponse<Conversation>();
                httpOperationResponse.Request = request;
                httpOperationResponse.Response = response;

                httpOperationResponse.Body = JsonConvert.DeserializeObject<Conversation>(json);
                return httpOperationResponse;
            }
            else
                throw new HttpOperationException();
        }


        public Task<HttpOperationResponse<ResourceResponse>> UploadWithHttpMessagesAsync(string conversationId, Stream file, string userId = null, string contentType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
