using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{

    public class model_access_token
    {
        public String token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public String access_token { get; set; }
    }

    public class model_bot_response
    {
        public String token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public String access_token { get; set; }
    }

    public class model_message_send
    {
        public String conversation { get; set; }
        public String from { get; set; }
        public String locale { get; set; }
        public String recipient { get; set; }
        public String replyToId { get; set; }
        public String type { get; set; }
    }


    static class GenericRequest
    {
        static public HttpResponseMessage PostMethod(List<KeyValuePair<String, String>> values, String url, String host, bool isJson, String token_type = null, String access_token = null)
        {
            HttpClient webClient = new HttpClient();
            if (isJson == false)
            {
                webClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            }
            else
            {
                webClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            webClient.DefaultRequestHeaders.TryAddWithoutValidation("Host", host);
            if (token_type != null && access_token != null)
                webClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token_type, access_token);
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            return (webClient.PostAsync(url, content).Result);
        }
    }

    class MicrosoftTokenManager
    {
        String _clientID;
        String _clientSecret;
        String _token_type;
        String _access_token;

        public async Task<model_access_token> GetAccessToken()
        {
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("client_id", _clientID));
            values.Add(new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default"));
            values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            values.Add(new KeyValuePair<string, string>("client_secret", _clientSecret));

            HttpResponseMessage Message = GenericRequest.PostMethod(values, "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", "https://login.microsoftonline.com", false);
            String Content = await Message.Content.ReadAsStringAsync();
            model_access_token AccessTokenClass = SerializationClass.Deserialize<model_access_token>(Content);
            _token_type = AccessTokenClass.token_type;
            _access_token = AccessTokenClass.access_token;
            //Console.WriteLine(AccessTokenClass.access_token);
            //return (await Task.FromResult(AccessTokenClass.access_token));
            return (AccessTokenClass);
        }
        public async Task<model_bot_response> SendPostWithAuthotization(String token_type = null, String access_token = null)
        {
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("client_id", _clientID));
            values.Add(new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default"));
            values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            values.Add(new KeyValuePair<string, string>("client_secret", _clientSecret));

            HttpResponseMessage Message;
            String ToRequest = "https://smba.trafficmanager.net/apis/v3/conversations/123456/activities/123456";
            if (token_type != null && access_token != null)
                Message = GenericRequest.PostMethod(values, ToRequest, "https://login.microsoftonline.com", true, token_type, access_token);
            else
                Message = GenericRequest.PostMethod(values, ToRequest, "https://login.microsoftonline.com", true, _token_type, _access_token);
            String Content = await Message.Content.ReadAsStringAsync();
            Console.WriteLine(Content);
            model_bot_response AccessTokenClass = SerializationClass.Deserialize<model_bot_response>(Content);
            return (AccessTokenClass);
        }
        public MicrosoftTokenManager(String ClientID, String ClientSecret)
        {
            _clientID = ClientID;
            _clientSecret = ClientSecret;
        }
    }
}