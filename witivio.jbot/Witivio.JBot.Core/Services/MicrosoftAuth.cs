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
    public class MicrosoftInfo
    {
        public String Email { get; set; }
        public String code { get; set; }
    }

    public class MicrosoftToken
    {
        public String access_token { get; set; }
        public String token_type { get; set; }
        public String expires_in { get; set; }
        public String expires_on { get; set; }
        public String resource { get; set; }
        public String refresh_token { get; set; }
        public String scope { get; set; }
        public String id_token { get; set; }
    }

    static class GenericRequest
    {
        static public HttpResponseMessage PostMethod(List<KeyValuePair<String, String>> values, String url)
        {
            using (HttpClient webClient = new HttpClient())
            {
                webClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                webClient.DefaultRequestHeaders.TryAddWithoutValidation("Host", "https://login.microsoftonline.com");

                FormUrlEncodedContent content = new FormUrlEncodedContent(values);

                return (webClient.PostAsync(url, content).Result);
            }
        }
    }

    class MicrosoftTokenManager
    {
        //private static async Task<String> CheckRefreshToken(String ClientID, String ClientSecret)
        public async Task<String> CheckRefreshToken(String ClientID, String ClientSecret)
        {
                List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("client_id", ClientID));
                values.Add(new KeyValuePair<string, string>("scope", "https://api.botframework.com/.default"));
                values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                values.Add(new KeyValuePair<string, string>("client_secret", ClientSecret));

                HttpResponseMessage Message = GenericRequest.PostMethod(values, "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token");
                String Content = await Message.Content.ReadAsStringAsync();
            return ("");
        }
    }
}