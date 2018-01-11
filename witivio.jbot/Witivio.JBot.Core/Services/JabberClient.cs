using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core.Services
{

    public interface IJabberClient
    {
        Task Start();
        void Stop();
        Task PostAsync(string conversationId, string message, MessageFormat format = MessageFormat.Text);
        Task SetPresence(bool isOnline);
        Task<string> StartNewConversation(ConversationParameters conversationParameters);
        Task<UserPresence> GetPresence(string email);
        Task IsTypingAsync(string key);
    }

    public class JabberClient : IJabberClient
    {
        public Task<UserPresence> GetPresence(string email)
        {
            throw new System.NotImplementedException();
        }

        public Task IsTypingAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task PostAsync(string conversationId, string message, MessageFormat format = MessageFormat.Text)
        {
            throw new System.NotImplementedException();
        }

        public Task SetPresence(bool isOnline)
        {
            throw new System.NotImplementedException();
        }

        public Task Start()
        {
            throw new System.NotImplementedException();
        }

        public Task<string> StartNewConversation(ConversationParameters conversationParameters)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}
