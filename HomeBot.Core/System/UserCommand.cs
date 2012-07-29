using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Base;
using HomeBot.Core.Communication;
using NDesk.Options;

namespace HomeBot.Core.System
{
    [Command("users", "Administer users")]
    public class UserCommand : ICommand
    {
        private string[] _args;
        private AvailableCommand _command;

        private ChatMessage _msg;
        private ICommunicator _comm;

        private enum AvailableCommand
        {
            List,
            Add,
            Help
        }

        public void Process(ChatMessage message, ICommunicator comm)
        {
            _args = message.Args;
            _msg = message;
            _comm = comm;

            var p = new OptionSet() 
            {
                { "l|list", "Show current users",
                    v => _command = AvailableCommand.List },
                { "add|a", "Add a new user",
                    v => _command = AvailableCommand.Add },
                { "h|help", "show this list of options",
                    v => _command = AvailableCommand.Help }
            };

            List<string> extras;
            try
            {
                extras = p.Parse(_args);
            }
            catch (OptionException e)
            {
                showHelp();
                return;
            }

            switch (_command)
            {
                case AvailableCommand.List:
                    listCommand();
                    break;

                case AvailableCommand.Add:
                    addCommand(extras.FirstOrDefault());
                    break;

                default:
                    showHelp();
                    break;
            }
        }

        private void showHelp()
        {
        }

        private void addCommand(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _comm.SendMessage(_msg.From, "Please specify a user to add");
            }
            else
            {
                _comm.AddUser(new BotUser() { Name = username });
                _comm.SendMessage(_msg.From, string.Format("'{0}' added", username));
            }
        }

        private void listCommand()
        {
            var users = _comm.GetUsers().Select(u => u.Name);
            _comm.SendMessage(_msg.From, string.Join(", ", users));
        }
    }
}
