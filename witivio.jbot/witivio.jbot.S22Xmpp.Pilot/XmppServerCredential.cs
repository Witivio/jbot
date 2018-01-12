using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace witivio.jbot.S22Xmpp.Pilot
{
    public class XmppServerModelCredential
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
    }

    public class Credentials
    {
        public XmppServerModelCredential XmppServerModelCredential { get; set; }
    }

    public class RootObject
    {
        public Credentials Credentials { get; set; }
    }

    


    public class XmppServerCredential /* TODO implement interface */
    {
        public T GetCredential<T>()
        {
            //Directory.GetCurrentDirectory() + "\\";
            return (LoadConfiguration.LoadFileReturnJSON<T>("../../" + "appsettings.json"));
        }
    }
}
