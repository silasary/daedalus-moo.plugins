using System.Collections.Generic;
using Chiroptera.Base;
using Daedalus.PluginModel;
using Silasary;

namespace Daedalus.Plugin.Growl
{
    public class GrowlPlugin : IPlugin
    {
        Growler growler = new Growler();
        public List<MCP.MCPPackage> MCPPackages
        {
            get { return new List<MCP.MCPPackage>(); }
        }

        public void NewConnection(IConnection conn)
        {
            new GrowlService(conn, growler);
        }
        
        private class GrowlService
        {
            private IConnection connection;
            Growler g;
            public GrowlService(IConnection conn, Growler growler)
            {
                conn.ServicesDispatcher.RegisterMessageHandler(MessageHandler);
                connection = conn;
                g = growler;
            }

            private ColorMessage MessageHandler(ColorMessage message)
            {
                if (message.Text.ToLowerInvariant().Contains(connection.Session.Username.ToLowerInvariant()))
                    g.Notify(message.Text, "Mention");
                return message;
            }
        }
    }
}
