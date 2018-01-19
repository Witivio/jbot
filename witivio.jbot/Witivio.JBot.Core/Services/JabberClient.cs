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

        XmppClient _client;

        public JabberClient(XmppServerCredential MyCredential, IPersistantDataStore applicationDataStore)
        {
            _applicationDataStore = applicationDataStore;

            _credentials = MyCredential;
            int port = Int32.Parse(_credentials.Port);
            _client = new XmppClient(_credentials.Host, port, true);
            _client.Username = _credentials.User;
            _client.Password = _credentials.Password;
            _client.StatusChanged += PersistantStatus;
            _client.Message += JBNewMessage;
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
            _client.AddContact(new S22.Xmpp.Jid(domain, node));
        }

        public void PersistantStatus(object sender, StatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        public Task<Availability> GetPresence(string jid)
        {
            StatusEventArgs e = new StatusEventArgs(new S22.Xmpp.Jid(jid), null);
            
            return (Task.FromResult(e.Status.Availability));
        }

        public Task IsTypingAsync(string key)
        {
            //await _communicator.SetIsTyping(S4BConversationId.Format(conversationId));
            throw new System.NotImplementedException();
        }

        public void SetPresence(bool isOnline)
        {
            if (isOnline)
                _client.SetStatus(S22.Xmpp.Im.Availability.Online);
            else
                _client.SetStatus(S22.Xmpp.Im.Availability.Offline);
        }

        public Task Start()
        {
            _client.Connect();
            if (_client.Connected == false)
                this.Start();
            SetPresence(true);
            //_eventTask = Task.Run(() => ListenEvents(), _stopCancellationToken.Token);
            _stopCancellationToken = new CancellationTokenSource();
            return (Task.FromResult<object>(null));
        }

        public async Task<string> StartNewConversation(ConversationParameters conversationParameters)
        {
            if (conversationParameters == null) throw new ArgumentNullException(nameof(conversationParameters));
            if (conversationParameters.Members != null)
            {
                if (!conversationParameters.IsGroup.GetValueOrDefault())
                {
                    string to = conversationParameters.Members.First().Id;

                    var presence = await this.GetPresence("");
                    if (presence == S22.Xmpp.Im.Availability.Online || presence == S22.Xmpp.Im.Availability.ExtendedAway || presence == S22.Xmpp.Im.Availability.Chat)
                    {
                        if (string.IsNullOrEmpty(conversationParameters.Activity.Id))
                            conversationParameters.Activity.Id = Guid.NewGuid().ToString();

                        var directLineConversation = await this.AskANewConversation?.Invoke();
                        proactiveConversationStack.TryAdd(conversationParameters.Activity.Id, new ProactiveConversation { Activity = conversationParameters.Activity, DirectLineConversation = directLineConversation });
                        //start message await _client.StartMessaging(to, conversationParameters.Activity.Id);
                        return directLineConversation.ConversationId;
                    }
                }
                throw new NotSupportedException("Group message is not supported yet");
            }
            throw new ArgumentNullException("conversationParameters.Members");
        }

        public async Task PostAsync(string ToUser, string message, MessageFormat format = MessageFormat.Text)
        {
            _client.SendMessage(ToUser, message);
        }

        public void Stop()
        {
            _client.Close();
        }
    }
}