using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Helpers;

namespace HomeBot.Core.Communication
{
    public class ChatMessage
    {
        public string From { get; set; }
        public string Message { get; set; }

        public string[] Args
        {
            get
            {
                return Message.SplitCommandLine().Skip(1).ToArray();
            }
        }
    }
}
