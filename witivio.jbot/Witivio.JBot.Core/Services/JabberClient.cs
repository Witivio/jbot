using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

using S22.Xmpp.Im;
using System;
using S22.Xmpp.Client;
using Microsoft.Extensions.Configuration;
using System.IO;
using Witivio.JBot.Core.Services.EventArgs;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net.Mime;
using Witivio.JBot.Core.Infrastructure;


using Witivio.JBot.Core.Models.ProActive.Listener;
using Witivio.JBot.Core.Services.Botndo.S4Bot.Core.UCWA;

namespace Witivio.JBot.Core.Services
{
    public interface IJabberClient
    {
        event EventHandler<NewConversationEventArgs> NewMessage;
        event EventHandler<NewConversationEventArgs> NewConversation;
        event EventHandler<StatusEventArgs> StatusChanged;
        event EventHandler<ConversationEventArgs> ConversationEnded;
        event EventHandler<ProActiveConversationEventArgs> ProactiveConversation;
        event Func<Task<Microsoft.Bot.Connector.DirectLine.Conversation>> AskANewConversation;


        Task Start();
        void Stop();
        Task PostAsync(string convId, string message, MessageFormat format = MessageFormat.Text);
        void SetPresence(bool isOnline);
        Task<string> StartNewConversation(ConversationParameters conversationParameters);
        Task<Availability> GetPresence(string email);
        Task IsTypingAsync(string key);
    }

    public class JabberClient : IJabberClient
    {
        public event EventHandler<NewConversationEventArgs> NewMessage;
        public event EventHandler<NewConversationEventArgs> NewConversation;
        public event EventHandler<StatusEventArgs> StatusChanged;
        public event EventHandler<ConversationEventArgs> ConversationEnded;
        public event EventHandler<ProActiveConversationEventArgs> ProactiveConversation;
        public event Func<Task<Microsoft.Bot.Connector.DirectLine.Conversation>> AskANewConversation;


        private ConcurrentDictionary<string, ProactiveConversation> proactiveConversationStack;
        private CancellationTokenSource _stopCancellationToken;
        private XmppServerCredential _credentials;
        private IPersistantDataStore _applicationDataStore;
        private XmppClient _client;
        private EventProcessor _eventProcessor;
        private ConcurrentDictionary<string, MessagingInvitation> _tempIncommingMessageToFireIfMessageDoesNotCome;


        private String _botId;


        private Task _eventTask;
        private IUCWACommunicator _com;

        public JabberClient(IAuth MyCredential, IPersistantDataStore applicationDataStore, String BotId, IUCWACommunicator com)
        {
            _applicationDataStore = applicationDataStore;

            _credentials = JsonConvert.DeserializeObject<XmppServerCredential>(MyCredential.getCredential());
            try
            {
                _client = new XmppClient(_credentials.Host, ConversionClass.stringToInt(_credentials.Port, true), ConversionClass.stringToBool(_credentials.Tls, true))
                {
                    Username = _credentials.User,
                    Password = _credentials.Password,
                };
            }
            catch (Exception e)
            {
                //TODO debug
                Console.WriteLine(e.Message);
                System.Environment.Exit(1);
            }
            _client.StatusChanged += PersistantStatus;
            _client.Message += JBNewMessage;

            _botId = BotId;
            _com = com;
            _eventProcessor = new EventProcessor();
            proactiveConversationStack = new ConcurrentDictionary<string, ProactiveConversation>();
        }

        private async Task<string> findjabberclientid(String mail)
        {
            JidWithConvId res = await _applicationDataStore.GetValueAsync<JidWithConvId>(mail);
            if (res != null && res.Jid != null && mail == res.Jid.ToEmail())
                return (res.convid);
            return ("");
        }

        public async void JBNewMessage(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            String Id = await findjabberclientid(e.Message.To.ToEmail());
            if (Id != "")
            {
                NewMessage?.Invoke(this, new NewConversationEventArgs(new User { Email = e.Message.From.ToEmail(), DisplayName = e.Message.From.Node },
                    new User { Email = e.Message.To.ToEmail(), DisplayName = e.Message.To.Node },
                    e.Message.Body, Id));
            }
            else
            {
                AddContact(e.Message.From.Domain, e.Message.From.Node);
                var directLineConversation = await AskANewConversation?.Invoke();
                await _applicationDataStore.TryAddOrUpdateAsync<JidWithConvId>(e.Message.To.ToEmail(), new JidWithConvId { convid = directLineConversation.ConversationId, Jid = e.Message.To });
                NewConversation?.Invoke(this, new NewConversationEventArgs(new User { Email = e.Message.From.ToEmail(), DisplayName = e.Message.From.Node },
                    new User { Email = e.Message.To.ToEmail(), DisplayName = e.Message.To.Node },
                    e.Message.Body, directLineConversation.ConversationId));
            }
            /*
                ConversationEnded?.Invoke(this, new NewConversationEventArgs(new User { Email = e.Message.From.ToEmail(), DisplayName = e.Message.From.Node },
                    new User { Email = e.Message.To.ToEmail(), DisplayName = e.Message.To.Node },
                    e.Message.Body, directLineConversation.ConversationId));
            */
        }

