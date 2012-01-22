using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HomeBot.Core.Communication;

namespace HomeBot.Core.Command.Respond.BuiltIn
{
    public class IPityTheFoolResponder : RegexSprocket
    {
        public override Regex Pattern
        {
            get { return new Regex(@".*(?:fool|pity)+.*", RegexOptions.IgnoreCase); }
        }

        protected override void ProcessMatch(Match match, ChatMessage message, ICommunicator comm)
        {
            comm.SendMessage(message.From, "http://xamldev.dk/IPityTheFool.gif");
        }
    }
}
