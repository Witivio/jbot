using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    using System;
    using System.Collections.Generic;

    using Witivio.JBot.Core.Models;
    using Witivio.JBot.Core.Models.ProActive.Listener;
    using Witivio.JBot.Core.Services.EventArgs;

    namespace Botndo.S4Bot.Core.UCWA
    {
        //public enum InvitationState
        //{
        //    Cancelled,
        //    Connected,
        //    Connecting,
        //    Declined,
        //    Alerting,
        //    Failed,
        //    Forwarded
        //}

        //public class MessagingInvitationWithState
        //{

        //    prop
        //    public MessagingInvitation Invitation { get; set; }

        //}

        public class EventProcessor
        {
            public List<IncommingMessage> IncommingIM(EventResponse eventResponse)
            {
                List<IncommingMessage> messages = new List<IncommingMessage>();
                foreach (var sender in eventResponse.Sender)
                {
                    if (sender.Rel != "conversation")
                        continue;
                    foreach (var @event in sender.Events)
                    {
                        if (@event.Status == "Success")
                        {
                            if (@event.Embedded.Message != null && @event.Type == "completed" && @event.Embedded.Message.Direction == "Incoming")
                            {
                                string fromEmail = Domain.ExtractEmailFromUCWAUrl(@event.Embedded.Message.Links.Contact.Href);
                                string fromName = @event.Embedded.Message.Links.Participant.Title;
                                if (@event.Embedded.Message.Links.PlainMessage != null)
                                {

                                    messages.Add(new IncommingMessage
                                    {
                                        ConversationId = sender.Href,
                                        Message = MessageDecoder.Decode(@event.Embedded.Message.Links.PlainMessage.Href),
                                        From = new User
                                        {
                                            Email = fromEmail,
                                            DisplayName = fromName
                                        },
                                        Format = MessageFormat.Text
                                    });
                                }
                                else if (@event.Embedded.Message.Links.HtmlMessage != null)
                                {
                                    messages.Add(new IncommingMessage
                                    {
                                        ConversationId = sender.Href,
                                        Message = MessageDecoder.Decode(@event.Embedded.Message.Links.HtmlMessage.Href),
                                        From = new User
                                        {
                                            Email = fromEmail,
                                            DisplayName = fromName
                                        },
                                        Format = MessageFormat.Html
                                    });
                                }
                            }
                        }
                    }
                }
                return messages;
            }


            public List<MessagingInvitation> IncommingInvitation(EventResponse eventResponse)
            {
                List<MessagingInvitation> invitations = new List<MessagingInvitation>();
                foreach (var sender in eventResponse.Sender)
                {
                    if (sender.Rel != "communication")
                        continue;
                    foreach (var @event in sender.Events)
                    {
                        if (@event.Embedded != null && (@event.Embedded.MessagingInvitation != null && (@event.Embedded.MessagingInvitation.Direction == "Incoming" &&
                                                                                                        @event.Embedded.MessagingInvitation.State == "Connecting")))
                        {
                            if (@event.Embedded.MessagingInvitation.Links.Accept != null)
                            {
                                @event.Embedded.MessagingInvitation.Message = MessageDecoder.Decode(@event.Embedded.MessagingInvitation.Links.Message.Href);
                                invitations.Add(@event.Embedded.MessagingInvitation);
                            }
                        }
                    }
                }
                return invitations;
            }

            public List<MessagingInvitation> WaitingOutgoingInvitation(EventResponse eventResponse)
            {
                List<MessagingInvitation> invitations = new List<MessagingInvitation>();
                foreach (var sender in eventResponse.Sender)
                {
                    if (sender.Rel != "communication")
                        continue;
                    foreach (var @event in sender.Events)
                    {
                        if (@event.Embedded != null && (@event.Embedded.MessagingInvitation != null && (@event.Embedded.MessagingInvitation.Direction == "Outgoing" &&
                                                                                                        @event.Embedded.MessagingInvitation.State == "Connecting")))
                        {

                            invitations.Add(@event.Embedded.MessagingInvitation);
                        }
                    }
                }
                return invitations;
            }

            public List<MessagingInvitation> OutgoingInvitation(EventResponse eventResponse)
            {
                List<MessagingInvitation> invitations = new List<MessagingInvitation>();
                foreach (var sender in eventResponse.Sender)
                {
                    if (sender.Rel != "communication")
                        continue;
                    foreach (var @event in sender.Events)
                    {
                        if (@event.Embedded != null && (@event.Embedded.MessagingInvitation != null && (@event.Embedded.MessagingInvitation.Direction == "Outgoing")))// &&
                                                                                                                                                                      // @event.Embedded.MessagingInvitation.State == "Connected")))
                        {

                            invitations.Add(@event.Embedded.MessagingInvitation);
                        }
                    }
                }
                return invitations;
            }

            internal void ParticipantAdded(EventResponse eventResponse)
            {
                foreach (var sender in eventResponse.Sender)
                {
                    if (sender.Rel != "conversation")
                        continue;
                    foreach (var @event in sender.Events)
                    {
                        if (@event.Link != null)
                        {
                            if (@event.Link.Rel == "participant" && @event.Type == "added")
                            {
                                int b = 5;
                            }
                        }
                    }
                }
            }

            internal List<string> ConversationEnded(EventResponse eventResponse)
            {
                List<string> conversations = new List<string>();
                foreach (var sender in eventResponse.Sender)
                {
                    if (sender.Rel != "conversation")
                        continue;
                    foreach (var @event in sender.Events)
                    {
                        if ((@event.Embedded != null && (@event.Embedded.Messaging != null && @event.Embedded.Messaging.State == "Disconnected"))
                            && @event.Reason != null && @event.Reason.Subcode == "Ended")
                        {

                            conversations.Add(@event.Link.Href);
                        }
                    }
                }
                return conversations;
            }
        }
    }
}
