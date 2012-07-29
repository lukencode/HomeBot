using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using HomeBot.Core.Communication;

namespace HomeBot.Core.Base
{
    [InheritedExport]
    public interface ICommand
    {
        void Process(ChatMessage message, ICommunicator comm);
    }
}
