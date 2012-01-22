using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HomeBot.Core.Communication;

namespace HomeBot.Core.Command.Respond
{
    public abstract class RegexSprocket : IMessageHandler
    {
        public abstract Regex Pattern { get; }

        public bool Handle(ChatMessage message, ICommunicator comm)
        {
            if (Pattern == null)
            {
                return false;
            }

            Match match;
            if (!(match = Pattern.Match(message.Message)).Success)
            {
                return false;
            }

            ProcessMatch(match, message, comm);

            return true;
        }

        protected abstract void ProcessMatch(Match match, ChatMessage message, ICommunicator comm);
    }
}
