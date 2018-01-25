using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witivio.JBot.Core.Services
{
    public static class ConversionClass
    {
        public static int stringToInt(String toConvert, bool exitProgramm = false)
        {
            try
            {
                return (Int32.Parse(toConvert));
            }
            catch (Exception e)
            {
                //TODO debug
                Console.WriteLine(e.Message);
                if (exitProgramm == true)
                    System.Environment.Exit(1);
            }
            return (-1);
        }

        public static bool stringToBool(String toConvert, bool exitProgramm = false)
        {
            try
            {
                return (Convert.ToBoolean(toConvert));
            }
            catch (Exception e)
            {
                //TODO debug
                Console.WriteLine(e.Message);
                if (exitProgramm == true)
                    System.Environment.Exit(1);
            }
            return (false);
        }
    }
}
