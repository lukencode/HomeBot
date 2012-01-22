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

namespace HomeBot.Communication
{
    public class XMPPCommunicator : ICommunicator
    {
        private Timer _reconnectTimer;
        private int _timerDelay = 15000; // 15s start delay
        private XmppClientConnection _xmpp;
        private bool _sockWasConnected = false;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Configure(ServerConfiguration configuration)
        {
            _xmpp = new XmppClientConnection
            {
                Server = configuration.Server,
                //ConnectServer = configuration.ConnectServer,
                Username = configuration.Username,
                Password = configuration.Password,
                KeepAlive = true,
                UseStartTLS = true,
                SocketConnectionType = SocketConnectionType.Direct,
                AutoResolveConnectServer = true
            };

            RegisterXmppEventHandlers();
        }

        private void RegisterXmppEventHandlers()
        {
            _xmpp.OnMessage += xmpp_OnMessage;
            _xmpp.OnReadXml += xmpp_OnReadXml;
            _xmpp.OnWriteXml += xmpp_OnWriteXml;
            _xmpp.OnLogin += xmpp_OnLogin;
            _xmpp.OnAuthError += xmpp_OnAuthError;
            _xmpp.OnSocketError += xmpp_OnSocketError;
            _xmpp.OnError += xmpp_OnError;
            _xmpp.OnXmppConnectionStateChanged += xmpp_OnXmppConnectionStateChanged;
            _xmpp.OnClose += xmpp_OnClose;
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

        private void xmpp_OnWriteXml(object sender, string xml)
        {
            _logger.Debug("OnWriteXml : {0}", xml);
        }

        private void xmpp_OnReadXml(object sender, string xml)
        {
            _logger.Debug("OnReadXml : {0}", xml);
        }

        private void xmpp_OnMessage(object sender, Message msg)
        {
            _logger.Info("OnMessage : {0} - {1}", msg.Type, msg.Body);

            if (ShouldHandleMessage(msg))
            {
                OnMessage(this, new OnMessageHandlerArgs(msg));
            }
        }

        private void xmpp_OnError(object sender, Exception ex)
        {
            _logger.Error("OnError : {0} - {1}", ex.Message, ex.StackTrace);
        }

        private void xmpp_OnSocketError(object sender, Exception ex)
        {
            _logger.Error("OnSocketError : message = {0} - stack trace = {1}", ex.Message, ex.StackTrace);
        }

        private void xmpp_OnAuthError(object sender, Element e)
        {
            _logger.Error("OnAuthError : Authorization Error");
        }

        private bool ShouldHandleMessage(Message msg)
        {
            return msg.Type == MessageType.chat && !string.IsNullOrEmpty(msg.Body);
        }
    }
}
