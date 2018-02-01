using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Witivio.JBot.Core.Services
{
    public class HtmlHelper
    {
        public static string GetPlainTextFromHtml(string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = htmlString.Replace("&nbsp;", " ");

            return htmlString;
        }
    }
}
