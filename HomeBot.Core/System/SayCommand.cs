using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Base;
using HomeBot.Core.Communication;

namespace HomeBot.Core.System
{
    [Command("say", "talk via your robot friend")]
    public class SayCommand : ICommand
    {
        public void Process(ChatMessage message, ICommunicator comm)
        {
            var username = message.Args.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(username))
            {
                var text = string.Join(" ", message.Args.Skip(1));

                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (username == "all")
                        comm.SendMessageToAll(text);
                    else
                        comm.SendMessage(username, text);
                }
                else
                {
                    comm.SendMessage(message.From, "Please use the format: 'say {username|all} {msg}'");
                }
            }
            else
            {
                comm.SendMessage(message.From, "Please use the format: 'say {username|all} {msg}'");
            }
        }
    }
}
