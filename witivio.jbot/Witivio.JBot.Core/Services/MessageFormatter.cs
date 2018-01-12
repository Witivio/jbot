using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Connector.DirectLine;
using System.Linq;

namespace Witivio.JBot.Core.Services
{
    public interface IMessageFormater
    {
        string FormatToText(Activity message);
        string FormatToHtml(Activity message);
    }

    public class MessageFormater : IMessageFormater
    {
        public string FormatToText(Activity message)
        {
            if (message.Attachments != null && message.Attachments.Any())
            {
                if (message.AttachmentLayout == "list")
                    return FormatListToText(message);
                else if (message.AttachmentLayout == "carousel")
                    return FormatCarouselToText(message);
            }
            if (message.SuggestedActions != null && message.SuggestedActions.Actions.Any())
            {
                return FormatSuggestedActionsToText(message);
            }
            return message.Text;
        }

        private string FormatSuggestedActionsToText(Activity message)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message.Text);
            int nb = 1;
            foreach (var action in message.SuggestedActions.Actions)
            {
                sb.AppendLine();
                sb.AppendLine(nb + ". " + action.Title + " " + action.Value);
                nb++;
            }
            return sb.ToString();
        }

        private string FormatCarouselToText(Activity message)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var attachment in message.Attachments.Where(a => a.ContentType == "application/vnd.microsoft.card.hero"))
            {
                sb.AppendLine();
                dynamic json = JObject.Parse(attachment.Content.ToString());
                if ((string)json.title != null)
                {
                    sb.AppendLine((string)json.title);
                }
                if ((string)json.subtitle != null)
                {
                    sb.AppendLine((string)json.subtitle);
                }
                if ((string)json.text != null)
                {
                    sb.AppendLine((string)json.text);
                }
                //sb.AppendLine(((JArray)json.images)[0].Value<string>("url"));*
                if (json.buttons != null)
                {
                    foreach (dynamic button in ((JArray)json.buttons))
                    {
                        sb.AppendLine((string)button.title + ": " + (string)button.value);
                    }
                }
            }
            return sb.ToString();

        }

        private string FormatListToText(Activity message)
        {
            var attachment = message.Attachments.FirstOrDefault();
            if (attachment == null)
                return string.Empty;
            dynamic json = JObject.Parse(attachment.Content.ToString());

            StringBuilder sb = new StringBuilder();
            sb.AppendLine((string)json.text);
            int nb = 1;
            foreach (dynamic btn in ((JArray)json.buttons))
            {
                sb.AppendLine(nb + ". " + btn.title);
                nb++;
            }
            return sb.ToString();
        }

        public string FormatToHtml(Activity message)
        {
            if (message.Attachments.Any())
            {
                if (message.AttachmentLayout == "list")
                    return FormatListToText(message);
                else if (message.AttachmentLayout == "carousel")
                    return FormatCarouselToHtml(message);
            }
            return string.Empty;
        }

        private string FormatCarouselToHtml(Activity message)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var attachment in message.Attachments.Where(a => a.ContentType == "application/vnd.microsoft.card.hero"))
            {
                sb.AppendLine("<br>");
                dynamic json = JObject.Parse(attachment.Content.ToString());
                sb.Append($"<span style='font-weight:bold'>{(string)json.title}</span>");
                sb.AppendLine("<br>");
                if ((string)json.subtitle != null)
                {
                    sb.Append($"<span style='font-style:italic'>{(string)json.subtitle}</span>");
                    sb.AppendLine("<br>");
                }
                if ((string)json.text != null)
                {
                    sb.Append((string)json.text);
                    sb.AppendLine("<br>");
                }
                if (json.buttons != null)
                {
                    foreach (dynamic button in ((JArray)json.buttons))
                    {
                        sb.Append((string)button.title + " " + (string)button.value);
                        sb.AppendLine("<br>");
                    }
                }
                //sb.Append(((JArray)json.images)[0].Value<string>("url"));
            }
            return sb.ToString();
        }
    }
}
