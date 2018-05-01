using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet;
using KMCCC.Launcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using static GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet.PacketAssetsCheck;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core
{
    class Program
    {
        public const string ServiceName = "LaunchAgent";
        static AutoResetEvent appServiceExit;
        static Dictionary<string, IServicePacket> packets = new Dictionary<string, IServicePacket>();

        public static LauncherCore Core;

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

            RegisterPacket(new PacketInit());
            RegisterPacket(new PacketLaunchMessage());
            RegisterPacket(new PacketLibrariesCheck());
            RegisterPacket(new PacketVersionsList());
            RegisterPacket(new PacketVersionUrl());
            RegisterPacket(new PacketAssetsCheck());
            RegisterPacket(new PacketAssetIndexCheck());

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


                Core = OneMinecraftLauncher.Core.OneMinecraftLauncher.CreateLauncherCore(message);
                List<Library> l = Core.GetVersion("1.12.2").Libraries;
                //OneMinecraftLauncher.Core.OneMinecraftLauncher.Launch(core, message);
            }
            
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

        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            setColor(ConsoleColor.Yellow);
            string type = args.Request.Message["type"].ToString();
            Console.WriteLine("*************************************************");
            Console.WriteLine("Received request, type: " + type);
            Console.WriteLine("*************************************************");
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
                    tmp = service.OnRequest(sender, args);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
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

            setColor(ConsoleColor.Yellow);
            Console.WriteLine("*************************************************");
            setColor(ConsoleColor.White);
        }
       
    }

    public class DLibrary
    {
        public string Name;

        public string Path;

        public string Url;
    }
}
