using System;
using S22.Xmpp.Client;

public class XmppConnectionClass
{
    String _host;
    String _username;
    String _password;
    XmppClient _client;

    public XmppConnectionClass(String host, String username, String password)
	{
        //Console.WriteLine("qdddddddddddddddddddddddd\n");
        //Console.WriteLine(host);
        _host = host;
        _username = username;
        _password = password;
        //XmppClient _client = new XmppClient(_host, _username, _password);
    }

    public void Connect()
    {
        /*
        _client.Connect();
        Console.WriteLine("Connected as " + _client.Jid + Environment.NewLine);
        Console.WriteLine(" Type 'send <JID> <Message>' to send a chat message, or 'quit' to exit.");
        Console.WriteLine(" Example: send user@domain.com Hello!");
        Console.WriteLine();
        */
    }
}
