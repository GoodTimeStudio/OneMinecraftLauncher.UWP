using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet;
using KMCCC.Launcher;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("UWPCore", "Main");

        public const string ServiceName = "LaunchAgent";
        static AutoResetEvent appServiceExit;
        static Dictionary<string, IServicePacket> packets = new Dictionary<string, IServicePacket>();

        public static OneMCL Launcher;
        public static bool isInited
        {
            get => Launcher != null;
        }

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

            //Get log4net config in assembly
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(Properties.Resources.log4net_config);
            XmlConfigurator.Configure(LogManager.CreateRepository("UWPCore"), xml.DocumentElement);

            Logger.Info("Starting One Minecraft Launcher (UWP Core)");

            RegisterPacket(new PacketInit());
            RegisterPacket(new PacketLaunchMessage());
            RegisterPacket(new PacketLibrariesCheck());
            RegisterPacket(new PacketVersionsList());
            RegisterPacket(new PacketVersionUrl());
            RegisterPacket(new PacketAssetsCheck());

            appServiceExit = new AutoResetEvent(false);
            //LaunchTest();

            try
            {
                Process(args);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }

            appServiceExit.WaitOne();

            Logger.Info("UWPCore is shutting down.");
            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }

        private static void LaunchTest()
        {
            OneMCL mcl = new OneMCL(@"E:\BestOwl\Desktop\LauncherTest");
            LaunchOptionBase launchOption = new LaunchOptionBase("test")
            {
                javaExt = @"D:\Program Files (x86)\Minecraft\runtime\jre-x64\1.8.0_51\bin\java.exe",
                versionId = "1.12.2",
            };
            mcl.UserAuthenticator = new KMCCC.Authentication.OfflineAuthenticator("MicroOwl");

            List<Library> l = mcl.Core.GetVersion("1.12.2").Libraries;
            mcl.Launch(launchOption);
        }

        private static void setColor(ConsoleColor c)
        {
            Console.ForegroundColor = c;
        }

        public static void RegisterPacket(IServicePacket packet)
        {
            packets.Add(packet.GetTypeName(), packet);
        }

        public async static void Process(string[] args)
        {
            Logger.Info("Openning AppService connection.");

            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = ServiceName;
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status == AppServiceConnectionStatus.Success)
            {
                setColor(ConsoleColor.Green);
                Logger.Info("Connection established, waiting for requests");
                setColor(ConsoleColor.White);
            }
            else
            {
                setColor(ConsoleColor.Red);
                Logger.Error("Connection open failed: " + status.ToString());
                setColor(ConsoleColor.White);
                appServiceExit?.Set();
            }
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            setColor(ConsoleColor.Red);
            Logger.Error("Connection closed");
            setColor(ConsoleColor.White);
            appServiceExit?.Set();
        }

        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            setColor(ConsoleColor.Yellow);
            string type = args.Request.Message["type"].ToString();
            Logger.Info("Received request, type: " + type);
            setColor(ConsoleColor.White);

            IServicePacket service = null;
            packets.TryGetValue(type, out service);

            ValueSet ret = null;
            if (service == null)
            {
                //send response
                ret = new ValueSet();
            }
            else
            {
                ValueSet tmp = null;
                try
                {
                    if (isInited || service is PacketInit)
                    {
                        tmp = service.OnRequest(sender, args);
                    }
                    else
                    {
                        Logger.Error("Receied packet before launcher initialized, use PacketInit first");
                    }
                }
                catch(Exception e)
                {
                    Logger.Fatal(e);
                }

                if (tmp == null)
                {
                    tmp = new ValueSet();
                }

                tmp["type"] = type;
                ret = tmp;
                tmp = null;
            }
            await args.Request.SendResponseAsync(ret);

        }
       
    }

}
