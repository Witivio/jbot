using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public static class ConversionClass
    {
        public static int stringToInt(String toConvert, bool exitProgramm = false)
        {
            int number;

            bool result = Int32.TryParse(toConvert, out number);
            if (result)
                return (number);
            else
            {
                Debug.WriteLine("Error convert String to Int.");
                if (exitProgramm == true)
                    System.Environment.Exit(1);
            }
            return (-1);
        }

        public static bool stringToBool(String toConvert, bool exitProgramm = false)
        {

            bool resbool;

            bool result = Boolean.TryParse(toConvert, out resbool);
            if (result)
                return (resbool);
            else
            {
                Debug.WriteLine("Error convert String to Bool.");
                if (exitProgramm == true)
                    System.Environment.Exit(1);
            }
            return (false);
        }
    }
}
