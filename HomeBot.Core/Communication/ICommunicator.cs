using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace HomeBot.Core.Communication
{
    public interface ICommunicator
    {
        void SendMessage(string to, string message);
        void SendMessageToAll(string message);
        event OnRequestMessageHandler OnMessage;
        void OpenConnection();
        void Configure(ServerConfiguration configuration);
    }

    public delegate void OnRequestMessageHandler(object sender, OnMessageHandlerArgs args);
}
