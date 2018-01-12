using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace witivio.jbot.S22Xmpp.Pilot
{
    public static class SerializationClass
    {
        public static String Serialize<T>(T ToSerialize)
        {
            string Serialized = JsonConvert.SerializeObject(ToSerialize);
            return (Serialized);
        }

        public static T Deserialize<T>(String ToDeSerialise)
        {
            T Deserialized = JsonConvert.DeserializeObject<T>(ToDeSerialise);
            return (Deserialized);
        }
    }

    static class LoadConfiguration
    {
        static public T LoadFileReturnJSON<T>(String File)
        {
            String Res = "";
            try
            {
                Res = System.IO.File.ReadAllText(File);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return (SerializationClass.Deserialize<T>(Res));
        }
    }
}
