using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeBot.Communication
{
    public class ServerConfiguration
    {
        public ServerConfiguration(string server, string connectServer, string username, string password)
        {
            Server = server;
            Username = username;
            Password = password;
            ConnectServer = connectServer;
        }

        public string Server { get; set; }
        public string ConnectServer { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
