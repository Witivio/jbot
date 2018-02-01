using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public class MessageDecoder
    {
        private const string PlainHeader = "data:text/plain;charset=utf-8,";
        private const string HtmlHeader = "data:text/html;charset=utf-8,";


        public static string Decode(string message)
        {

            if (message.StartsWith(PlainHeader))
                return DecodePlainText(message);
            if (message.StartsWith(HtmlHeader))
                return DecodeHtmlText(message);
            return null;
        }

        private static string DecodeHtmlText(string message)
        {
            string html = WebUtility.UrlDecode(message.Substring(HtmlHeader.Length));
            string plainText = HtmlHelper.GetPlainTextFromHtml(html);
            return plainText;
        }

        private static string DecodePlainText(string message)
        {
            return WebUtility.UrlDecode(message.Substring(PlainHeader.Length));
        }


        //data:text/html;charset=utf-8,%3cspan+style%3d%22font-size%3a10pt%3bmargin-bottom%3a0pt%3bline-height%3anormal%3b%22%3eyo%3c%2fspan%3e

    }
}
