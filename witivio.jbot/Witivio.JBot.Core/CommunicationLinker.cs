using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core
{
    public interface ICommunicationLinker : IDisposable
    {
        void Start();
        JBotError GetError();
    }

    public class CommunicationLinker : ErrorProvider, ICommunicationLinker
    {
        public CommunicationLinker(IDirectLineClient directLineClient, JabberClient jabberClient)
        {
            _directLineClient = directLineClient;
            _jabberClient = jabberClient;
        }

        private IDirectLineClient _directLineClient { get; }
        private IJabberClient _jabberClient { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            try
            {
                //_jabberClient.NewConversation += ClientOnNewConversation;
                //_jabberClient.NewMessage += ClientOnMessageReceived;
                //_jabberClient.ConversationEnded += ConversationEnded;
                //_jabberClient.ProactiveConversation += ProactiveConversation;
                //_jabberClient.AskANewConversation += StartNewDirectLineConversation;

                _jabberClient.Start();
                _jabberClient.SetPresence(true);

            }
            catch (Exception e)
            {
                SetError(e);
            }
        }
    }
}
