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
using Witivio.JBot.Core.Services.Data;
using System.Diagnostics;
using Witivio.JBot.Core.Configuration;

namespace Witivio.JBot.Core
{
    public interface ICommunicationLinker : IDisposable
    {
        Task StartAsync();
        JBotError GetError();
    }

    public class CommunicationLinker : ErrorProvider, ICommunicationLinker
    {
        public CommunicationLinker(IDirectLineClient directLineClient, IConversationDataStore conversationsStates, IMessageFormater messageFormater,
            IJabberClient jabberClient, IConfiguration config, IScheduler keepALiveScheduler)
        {
            _config = config;
            _directLineClient = directLineClient;
            _jabberClient = jabberClient;
            _conversationsStates = conversationsStates;
            _messageFormater = messageFormater;
            _keepALiveScheduler = keepALiveScheduler;
       }

        private IDirectLineClient _directLineClient;
        private IJabberClient _jabberClient;
        private IConversationDataStore _conversationsStates;
        private IMessageFormater _messageFormater;
        private IConfiguration _config;
        private IScheduler _keepALiveScheduler;

        private readonly int bufSize = 2048;
        private const int TIMER_CHECK_AFK_PEOPLE_IN_MINUTE = 2;

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
                            byte[] bufferbytearray = new byte[bufSize];
                            WebSocketReceiveResult socketresult = await client.ReceiveAsync(new ArraySegment<byte>(bufferbytearray), taskState.CancellationTokenSource.Token);
                            AllPacketComplet = socketresult.EndOfMessage;
                            try
                            {
                                AllPacket += System.Text.Encoding.UTF8.GetString(bufferbytearray);
                            }
                            catch(Exception e)
                            {
                                //TODO debug
                                continue;
                            }
                        }
                        ActivitySet activitySet = Newtonsoft.Json.JsonConvert.DeserializeObject<ActivitySet>(AllPacket);
                        if (activitySet == null)
                            continue;
                       foreach (var activity in activitySet.Activities.Where(m => m.From.Id != conversationState.From))
                        {
                            var typingActivity = activity.AsTypingActivity();
                            if (typingActivity != null)
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

        private async Task SendMessageToDirectLine(NewMessageEventArgs e)
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

                        var directLineKey = _config.Get<string>(ConfigurationKeys.Credentials.DirectLine);
                        var recipientProperty = new ChannelAccount($"{_config.Get<String>(ConfigurationKeys.Credentials.Account)}", $"{_config.Get<String>(ConfigurationKeys.Credentials.Account)}");
                        var channelData = new ChannelAccount(_config.Get<String>(Configuration.ConfigurationKeys.Credentials.BotId), _config.Get<string>(Configuration.ConfigurationKeys.Credentials.BotId));
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

        private async void DeleteConvStore(string convid)
        {
            try
            {
                var state = await _conversationsStates.GetValueAsync<ConversationTaskState>(convid);
                if (state != null)
                {
                    state.CancellationTokenSource.Cancel();

                    var result = await _conversationsStates.TryDeleteAsync(convid);
                }
            }
            catch (Exception exception)
            {
                SetError(exception);
            }
        }

        
        private async void ClientOnMessageReceivedOrNewConversation(object sender, NewMessageEventArgs e)
        {
            String mail;
            ConversationState res = await _conversationsStates.GetValueAsync<ConversationState>(e.From.Email);
            if (res != null && res.Conversation != null && e.From.Email == res.From)
                mail = e.From.Email;
            else
                mail = String.Empty;
            try
            {
                if (string.IsNullOrEmpty(mail))
                {
                    var conversation = await StartNewDirectLineConversationAsync();
                    var conversationState = new ConversationState { Conversation = conversation, From = e.From.Email, Date = e.Date };
                    var taskState = ListenWebSockets(conversation, e.ConversationId, conversationState);
                    await _conversationsStates.TryAddOrUpdateAsync(e.ConversationId, taskState);
                    await SendMessageToDirectLine(e);
                    return;
                }
            }
            catch (Exception exception)
            {
                SetError(exception);
                return;
            }
            res.Date = e.Date;
            await _conversationsStates.TryAddOrUpdateAsync(e.ConversationId, res);
            await SendMessageToDirectLine(e);
        }

        private async Task<Conversation> StartNewDirectLineConversationAsync()
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

        /*
        private async Task PeriodicRunProactive(TimeSpan interval)
        {
            while (true)
            {
               // _jabberClient.CheckingProactivityInQueue();
                await Task.Delay(interval);
            }
        }
        */
        private async Task CheckAfkConversationAndDeleteItAsync()
        {
            var allconv = await _conversationsStates.GetAllAsync<ConversationTaskState>();
            foreach (var currentconv in allconv)
            {
                if (currentconv.Value != null && currentconv.Value.ConversationState != null && currentconv.Value.ConversationState.Date != null && (DateTime.UtcNow - currentconv.Value.ConversationState.Date).Duration() > TimeSpan.FromMinutes(1))
                {
                    Debug.WriteLine("Close conversation " + currentconv.Value.ConversationState.From);
                    DeleteConvStore(currentconv.Value.ConversationState.From);
                }
            }
        }

        private async Task PeriodicCheckAfkTask()
        {
            while (true)
            {
                await CheckAfkConversationAndDeleteItAsync();
            }
        }
        //TODO mettre status demarrage
        //TODO methode httpget send msg to pidgin
        public async Task StartAsync()
        {
            try
            {
                _jabberClient.StatusChanged += PersistantStatus;
                _jabberClient.NewMessageOrNewConversation += ClientOnMessageReceivedOrNewConversation;

                _jabberClient.AskANewConversation += StartNewDirectLineConversationAsync;
                _jabberClient.ProactiveConversation += ProactiveConversation;
                _jabberClient.Start();
                _jabberClient.SetPresence(true);
                _keepALiveScheduler.Start(PeriodicCheckAfkTask, TimeSpan.FromMinutes(TIMER_CHECK_AFK_PEOPLE_IN_MINUTE));
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