using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;
using HomeBot.Core.Command.Announce;
using System.Threading.Tasks;
using NLog;
using HomeBot.Core.Command.Respond;

namespace HomeBot.Core
{
    public class HomeBotController
    {
        private ICommunicator _comm;
        private IScheduler _scheduler;

        private IEnumerable<IAnnounce> _announcers;
        private IEnumerable<IMessageHandler> _handlers;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        public HomeBotController(ICommunicator comm, IScheduler scheduler, IEnumerable<IAnnounce> announcers, IEnumerable<IMessageHandler> handlers)
        {
            _announcers = announcers;
            _handlers = handlers;

            _comm = comm;
            _comm.OnMessage += new OnRequestMessageHandler(comm_OnMessage);

            _scheduler = scheduler;
        }

        public void Start()
        {
            _comm.OpenConnection();
            _scheduler.Start(_announcers, _comm);
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
                _logger.Debug("Processing: {0} - {1}", message.From, message.Message);

                var handled = false;

                foreach (var handler in _handlers)
                {
                    if (handler.Handle(message, _comm))
                    {
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    // Loop over the unhandled message sprockets
                    //foreach (var handler in _unhandledMessageSprockets)
                    //{
                    //    // Stop at the first one that handled the message
                    //    if (handler.Handle(message, this))
                    //    {
                    //        break;
                    //    }
                    //}

                    var msg = string.Format("No responder found for '{0}'", message.Message);
                    _logger.Debug(msg);
                    _comm.SendMessage(message.From, msg);
                }
            })
            .ContinueWith(task =>
            {
                // Just write to debug output if it failed
                if (task.IsFaulted)
                {
                    _logger.Debug("Failed to process messages. {0}", task.Exception.GetBaseException());
                    _comm.SendMessage(message.From, string.Format("Failed to process messages. {0}", task.Exception.GetBaseException()));
                }
            });
        }

    }
}
