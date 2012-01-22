using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using HomeBot.Core.Communication;

namespace HomeBot.Core.Command.Respond
{
    [InheritedExport]
    public interface IMessageHandler
    {
        bool Handle(ChatMessage message, ICommunicator comm);
    }
}