        private void AddContact(String domain, String node)
        {
            try
            {
                _client.AddContact(new S22.Xmpp.Jid(domain, node));
            }
            catch (Exception e)
            {
                //TODO debug
                return;
            }
        }

        public void PersistantStatus(object sender, StatusEventArgs e)
        {
            if (e.Status.Availability != S22.Xmpp.Im.Availability.Online)
                StatusChanged?.Invoke(this, e);
        }

        public Task Start()
        {
            try
            {
                _client.Connect();
            }
            catch (Exception e)
            {
                //TODO debug
                return (Task.FromResult<object>(null));
            }
            if (_client.Connected == false)
                this.Start();
            SetPresence(true);
            _stopCancellationToken = new CancellationTokenSource();
            //_eventTask = Task.Run(() => ListenEvents(), _stopCancellationToken.Token); // TODO check pour proactivité
            return (Task.FromResult<object>(null));
        }

        private string GetNextUrlDataStoreKey()
        {
            return "eventLink_" + _com.ApplicationId;
        }

        private async Task<EventResponse> GetEventAndSaveNextUrl()
        {
            string key = GetNextUrlDataStoreKey();
            var eventLinkValue = await _applicationDataStore.GetValueAsync<string>(key);

            EventResponse eventResponse = null;
            if (string.IsNullOrWhiteSpace(eventLinkValue))
                eventResponse = await _com.GetEvents(_stopCancellationToken.Token);
            else
                eventResponse = await _com.GetEvents(_stopCancellationToken.Token, eventLinkValue);

            if (eventResponse != null)
            {
                await _applicationDataStore.TryAddOrUpdateAsync(key, eventResponse.Links.Next.Href);
            }
            return eventResponse;
        }

        private async Task SendCallbackResponse(ProactiveConversation messageToSend, ProActiveMessageResult result)
        {
            if (!string.IsNullOrWhiteSpace(messageToSend.Activity.ServiceUrl))
            {
                using (var client = new HttpClient())
                {
                    var response = new ProActiveMessageResponse();
                    response.ActivityId = messageToSend.Activity.Id;
                    response.BotId = _botId;
                    response.Result = result;
                    await client.PostAsync(messageToSend.Activity.ServiceUrl, new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, MediaTypeNames.Application.Json));
                }
            }
        }


        public async Task PostAsync(string ToUser, string message, MessageFormat format = MessageFormat.Text)
        {
            try
            {
                _client.SendMessage(ToUser, message);
            }
            catch (Exception e)
            {
                //TODO debug
            }
        }

        public Task IsTypingAsync(string key)
        {
            //await _communicator.SetIsTyping(S4BConversationId.Format(conversationId));
            throw new System.NotImplementedException();
        }

        public Task<Availability> GetPresence(string jid)
        {
            StatusEventArgs e = new StatusEventArgs(new S22.Xmpp.Jid(jid), null);

            return (Task.FromResult(e.Status.Availability));
        }

        public void SetPresence(bool isOnline)
        {
            if (isOnline)
                _client.SetStatus(S22.Xmpp.Im.Availability.Online);
            else
                _client.SetStatus(S22.Xmpp.Im.Availability.Offline);
        }

        public async Task<string> StartNewConversation(ConversationParameters conversationParameters)
        {
            if (conversationParameters == null) throw new ArgumentNullException(nameof(conversationParameters));
            if (conversationParameters.Members != null)
            {
                if (!conversationParameters.IsGroup.GetValueOrDefault())
                {
                    string to = conversationParameters.Members.First().Id;
                    var presence = await this.GetPresence(to);
                    if (presence == S22.Xmpp.Im.Availability.Online || presence == S22.Xmpp.Im.Availability.ExtendedAway || presence == S22.Xmpp.Im.Availability.Chat)
                    {
                        if (string.IsNullOrEmpty(conversationParameters.Activity.Id))
                            conversationParameters.Activity.Id = Guid.NewGuid().ToString();

                        var directLineConversation = await this.AskANewConversation?.Invoke();
                        proactiveConversationStack.TryAdd(conversationParameters.Activity.Id, new ProactiveConversation { Activity = conversationParameters.Activity, DirectLineConversation = directLineConversation });
                        //await _communicator.StartMessaging(to, conversationParameters.Activity.Id);
                        /*
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
                        */

                        /* JBNewMessage(this, new S22.Xmpp.Im.MessageEventArgs(_client.Jid, message) { }); */
                        // TODO check if good
                        return directLineConversation.ConversationId;
                    }
                }
                throw new NotSupportedException("Group message is not supported yet");
            }
            throw new ArgumentNullException("conversationParameters.Members");
        }


        public void Stop()
        {
            _client.Close();
        }
    }
}