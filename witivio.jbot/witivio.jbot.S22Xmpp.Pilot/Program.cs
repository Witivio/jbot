using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using Witivio.JBot.Core.Services;

namespace witivio.jbot.S22Xmpp.Pilot
{
    class Program
    {
        static void Main(string[] args)
        {
            // lire les settings DONE
            //File.io Readfile DONE
            //string Deeserialise json newtonsoft DONE
            // recuperer les credentials DONE
            // instancier la classe Jabber Client DONE
            // passer les crendentials DONE
            // connection DONE
            // abonnement à tous les event 
            // afficher les infos dans la console
            
            XmppServerCredential JabberCredential = new XmppServerCredential();
            Witivio.JBot.Core.Models.ServerModelCredential Test = new Witivio.JBot.Core.Models.ServerModelCredential();
            witivio.jbot.S22Xmpp.Pilot.XmppServerModelCredential Tmp = JabberCredential.GetCredential<RootObject>().Credentials.XmppServerModelCredential;
            Test.Host = Tmp.Host;
            Test.User = Tmp.User;
            Test.Password = Tmp.Password;
            Test.Port = Tmp.Port;
            var client = new JabberClient(Test);
            client.Start();
            Thread.Sleep(130000);
        }
    }
}
