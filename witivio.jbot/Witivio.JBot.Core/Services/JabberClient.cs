using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

using S22.Xmpp.Im;
using System;
using S22.Xmpp.Client;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace Witivio.JBot.Core.Services
{
    public interface IJabberClient
    {
        void NewMessage(object sender, MessageEventArgs e);
        void PersistantStatus(object sender, StatusEventArgs e);

        Task Start();
        void Stop();
        void PostAsync(MessageEventArgs MEA);
        Task PostAsync(string JId, string message, MessageFormat format = MessageFormat.Text);
        void SetPresence(bool isOnline);
        Task<string> StartNewConversation(ConversationParameters conversationParameters);
        Task<Availability> GetPresence(string email);
        Task IsTypingAsync(string key);
    }

    public class JabberClient : IJabberClient
    {
        private XmppServerCredential _credentials;
        XmppClient _client;

        public JabberClient(XmppServerCredential MyCredential)
        {
            _credentials = MyCredential;
            int port = Int32.Parse(_credentials.Port);
            _client = new XmppClient(_credentials.Host, port, true);
            _client.Username = _credentials.User;
            _client.Password = _credentials.Password;
        }

        public void NewMessage(object sender, MessageEventArgs e)
        {
            Message ToSendMsg = new Message(e.Jid, "Le robot vous renvoie votre texte: " + e.Message.Body);
            _client.AddContact(e.Jid.Node + "@" + e.Jid.Domain);
            MessageEventArgs newmessage = new MessageEventArgs(e.Jid, ToSendMsg);
            this.PostAsync(newmessage);
            // TDC : Passe par des event pour faire remonter l'information dans le communication linker.
            // inspire toi de l'existant sur le S4B
            // ne t'embete pas a recuperer des Tokens le directlineclient le fait pour toi dans le communication linker.
        }

        public void PersistantStatus(object sender, StatusEventArgs e)
        {
            _client.SetStatus(S22.Xmpp.Im.Availability.Chat);
        }

        public Task<Availability> GetPresence(string jid)
        {
            StatusEventArgs e = new StatusEventArgs(jid, null);
            
            return (Task.FromResult(e.Status.Availability));
        }

        public Task IsTypingAsync(string key)
        {
            throw new System.NotImplementedException();
        }


        public void PostAsync(MessageEventArgs MEA)
        {
            _client.Buzz(MEA.Jid);
            _client.SendMessage(MEA.Jid, MEA.Message.Body);

           //new MicrosoftTokenManager().CheckRefreshToken("ffa9b918-3a31-4760-8e22-dda9b42845e9", "nrpECVA81ifoiYAS352}|_)");
        }


        public Task PostAsync(string JID, string message, MessageFormat format = MessageFormat.Text)
        {
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
            _client.SetAvatar("../../../../Resource/bot.jpg");
            _client.SetStatus(S22.Xmpp.Im.Availability.Chat);
            _client.Message += NewMessage;
            _client.StatusChanged += PersistantStatus;
            Console.WriteLine("Connected as " + _client.Jid + Environment.NewLine);
            return (Task.FromResult<object>(null));
        }

        public Task<string> StartNewConversation(ConversationParameters conversationParameters)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            _client.Close();
        }
    }
}

/*
        //event EventHandler<MessageEventArgs> NewMessage;
    NewMessage?.Invoke(this, new MessageEventArgs
    {
        ConversationId = S4BConversationId.Extract(incommingMessage.ConversationId),
        Message = incommingMessage.Message,
        From = incommingMessage.From
    });
    _client.StatusChanged += PersistantStatus;
    //PersistantStatus;
*/
