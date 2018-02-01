using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

using S22.Xmpp.Im;
using System;
using S22.Xmpp.Client;
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
//using Witivio.JBot.Core.Services.Botndo.S4Bot.Core.UCWA;
using Witivio.JBot.Core.Services.HttpManager;
using System.Diagnostics;
using Witivio.JBot.Core.Configuration;
using Witivio.JBot.Core.Services.Configuration;

namespace Witivio.JBot.Core.Services
{
    public interface IJabberClient
    {
        event EventHandler<NewMessageEventArgs> NewMessageOrNewConversation;
        event EventHandler<StatusEventArgs> StatusChanged;
        //event EventHandler<ConversationEventArgs> ConversationEnded;
        event EventHandler<ProActiveConversationEventArgs> ProactiveConversation;
        event Func<Task<Microsoft.Bot.Connector.DirectLine.Conversation>> AskANewConversation;


        //void CheckingProactivityInQueue();
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
        public event EventHandler<NewMessageEventArgs> NewMessageOrNewConversation;
        public event EventHandler<StatusEventArgs> StatusChanged;
        //public event EventHandler<ConversationEventArgs> ConversationEnded;
        public event EventHandler<ProActiveConversationEventArgs> ProactiveConversation;
        public event Func<Task<Microsoft.Bot.Connector.DirectLine.Conversation>> AskANewConversation;


        private IConfiguration _conf;
        private ConcurrentDictionary<string, ProactiveConversation> proactiveConversationStack;
        private CancellationTokenSource _stopCancellationToken;
        private XmppServerCredential _credentials;
        private XmppClient _client;
        private IProactiveRequest _proactiveRequest;



        public JabberClient(IXmppProvider xmppProvider, IConfiguration config, IProactiveRequest proactiveRequest)
        {
            _conf = config;
            _credentials = xmppProvider.GetLog();
            try
            {
                bool resbool;
                int resint;
                Boolean.TryParse(_credentials.Tls, out resbool);
                Int32.TryParse(_credentials.Port, out resint);
                _client = new XmppClient(_credentials.Host, resint, resbool)
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

            _client.ActivityChanged += _client_ActivityChanged;
            _client.ChatStateChanged += _client_ChatStateChanged;
            _client.StatusChanged += PersistantStatus;
            _client.Message += OnNewMessageReceived;

            _proactiveRequest = proactiveRequest;
            proactiveConversationStack = new ConcurrentDictionary<string, ProactiveConversation>();
        }

        private void _client_ChatStateChanged(object sender, S22.Xmpp.Extensions.ChatStateChangedEventArgs e)
        {
            Debug.WriteLine("State: " + e.ChatState + " " +  e.Jid.GetBareJid() + " " + DateTime.UtcNow);

            //_applicationDataStore.TryAddOrUpdateAsync<JidWithConvId>(e.Jid.ToEmail(), new JidWithConvId
            //{
            //    convid = e.Jid.ToEmail(),
            //    Jid = e.Jid,
            //    date = DateTime.UtcNow
            //});
        }

        private void _client_ActivityChanged(object sender, S22.Xmpp.Extensions.ActivityChangedEventArgs e)
        {
            _client.Im.ApproveSubscriptionRequest(e.Jid);
            _client.Im.RequestSubscription(e.Jid);
        }

        public void OnNewMessageReceived(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            _client.Im.ApproveSubscriptionRequest(e.Jid);
            _client.Im.RequestSubscription(e.Jid);
            NewMessageOrNewConversation?.Invoke(this, new NewMessageEventArgs
            {
                From = new User
                {
                    Email = e.Message.From.ToEmail(),
                    DisplayName = e.Message.From.Node
                },
                ConversationId = e.Message.To.ToEmail(),
                Date = DateTime.UtcNow,
                Message = e.Message.Body
            });
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
        // TODO PROACTIVITY
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