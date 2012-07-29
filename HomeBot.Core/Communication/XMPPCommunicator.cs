using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.net;
using agsXMPP.protocol.client;
using System.Threading;
using agsXMPP.Xml.Dom;
using NLog;
using agsXMPP.protocol.iq.roster;

namespace HomeBot.Core.Communication
{
    public class XMPPCommunicator : ICommunicator
    {
        private Timer _reconnectTimer;
        private int _timerDelay = 15000; // 15s start delay
        private XmppClientConnection _xmpp;
        private bool _sockWasConnected = false;

        private List<BotUser> _users;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Configure(ServerConfiguration configuration)
        {
            _users = new List<BotUser>();

            _xmpp = new XmppClientConnection
            {
                Server = configuration.Server,
                ConnectServer = configuration.ConnectServer,
                Username = configuration.Username,
                Password = configuration.Password,
                KeepAlive = true,
                UseStartTLS = true,
                SocketConnectionType = SocketConnectionType.Direct,
                AutoResolveConnectServer = true
            };

            RegisterXmppEventHandlers();
        }

        public void AddUser(BotUser user)
        {
            _users.Add(user);

            _xmpp.PresenceManager.Subscribe(user.Name);
            _xmpp.RosterManager.AddRosterItem(user.Name);
        }

        public IEnumerable<BotUser> GetUsers()
        {
            return _users.ToList();
        }

        private void RegisterXmppEventHandlers()
        {
            _xmpp.OnMessage += xmpp_OnMessage;
            _xmpp.OnLogin += xmpp_OnLogin;
            _xmpp.OnAuthError += xmpp_OnAuthError;
            _xmpp.OnError += xmpp_OnError;
            _xmpp.OnXmppConnectionStateChanged += xmpp_OnXmppConnectionStateChanged;
            _xmpp.OnClose += xmpp_OnClose;
            _xmpp.OnRosterItem += _xmpp_OnRosterItem;
            _xmpp.OnPresence += _xmpp_OnPresence;
        }

        void _xmpp_OnPresence(object sender, Presence pres)
        {
            var user = _users.FirstOrDefault(u => u.Name.ToLower() == pres.From.Bare);

            if (user == null)
                return;

            if (pres.Type == PresenceType.subscribe)
                _xmpp.PresenceManager.ApproveSubscriptionRequest(pres.From);

            if (pres.Type == PresenceType.available)
            {
                user.Online = true;
            }
            else if (pres.Type == PresenceType.unavailable)
            {
                user.Online = false;
            }
            else if (pres.Type == PresenceType.unsubscribe)
            {
                _users.Remove(user);
            }
        }

        private void _xmpp_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            var user = _users.FirstOrDefault(u => u.Name.ToLower() == item.Jid.Bare);

            if (user == null)
                return;
        }

        public void OpenConnection()
        {
            _logger.Debug("Attempting to open connection.");

            if (_sockWasConnected)
            {
                _xmpp.SocketDisconnect();
                _sockWasConnected = false;
            }

            _xmpp.Open();
        }

        public event OnRequestMessageHandler OnMessage;

        public void SendMessage(string jid, string message)
        {
            var to = new Jid(jid);
            var msg = new Message(to, MessageType.chat, message);

            _logger.Info("msg being sent to: {0} msg text : {1}", msg.To, msg.Body);

            _xmpp.Send(msg);
        }

        public void SendMessageToAll(string message)
        {
            foreach (var u in _users.Where(u => u.Online))
                SendMessage(u.Name, message);
        }

        private void xmpp_OnClose(object sender)
        {
            _logger.Debug("OnClose : Attempting Re-connection.");

            DelayedReconnect();
        }

        private void DelayedReconnect()
        {
            if (_reconnectTimer == null)
                _reconnectTimer = new Timer(AttemptReconnection, null, 0, _timerDelay);
        }

        private void AttemptReconnection(object state)
        {
            OpenConnection();
        }

        private void xmpp_OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            _logger.Debug("OnXmppConnectionStateChanged : Connection State - {0}", state);
        }

        private void xmpp_OnLogin(object sender)
        {
            _logger.Info("OnLogin : Connection established and sending prescence");

            _xmpp.SendMyPresence();

            DisposeConnectionTimer();
            _sockWasConnected = true;
        }

        private void DisposeConnectionTimer()
        {
            if (_reconnectTimer != null)
            {
                _reconnectTimer.Dispose();
                _reconnectTimer = null;
            }
        }

        private void xmpp_OnMessage(object sender, Message msg)
        {
            _logger.Info("OnMessage : {0} - {1}", msg.Type, msg.Body);
            var user = _users.FirstOrDefault(u => u.Name.ToLower() == msg.From.Bare.ToLower());

            if (user == null) 
            {
                SendMessage(msg.From.Bare, "ACCESS DENIED");
                _logger.Warn("UnAuthorized Message : {0} - {1}", msg.From.Bare, msg.Body);
            }
            else
            {
                user.Online = true;

                if (ShouldHandleMessage(msg))
                {
                    OnMessage(this, new OnMessageHandlerArgs(new ChatMessage { From = msg.From.Bare, Message = msg.Body }));
                }
            }
        }

        private bool ShouldHandleMessage(Message msg)
        {
            return msg.Type == MessageType.chat && !string.IsNullOrEmpty(msg.Body);
        }

        private void xmpp_OnError(object sender, Exception ex)
        {
            _logger.Error("OnError : {0} - {1}", ex.Message, ex.StackTrace);
        }

        private void xmpp_OnAuthError(object sender, Element e)
        {
            _logger.Error("OnAuthError : Authorization Error");
        }
    }
}
