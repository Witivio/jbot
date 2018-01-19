using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

using S22.Xmpp.Im;
using System.Linq;
using System.Threading;
using System.Net.WebSockets;
using Witivio.JBot.Core.Services.EventArgs;

namespace Witivio.JBot.Core
{
    public interface ICommunicationLinker : IDisposable
    {
        void Start();
        JBotError GetError();
    }

    public class CommunicationLinker : ErrorProvider, ICommunicationLinker
    {
        public CommunicationLinker(IDirectLineClient directLineClient, IConversationDataStore conversationsStates, IMessageFormater messageFormater, JabberClient jabberClient, String BoteId)
        {
            _boteId = BoteId;
            _directLineClient = directLineClient;
            _jabberClient = jabberClient;
            _conversationsStates = conversationsStates;
            _messageFormater = messageFormater;
        }

        private IDirectLineClient _directLineClient { get; }
        private IJabberClient _jabberClient { get; }
        private IConversationDataStore _conversationsStates { get; set; }
        private IMessageFormater _messageFormater { get; set; }

        private String _boteId { get; }

        private ConversationTaskState ListenWebSockets(Conversation conversation, string JBOTConversationId, ConversationState conversationState)
        {
            ConversationTaskState taskState = new ConversationTaskState { ConversationState = conversationState };
            taskState.CancellationTokenSource = new CancellationTokenSource();

            taskState.Task = Task.Run(async () =>
            {
                System.Net.WebSockets.ClientWebSocket client = new System.Net.WebSockets.ClientWebSocket();
                try
                {
                    await client.ConnectAsync(new Uri(conversation.StreamUrl), taskState.CancellationTokenSource.Token);

                    while (!taskState.CancellationTokenSource.IsCancellationRequested)
                    {
                        string AllPacket = "";
                        bool AllPacketComplet = false;
                        while (AllPacketComplet != true)
                        {
                            byte[] bufferbytearray = new byte[2048];
                            WebSocketReceiveResult socketresult = await client.ReceiveAsync(new ArraySegment<byte>(bufferbytearray), taskState.CancellationTokenSource.Token);
                            AllPacketComplet = socketresult.EndOfMessage;
                            AllPacket += System.Text.Encoding.Default.GetString(bufferbytearray);
                        }
                        ActivitySet activitySet = Newtonsoft.Json.JsonConvert.DeserializeObject<ActivitySet>(AllPacket);
                        if (activitySet == null)
                            continue;
                       foreach (var activity in activitySet.Activities.Where(m => m.From.Id != conversationState.From))
                        {
                            var typingActivity = activity.AsTypingActivity();
                            if (typingActivity != null)
                                // on continue car on ne peux pas set le "isTyping sur jabber"
                                continue;
                            if (!string.IsNullOrWhiteSpace(activity.Text))
                                await _jabberClient.PostAsync(conversationState.From, _messageFormater.FormatToText(activity));
                            else
                                await _jabberClient.PostAsync(conversationState.From, _messageFormater.FormatToText(activity), MessageFormat.Text);
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    SetError(ex);
                }
                finally
                {
                    client?.Dispose();
                }

            }, taskState.CancellationTokenSource.Token);

            return taskState;
        }

        private async Task SendMessageToDirectLine(NewConversationEventArgs e)
        {
            try
            {
                var botConversationState = await _conversationsStates.GetValueAsync<ConversationTaskState>(e.ConversationId);
                if (botConversationState != null)
                {
                    if (botConversationState.ConversationState.Conversation == null)
                        return;
                    try
                    {
                        var fromProperty = new ChannelAccount(e.From.Email, e.From.DisplayName);
                        var recipientProperty = new ChannelAccount($"sip:{e.Bot.Email}", $"sip:{e.Bot.Email}");
                        var channelData = new ChannelAccount(_boteId, _boteId);
                        var activity = new Microsoft.Bot.Connector.DirectLine.Activity(text: e.Message, fromProperty: fromProperty, type: ActivityTypes.Message, recipient: recipientProperty, channelData: channelData);
                        await _directLineClient.Conversations.PostActivityAsync(botConversationState.ConversationState.Conversation.ConversationId, activity);
                    }
                    catch (Exception exception)
                    {
                        SetError(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                SetError(exception);
            }
        }

        private async void ConversationEnded(object sender, ConversationEventArgs e)
        {
            try
            {
                var state = await _conversationsStates.GetValueAsync<ConversationTaskState>(e.ConversationId);
                if (state != null)
                {
                    state.CancellationTokenSource.Cancel();

                    var result = await _conversationsStates.TryDeleteAsync(e.ConversationId);
                }
            }
            catch (Exception exception)
            {
                SetError(exception);
            }
        }

        private async void ClientOnNewConversation(object sender, NewConversationEventArgs e)
        {
            try
            {
                var conversation = await StartNewDirectLineConversation();
                var conversationState = new ConversationState { Conversation = conversation, From = e.From.Email};
                var taskState = ListenWebSockets(conversation, e.ConversationId, conversationState);
                await _conversationsStates.TryAddOrUpdateAsync(e.ConversationId, taskState);

            }
            catch (Exception exception)
            {
                SetError(exception);
            }
            await SendMessageToDirectLine(e);
        }


        private async void ClientOnMessageReceived(object sender, NewConversationEventArgs e)
        {
            await SendMessageToDirectLine(e);
        }

        private async Task<Conversation> StartNewDirectLineConversation()
        {
            var conversation = await _directLineClient.Conversations.StartConversationAsync();
            return conversation;
        }

        private void PersistantStatus(object sender, StatusEventArgs e)
        {
            _jabberClient.SetPresence(true);
        }

        private async void ProactiveConversation(object sender, ProActiveConversationEventArgs e)
        {
            try
            {
                var conversationState = new ConversationState { Conversation = e.DirecLineConversation, From = e.From.Email, SupportedFormat = MessageFormat.Text };
                var taskState = ListenWebSockets(e.DirecLineConversation, e.ConversationId, conversationState);
                var result = await _conversationsStates.TryAddOrUpdateAsync(e.ConversationId, taskState);
            }
            catch (Exception exception)
            {
                SetError(exception);
            }
        }

        public void Start()
        {
            try
            {
                _jabberClient.StatusChanged += PersistantStatus;
                _jabberClient.NewMessage += ClientOnMessageReceived;
                _jabberClient.NewConversation += ClientOnNewConversation;
                _jabberClient.ConversationEnded += ConversationEnded;
                _jabberClient.AskANewConversation += StartNewDirectLineConversation;
                _jabberClient.ProactiveConversation += ProactiveConversation;

                _jabberClient.Start();
                _jabberClient.SetPresence(true);
            }
            catch (Exception e)
            {
                SetError(e);
            }
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
