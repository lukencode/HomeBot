using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;

namespace HomeBot.Core.Command.Announce
{
    public class PulseAnnouncer : IAnnounce
    {
        public TimeSpan Interval
        {
            get { return new TimeSpan(0, 1, 0); }
        }

        public void Execute(ICommunicator comm)
        {
            comm.SendMessageToAll("I AM STILL ALIVE");
        }
    }
}
