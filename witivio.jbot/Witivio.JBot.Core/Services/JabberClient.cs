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
        String _host;
        String _user;
        String _password;
        String _port;
        XmppClient _client;

        public JabberClient(ServerModelCredential MyCredential)
        {
            _host = MyCredential.Host;
            _user = MyCredential.User;
            _password = MyCredential.Password;
            _port = MyCredential.Port;
            int port = Int32.Parse(_port);
            _client = new XmppClient(_host, port, true);
            _client.Username = _user;
            _client.Password = _password;
        }

        public void NewMessage(object sender, MessageEventArgs e)
        {
            Message ToSendMsg = new Message(e.Jid, "Le robot vous renvoie votre texte: " + e.Message.Body);
            _client.AddContact(e.Jid.Node + "@" + e.Jid.Domain);
            MessageEventArgs newmessage = new MessageEventArgs(e.Jid, ToSendMsg);
            this.PostAsync(newmessage);
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

            MicrosoftTokenManager TokenManage = new MicrosoftTokenManager("ffa9b918-3a31-4760-8e22-dda9b42845e9", "nrpECVA81ifoiYAS352}|_)");
            Console.WriteLine("Access Token: " + TokenManage.GetAccessToken().Result.access_token);
            Console.WriteLine("TestRequest: " + TokenManage.SendPostWithAuthotization().Result);
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
