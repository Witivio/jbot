using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;
using Witivio.JBot.Core.Services;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.ExceptionManager;

namespace Witivio.JBot.Core.Proactivity
{
    public partial class ConversationResourceResponse
    {
        public ConversationResourceResponse() { }

        public ConversationResourceResponse(string activityId = default(string), string id = default(string))
        {
            ActivityId = activityId;
            Id = id;
        }

        [JsonProperty(PropertyName = "activityId")]
        public string ActivityId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

    }

    public class UserPresenceResponse
    {
        [JsonProperty("presence")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserPresence Presence { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }

    public class ConversationController : Controller
    {
        private readonly IJabberClient _jbClient;

        public ConversationController(IJabberClient jbClient)
        {
            _jbClient = jbClient;
        }

        [Route("/v3/conversations")]
        [HttpPost]
        public async Task<IActionResult> Conversations([FromBody] ConversationParameters conversationParameters)
        {
            try
            {
                var conversationId = await _jbClient.StartNewConversation(conversationParameters);
                return Ok(new ConversationResourceResponse { ActivityId = conversationParameters.Activity.Id, Id = conversationId });
            }   
            catch (StartNewConversationException ex)
            {
                if (ex.Presence == UserPresence.None)
                    return NotFound();
                else
                    return base.StatusCode(StatusCodes.Status451UnavailableForLegalReasons, ex.Presence.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [Route("/v3/conversations/{userEmail}/{userMessage}")]
        [HttpGet]
        public async Task<IActionResult> ConversationsGet(string userEmail, string userMessage)
        {
            UserPresence presence = await _jbClient.GetPresenceAsync(userEmail);
            if (presence == UserPresence.Online || presence == UserPresence.Chat)
            {
                await _jbClient.PostAsync(userEmail, userMessage, MessageFormat.Text);
                return Ok();
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }


        [Route("/v3/presence/{userEmail}")]
        [HttpGet]
        public async Task<IActionResult> GetPresence(string userEmail)
        {
            bool isEmail = Regex.IsMatch(userEmail, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

            if (!isEmail)
                return BadRequest();

            try
            {
                UserPresenceResponse response = new UserPresenceResponse();
                response.Presence = await _jbClient.GetPresenceAsync(userEmail);
                response.User = userEmail;
                return Ok(response);
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}