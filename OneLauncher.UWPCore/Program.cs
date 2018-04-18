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
            //LaunchTest();
            Process(args);
            appServiceExit.WaitOne();

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static void LaunchTest()
        {
            for (int i = 0; i < 3; i++)
            {
                LaunchMessage message = new LaunchMessage
                {
                    WorkDirPath = @"E:\BestOwl\Desktop\LauncherTest",
                    JavaExtPath = @"D:\Program Files (x86)\Minecraft\runtime\jre-x64\1.8.0_51\bin\java.exe",
                    VersionId = "1.12.2",
                    authenticator = new KMCCC.Authentication.OfflineAuthenticator("MicroOwl")
                };


                core = OneMinecraftLauncher.Core.OneMinecraftLauncher.CreateLauncherCore(message);
                List<Library> l = core.GetVersion("1.12.2").Libraries;
                //OneMinecraftLauncher.Core.OneMinecraftLauncher.Launch(core, message);
            }
            
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
                case "librariesCheck":
                    LibrariesCheck_RequestReceived(sender, args);
                    break;
                case "version-url":
                    VersionDownloadUrl_RequestReceived(sender, args);
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

        private async static void VersionDownloadUrl_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string versionID = args.Request.Message["version"].ToString();

            if (core == null)
                return;

            KMCCC.Launcher.Version ver = core.GetVersion(versionID);

            if (ver == null)
                return;

            ValueSet ret = new ValueSet();
            ret["client"] = ver.ClientJarUrl;
            ret["client-sha1"] = ver.ClientJarSHA1;
            ret["server"] = ver.ServerJarUrl;
            ret["server-sha1"] = ver.ServerJarSHA1;

            await args.Request.SendResponseAsync(ret);
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

        private async static void LibrariesCheck_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (core == null)
                return;

            string versionID = args.Request.Message["version"].ToString();
            if (string.IsNullOrWhiteSpace(versionID))
                return;

            Console.WriteLine("Scanning libraries and natives");
            KMCCC.Launcher.Version ver = core.GetVersion(versionID);

            List<DLibrary> missing = new List<DLibrary>();
            List<Library> missingLib = core.CheckLibraries(ver);
            List<Native> missingNative = core.CheckNatives(ver);

            Console.WriteLine("Found " + missingLib?.Count + " missing libraries");
            foreach (Library lib in missingLib)
            {
                string dName = lib.Url.Substring(lib.Url.LastIndexOf('/') + 1);
                Console.WriteLine("     # " + dName);
                missing.Add(new DLibrary
                {
                    Name = dName,
                    Path = core.GetLibPath(lib),
                    Url = lib.Url
                });
            }

            Console.WriteLine("Found " + missingNative?.Count + " missing natives");
            foreach (Native nav in missingNative)
            {
                string dName = nav.Url.Substring(nav.Url.LastIndexOf('/') + 1);
                Console.WriteLine("     # " + dName);
                missing.Add(new DLibrary
                {
                    Name = dName,
                    Path = core.GetNativePath(nav),
                    Url = nav.Url
                });
            }

            Console.WriteLine("Serializing list to json");
            string json = null;
            try
            {
                json = JsonConvert.SerializeObject(missing);
            }
            catch (JsonException)
            {
                return;
            }

            ValueSet ret = new ValueSet();
            ret["value"] = json;

            Console.WriteLine("Sending list to app");
            await args.Request.SendResponseAsync(ret);
        }
    }

    public class DLibrary
    {
        public string Name;

        public string Path;

        public string Url;
    }
}
