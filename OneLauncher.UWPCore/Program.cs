using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core
{
    class Program
    {
        public const string ServiceName = "LaunchAgent";
        static AutoResetEvent appServiceExit;

        public static LauncherCore core;

        static void Main(string[] args)
        {
            Console.WriteLine(" ******************************************************************** ");
            Console.WriteLine(" * ");
            Console.WriteLine(" *                            DEBUG MODE");
            Console.WriteLine(" * ");
            Console.WriteLine(" *        NOTE: THIS CONSOLE WINDOW WILL NOT APPEAR * ");
            Console.WriteLine(" *         WHEN IN NORMAL MODE                          ");
            Console.WriteLine(" * ");
            Console.WriteLine(" ******************************************************************** ");

            Console.WriteLine("Starting One Minecraft Launcher (UWP Core)");

            appServiceExit = new AutoResetEvent(false);
            Process(args);

            appServiceExit.WaitOne();

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static void setColor(ConsoleColor c)
        {
            Console.ForegroundColor = c;
        }


        public async static void Process(string[] args)
        {
            Console.WriteLine("Openning AppService connection.");

            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = ServiceName;
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status == AppServiceConnectionStatus.Success)
            {
                setColor(ConsoleColor.Green);
                Console.WriteLine("Connection established, waiting for requests");
                setColor(ConsoleColor.White);
            }
            else
            {
                setColor(ConsoleColor.Red);
                Console.WriteLine("Connection open failed: " + status.ToString());
                setColor(ConsoleColor.White);
                appServiceExit?.Set();
            }
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            setColor(ConsoleColor.Red);
            Console.WriteLine("Connection closed");
            setColor(ConsoleColor.White);
            appServiceExit?.Set();
        }

        private static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            setColor(ConsoleColor.Yellow);
            string type = args.Request.Message["type"].ToString();
            Console.WriteLine("*************************************************");
            Console.WriteLine("Received request, type: " + type);
            Console.WriteLine("*************************************************");
            setColor(ConsoleColor.White);
            switch (type)
            {
                case "init":
                    Init_RequestReceived(sender, args);
                    break;
                case "launch":
                    LaunchAgent_RequestReceived(sender, args);
                    break;
                case "versionsList":
                    VersionList_RequestReceived(sender, args);
                    break;
            }

            setColor(ConsoleColor.Yellow);
            Console.WriteLine("*************************************************");
            setColor(ConsoleColor.White);
        }

        private async static void Init_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string workDir = args.Request.Message["workDir"].ToString();
            if (string.IsNullOrWhiteSpace(workDir))
            {
                return;
            }

            Console.WriteLine("Work dir is " + workDir);
            Console.WriteLine("Creating KMCCC LauncherCore");

            LaunchMessage message = new LaunchMessage { WorkDirPath = workDir };
            core = OneMinecraftLauncher.Core.OneMinecraftLauncher.CreateLauncherCore(message);
            await args.Request.SendResponseAsync(new ValueSet());
        }

        private async static void VersionList_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (core == null)
                return;

            List<string> list = new List<string>();
            var vers = core.GetVersions();

            Console.WriteLine("Found " + vers.Count() + " versions: ");

            foreach (KMCCC.Launcher.Version ver in vers)
            {
                list.Add(ver.Id);

                Console.WriteLine("    # " + ver.Id);
            }

            Console.WriteLine("Serializing versions list to json");

            string json = null;
            try
            {
                json = JsonConvert.SerializeObject(list);
            }
            catch (JsonException)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(json))
                return;

            Console.WriteLine("Versions List json: ");
            Console.WriteLine("     " + json);

            ValueSet valueSet = new ValueSet();
            valueSet["type"] = "versionsList";
            valueSet["value"] = json;

            Console.WriteLine("Sending versions list to app");

            await args.Request.SendResponseAsync(valueSet);
        }

        private async static void LaunchAgent_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (core == null)
                return;

            string json = args.Request.Message["message"].ToString();

            Console.WriteLine("Launch message is :");
            Console.WriteLine("     " + json);
            Console.WriteLine("Deserializing launch message");

            LaunchMessage message = null;
            ValueSet ret = new ValueSet();
            ret["result"] = false;

            try
            {
                message = JsonConvert.DeserializeObject<LaunchMessage>(json);
            }
            catch (JsonException e)
            {
                ret["errorMessage"] = e.Message;
                ret["errorStack"] = e.StackTrace;

                Console.WriteLine("ERROR: " + e.Message);
                Console.WriteLine("     " + e.StackTrace);
            }

            if (message != null)
            {
                Console.WriteLine("Ready to launch");

                LaunchResult launchResult = OneMinecraftLauncher.Core.OneMinecraftLauncher.Launch(core, message);

                if (launchResult.Success)
                {
                    ret["result"] = true;

                    Console.WriteLine("Launch successfully");
                }
                else
                {
                    ret["errorMessage"] = launchResult.ErrorMessage;
                    ret["errorStack"] = launchResult.Exception?.StackTrace;

                    Console.WriteLine("Launch failed");
                    Console.WriteLine("ERROR: " + launchResult.ErrorMessage);
                }
            }

            Console.WriteLine("Sending launch result to app");
            await args.Request.SendResponseAsync(ret);
        }

    }
}
