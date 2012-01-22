using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeBot.Core.Communication;
using System.Configuration;
using System.IO;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using HomeBot.Core.Command.Announce;
using HomeBot.Core;
using NLog;
using System.Reflection;
using HomeBot.Core.Command.Respond;

namespace HomeBot
{
    class Program
    {
        private static readonly string _serverUrl = ConfigurationManager.AppSettings["Bot.Server"];
        private static readonly string _connectServerUrl = ConfigurationManager.AppSettings["Bot.ConnectServer"];
        private static readonly string _botName = ConfigurationManager.AppSettings["Bot.User"];
        private static readonly string _botPassword = ConfigurationManager.AppSettings["Bot.Password"];
        private static bool _appShouldExit = false;

        private const string ExtensionsFolder = "Extensions";

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                //configure xmpp link
                var comm = new XMPPCommunicator();
                comm.Configure(new ServerConfiguration(_serverUrl, _connectServerUrl, _botName, _botPassword));

                //grab extensions
                var container = CreateCompositionContainer();
                var announcers = container.GetExportedValues<IAnnounce>();
                var handlers = container.GetExportedValues<IMessageHandler>();

                //fire her up
                var bot = new HomeBotController(comm, new Scheduler(), announcers, handlers);
                bot.Start();

                Console.Write("Press enter to quit...");
                Console.ReadLine();

                bot.Stop();
            }
            catch (Exception ex)
            {
                _logger.FatalException("ERROR: ", ex);
            }
        }
        
        private static CompositionContainer CreateCompositionContainer()
        {
            ComposablePartCatalog catalog;

            var extensionsPath = GetExtensionsPath();

            // If the extensions folder exists then use them
            if (Directory.Exists(extensionsPath))
            {
                catalog = new AggregateCatalog(
                    new AssemblyCatalog(typeof(HomeBotController).Assembly),
                    new AssemblyCatalog(typeof(Program).Assembly),
                    new DirectoryCatalog(extensionsPath, "*.dll"));
            }
            else
            {
                catalog = new AssemblyCatalog(typeof(HomeBotController).Assembly);
            }

            return new CompositionContainer(catalog);
        }

        private static string GetExtensionsPath()
        {
            var rootPath = Directory.GetCurrentDirectory();
            return Path.Combine(rootPath, ExtensionsFolder);
        }
    }
}
