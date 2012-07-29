using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Base;
using HomeBot.Core.Communication;
using NDesk.Options;

namespace HomeBot.Core.System
{
    [Command("system", "Administer homebot")]
    public class SystemCommand : ICommand
    {
        public void Process(ChatMessage message, ICommunicator comm)
        {
            throw new NotImplementedException();
        }
    }
}
