using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
namespace Witivio.JBot.Core
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
}
