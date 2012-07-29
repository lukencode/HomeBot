using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;
using System.Threading.Tasks;
using NLog;
using HomeBot.Core.Base;
using HomeBot.Core.Helpers;
using System.Reflection;

namespace HomeBot.Core
{
    public class HomeBotController
    {
        private ICommunicator _comm;
        private IScheduler _scheduler;

        private IEnumerable<IAnnounce> _announcers;
        private IEnumerable<ICommand> _commands;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        private static Type _commandAttributeType = typeof(CommandAttribute);
        private static Type _commandInterfaceType = typeof(ICommand);

        public HomeBotController(ICommunicator comm, IScheduler scheduler, IEnumerable<IAnnounce> announcers, IEnumerable<ICommand> commands)
        {
            _announcers = announcers;
            _commands = commands;

            _comm = comm;
            _comm.OnMessage += new OnRequestMessageHandler(comm_OnMessage);

            _scheduler = scheduler;
        }

        public void Start()
        {
            _comm.OpenConnection();
            //_scheduler.Start(_announcers, _comm);
        }

        public void Stop()
        {
            _scheduler.Stop();
        }

        private void comm_OnMessage(object sender, OnMessageHandlerArgs args)
        {
            proccessMessage(args.Message);
        }

        private void proccessMessage(ChatMessage message)
        {
            Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrWhiteSpace(message.Message))
                    throw new ArgumentNullException("Message", "Message text cannot be empty");

                _logger.Debug("Processing: {0} - {1}", message.From, message.Message);

                var split = message.Message.SplitCommandLine();
                var command = (split.FirstOrDefault() ?? message.Message).ToLower();

                if (command == "help")
                {
                    sendHelp(message);
                }
                else
                {
                    //todo make better
                    var commandType = (from c in _commands
                                       let attr = (CommandAttribute)c.GetType().GetCustomAttributes(_commandAttributeType, false).FirstOrDefault()
                                       where attr != null
                                       && attr.Name.ToLower() == command
                                       select c).FirstOrDefault();

                    if (commandType == null)
                    {
                        var msg = string.Format("No command found for '{0}'", message.Message);
                        _logger.Debug(msg);
                        _comm.SendMessage(message.From, msg);
                    }
                    else
                    {
                        commandType.Process(message, _comm);
                    }
                }
            })
            .ContinueWith(task =>
            {
                // Just write to debug output if it failed
                if (task.IsFaulted)
                {
                    _logger.ErrorException("Failed to process messages. {0}", task.Exception.GetBaseException());
                    _comm.SendMessage(message.From, string.Format("Failed to process messages. {0}", task.Exception.GetBaseException().Message));
                }
            });
        }

        private void sendHelp(ChatMessage message)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(string.Format("{0} - Displays this...", "HELP".PadRight(15, ' ')));

            foreach (var c in _commands)
            {
                var type = c.GetType();
                var attr = (CommandAttribute)type.GetCustomAttributes(_commandAttributeType, false).FirstOrDefault();

                if (attr == null)
                    continue;

                sb.AppendLine(string.Format("{0} - {1}", attr.Name.ToUpper().PadRight(15, ' '), attr.Description));
            }

            _comm.SendMessage(message.From, sb.ToString());
        }
    }
}
