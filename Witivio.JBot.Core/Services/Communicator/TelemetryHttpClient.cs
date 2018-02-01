using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services.Communicator
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
//    using Microsoft.ApplicationInsights;
    using Newtonsoft.Json;
    using Polly;
    using System.Collections.Generic;
    using Witivio.JBot.Core.Infrastructure;
    using Witivio.JBot.Core.Services.HttpManager;

    namespace Botndo.S4Bot.Core.UCWA
    {

        public class ResilientHttpClient : IDisposable
        {
            private readonly HttpClient _client;



            public ResilientHttpClient()
            {
                _client = new HttpClient();
                DefaultRequestHeaders = _client.DefaultRequestHeaders;
            }

            public System.Net.Http.Headers.HttpRequestHeaders DefaultRequestHeaders { get; private set; }

            public Task<HttpResponseMessage> PostAsyncWithRetry(string uri, string content)
            {
                return PostAsyncWithRetry(uri, new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));



            }

            public Task<string> GetStringAsyncWithRetry(string uri)
            {
                return HttpInvoker(uri, async () =>
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                    _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", MediaTypeNames.Application.Json);
                    var response = await _client.SendAsync(requestMessage);
                    if (!response.IsSuccessStatusCode)
                        return null;
                    string str = await response.Content.ReadAsStringAsync();
                    str = FixFirstChar(str);
                    return str;
                });
            }

            public async Task<string> GetStringAsync(string uri)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", MediaTypeNames.Application.Json);
                var response = await _client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                    return null;
                string str = await response.Content.ReadAsStringAsync();
                str = FixFirstChar(str);
                return str;
            }

            public Task<HttpResponseMessage> PostAsyncWithRetry(string uri, StringContent content)
            {
                return HttpInvoker(uri, async () =>
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                    requestMessage.Content = content;
                    var response = await _client.SendAsync(requestMessage);
                    return response;
                });
            }

            internal Task PutAsyncWithRetry(string uri, StringContent stringContent, Tuple<string, string> header = null)
            {

                return HttpInvoker(uri, async () =>
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Put, uri);
                    requestMessage.Content = stringContent;
                    if (header != null)
                        _client.DefaultRequestHeaders.TryAddWithoutValidation(header.Item1, header.Item2);
                    var response = await _client.SendAsync(requestMessage);
                    return response;
                });
            }

            public Task<HttpResult<T>> GetJsonAsyncWithRetry<T>(string uri) where T : class
            {
                return GetJsonAsyncWithRetry<T>(uri, CancellationToken.None);
            }


            public Task<HttpResult<T>> GetJsonAsyncWithRetry<T>(string url, CancellationToken cancellationToken) where T : class
            {
                return HttpInvoker(url, async () =>
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                    try
                    {
                        var response = await _client.SendAsync(requestMessage, cancellationToken);
                        var result = new HttpResult<T>();
                        string json = await response.Content.ReadAsStringAsync();

                        json = FixFirstChar(json);

                        result.Result = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                        {
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        result.StatusCode = response.StatusCode;
                        return result;
                    }
                    catch (TaskCanceledException)
                    {
                        return null;
                    }
                });
            }

            private string FixFirstChar(string json)
            {
                if (json[0] == 65279)
                    json = json.TrimStart((char)65279);
                return json;
            }



            private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
            {
                var normalizedOrigin = NormalizeOrigin(origin);

                return await RetryPolicies.Retry.ExecuteAsync(action, new Context(normalizedOrigin));
            }


            private static string NormalizeOrigin(string origin)
            {
                return origin?.Trim()?.ToLower();
            }

            private static string GetOriginFromUri(string uri)
            {
                var url = new Uri(uri);

                var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";

                return origin;
            }

            public void Dispose()
            {
                _client?.Dispose();
            }


        }
    }
}
