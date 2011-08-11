using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Growl.Connector;

namespace Silasary
{
    class Growler
    {
        Growl.Connector.Application growlApp = new Growl.Connector.Application("Daedalus");
        GrowlConnector connector = new GrowlConnector();
        bool growlregistered = false;

        NotificationType[] growlNotificationTypes = new NotificationType[] { new NotificationType("Mention"), new NotificationType("User Connected"), new NotificationType("Friend Connected"), new NotificationType("Line Recieved") };

        public bool Notify(string Message)
        {
            return Notify(Message, "Message");
        }

        public bool Notify(string Message, string NotificationType)
        {
            return Notify(Message, NotificationType, null);
        }
        public bool Notify(string Message, string NotificationType, string ID)
        {
            if (GrowlConnector.IsGrowlRunningLocally())
            {
                if (!growlregistered)
                {
                    connector.Register(growlApp, growlNotificationTypes);
                    connector.NotificationCallback += new GrowlConnector.CallbackEventHandler(connector_NotificationCallback);
                    growlregistered = true;
                }
                string title = Daedalus.Settings.Default.ClientName;
                Notification n = new Notification(growlApp.Name, NotificationType, ID, title, Message);
                connector.Notify(n);
                return true;
            }
            return false;
        }

        #region static
        internal static bool isGrowlDeployed()
        {
            return File.Exists(Path.Combine(Environment.CurrentDirectory, "Growl.CoreLibrary.dll"));
        }
        public static Dictionary<string, Action> Callbacks = new Dictionary<string, Action>();
        public static string Callback(Action action)
        {
            string id = new Random().Next().ToString();
            Callbacks.Add(id, action);
            return id;
        }
        static void connector_NotificationCallback(Response response, CallbackData callbackData, object state)
        {
            if (Callbacks.ContainsKey(callbackData.NotificationID))
            {
                Callbacks[callbackData.NotificationID].Invoke();
            }
            Callbacks.Remove(callbackData.NotificationID);
        }

        #endregion

        #region growlsearch
        // Move these to Growler.
        internal static void GrowlFinder_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                string Growl = GrowlSearch(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                if (Growl == null)
                    Growl = GrowlSearch(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                if (Growl == null)
                    foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
                    {
                        Growl = GrowlSearch(drive.RootDirectory.ToString());
                        if (Growl != null)
                            break;
                    }
                if (Growl != null)
                {
                    System.Diagnostics.Process.Start(Growl);
                }
            }
            catch (Exception v)
            {
                //Log.Error("Searching for Growl", v);
            }
        }

        private static string GrowlSearch(string path)
        {
            if (!Directory.Exists(path))
                return null; //Only situation this should show up is if we're passed ProgramFilesX86 on a non-64bit system.  Or the disk isn't as fixed as it claimed :/
            string growl = Directory.GetFiles(path).FirstOrDefault(f => Path.GetFileName(f).ToLower() == "growl.exe");
            if (growl != null)
                return growl;
            foreach (string folder in Directory.GetDirectories(path))
            {
                try
                {
                    growl = GrowlSearch(folder);
                    if (growl != null)
                        return growl;
                }
                catch (UnauthorizedAccessException)
                {//Access Denied.
                    //System.Diagnostics.Debugger.Break();
                }
            }
            return null;
        }
        #endregion

        internal bool IsGrowlRunningLocally()
        {
            return GrowlConnector.IsGrowlRunningLocally(); //Chaining this is illogical extremes, because I can afford for typeof(Growler) to fail loading.
        }
    }
}
