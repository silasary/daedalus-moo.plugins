using System.Collections.Generic;
using Chiroptera.Base;
using Daedalus.PluginModel;
using Silasary;
using System.IO;
using System;

namespace Daedalus.Plugin.Growl
{
    public class GrowlPlugin : IPlugin
    {
        Growler growler;

        public GrowlPlugin()
        {
            try
            {
                growler = new Growler();
            }
            catch (System.IO.FileNotFoundException v) // The CLR doesn't scan subdirectories for required assemblies.  We have two options: screw around with WD and/or Path, or Find and load them ourselves.
            {
                //AppDomain.CurrentDomain.Load(File.ReadAllBytes(Path.GetFullPath(Directory.GetFiles(".", "Growl.CoreLibrary.dll", SearchOption.AllDirectories)[0])));
                //AppDomain.CurrentDomain.Load(File.ReadAllBytes(Path.GetFullPath(Directory.GetFiles(".", "Growl.Connector.dll", SearchOption.AllDirectories)[0])));
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                growler = new Growler();  // Now the assemblies are loaded, this will work.
            }
        }

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
                if (connection.Session.Username == "" || connection.Session.Username == "Username")
                    return message;
                if (message.Text.ToLowerInvariant().Contains(connection.Session.Username.ToLowerInvariant()))
                    g.Notify(message.Text, "Mention");
                return message;
            }
        }
    }
}
