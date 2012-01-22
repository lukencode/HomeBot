using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;
using System.ComponentModel.Composition;

namespace HomeBot.Core.Command.Announce
{
    [InheritedExport]
    public interface IAnnounce
    {
        TimeSpan Interval { get; }
        void Execute(ICommunicator comm);
    }
}
