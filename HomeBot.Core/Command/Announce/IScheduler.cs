using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;

namespace HomeBot.Core.Command.Announce
{
    public interface IScheduler
    {
        void Start(IEnumerable<IAnnounce> tasks, ICommunicator comm);
        void Stop();
    }
}
