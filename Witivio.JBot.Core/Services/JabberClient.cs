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
using Witivio.JBot.Core.HttpManager;
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
        Task<UserPresence> GetPresenceAsync(string email);
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
        private IScheduler _keepALiveScheduler;
        private IPersistantDataStore _userContactStatus;
        private ConcurrentDictionary<string, ProactiveConversation> proactiveConversationStack;
        private CancellationTokenSource _stopCancellationToken;
        private XmppServerCredential _credentials;
        private XmppClient _client;
        private IProactiveRequest _proactiveRequest;
        private const int TIMER_CHECK_USER_TO_VIEW_STATUS_IN_ROSTER = 3;

        public JabberClient(IXmppProvider xmppProvider, IConfiguration config, IProactiveRequest proactiveRequest, IScheduler keepALiveScheduler, IPersistantDataStore userContactSatus)
        {
            _conf = config;
            _credentials = xmppProvider.GetLog();
            _userContactStatus = userContactSatus;
            _keepALiveScheduler = keepALiveScheduler;
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
            _client.ChatStateChanged += _client_ChatStateChanged;
            _client.StatusChanged += PersistantStatus;
            _client.Message += OnNewMessageReceived;
            /*
            _client.SubscriptionApproved += OnNewAccept;
            _client.SubscriptionRefused += OnNewRefuse;
            */
            _proactiveRequest = proactiveRequest;
            proactiveConversationStack = new ConcurrentDictionary<string, ProactiveConversation>();
        }
        /*
        private void OnNewRoster(object sender, RosterUpdatedEventArgs e)
        {
            if (e.Item.Jid.ToEmail() == "test3@tlsc.fr")
                Debug.WriteLine("New contact");
        }
        private void OnNewAccept(object sender, SubscriptionApprovedEventArgs e)
        {
            if (e.Jid.ToEmail() == "test3@tlsc.fr")
                Debug.WriteLine("New contact");
        }
        private void OnNewRefuse(object sender, SubscriptionRefusedEventArgs e)
        {
            if (e.Jid.ToEmail() == "test3@tlsc.fr")
                Debug.WriteLine("New contact");
        }
        */


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

        private Task ApprovedContactToWatchStatusTask()
        {
            while (true)
            {
                try
                {
                    Roster Contacts = _client.GetRoster();
                    foreach (RosterItem currentitem in Contacts)
                    {
                        _client.Im.ApproveSubscriptionRequest(currentitem.Jid);
                        _client.Im.RequestSubscription(currentitem.Jid);
                    }
                }
                catch(Exception e)
                { }
            }
        }

        public void OnNewMessageReceived(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            AddContact(e.Message.From.Node, e.Message.From.Domain);
            _client.Im.RequestSubscription(e.Message.From);
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
            if (e.Jid.ToEmail() == _conf.Get<string>(ConfigurationKeys.Credentials.Account))
            {
                if (e.Status.Availability != S22.Xmpp.Im.Availability.Online)
                    StatusChanged?.Invoke(this, e);
            }
            else
            {
                _userContactStatus.TryAddOrUpdateAsync(e.Jid.ToEmail() + "UserStatus", new UserPresenceClass
                {
                    userPresence = (UserPresence)e.Status.Availability
                });
            }
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
            _keepALiveScheduler.Start(ApprovedContactToWatchStatusTask, TimeSpan.FromSeconds(TIMER_CHECK_USER_TO_VIEW_STATUS_IN_ROSTER));

            return (Task.FromResult<object>(null));
        }

        public async Task PostAsync(string ToUser, string message, MessageFormat format = MessageFormat.Text)
        {
            try
            {
                if (!String.IsNullOrEmpty(message))
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

        public async Task<UserPresence> GetPresenceAsync(string jidAccount)
        {
            UserPresenceClass res = await _userContactStatus.GetValueAsync<UserPresenceClass>(jidAccount + "UserStatus");
            if (res != null)
                return (res.userPresence);
            else
                return (UserPresence.None);
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
                    var presence = await this.GetPresenceAsync(to);
                    if (presence == UserPresence.Online || presence == UserPresence.ExtendedAway || presence == UserPresence.Away || presence == UserPresence.Chat)
                    {
                        if (string.IsNullOrEmpty(conversationParameters.Activity.Id))
                            conversationParameters.Activity.Id = Guid.NewGuid().ToString();

                        var directLineConversation = await this.AskANewConversation?.Invoke();
                        proactiveConversationStack.TryAdd(conversationParameters.Activity.Id, new ProactiveConversation { Activity = conversationParameters.Activity, DirectLineConversation = directLineConversation });
                        return directLineConversation.ConversationId;
                    }
                    else
                        throw new NotSupportedException("User is not online");
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