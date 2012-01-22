using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP.protocol.client;

namespace HomeBot.Communication
{
    public class OnMessageHandlerArgs : EventArgs
    {
        public OnMessageHandlerArgs(Message msg)
        {
            Message = msg;
        }

        public Message Message { get; set; }
    }
}
