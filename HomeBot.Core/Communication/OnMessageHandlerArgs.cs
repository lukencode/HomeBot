using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP.protocol.client;

namespace HomeBot.Core.Communication
{
    public class OnMessageHandlerArgs : EventArgs
    {
        public OnMessageHandlerArgs(ChatMessage msg)
        {
            Message = msg;
        }

        public ChatMessage Message { get; set; }
    }
}
