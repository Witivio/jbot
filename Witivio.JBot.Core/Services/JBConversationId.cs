using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public class JBConversationId
    {
        public static string Extract(string url)
        {
            string strRegex = @"/ucwa/oauth/v1/applications/(\w+)/communication/conversations/(\w+-\w+-\w+-\w+-\w+)";
            Regex myRegex = new Regex(strRegex, RegexOptions.Singleline);
            foreach (Match myMatch in myRegex.Matches(url))
            {
                if (myMatch.Success)
                {
                    return myMatch.Groups[1].Value + "_" + myMatch.Groups[2].Value;
                }
            }
            throw new FormatException("The url is not in correct format: " + url);
        }

        public static string Format(string id)
        {
            string strRegex = @"(\w+)_(\w+-\w+-\w+-\w+-\w+)";
            Regex myRegex = new Regex(strRegex, RegexOptions.Singleline);
            var items = myRegex.Split(id);

            return $"/ucwa/oauth/v1/applications/{items[1]}/communication/conversations/{items[2]}";
        }
    }
}
