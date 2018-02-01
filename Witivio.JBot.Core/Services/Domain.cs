using System;
using System.Linq;

namespace Witivio.JBot.Core.Services
{
    public class Domain
    {
        public static string ExtractFromEmail(string email)
        {
            int i = email.IndexOf('@');
            if (i >= 0)
            {
                return email.Substring(i + 1);
            }
            throw new FormatException("The email parameter is not an email. " + email);
        }

        public static string ExtractEmailFromUCWAUrl(string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            var segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return segments.Last();
        }
    }
}