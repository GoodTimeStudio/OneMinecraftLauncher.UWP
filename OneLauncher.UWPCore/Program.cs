using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core
{
    class Program
    {
        static readonly string ServiceName = "RemoteLaunchAgentService";

        static string[] _args;

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

            _args = args;
            foreach (string str in args)
            {
                Console.WriteLine("arg: " + str);
            }

            Task.Run(async () =>
            {
                await DO();
            }).Wait();

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }


        public async static Task DO()
        {

            foreach (string str in _args)
            {
                switch (str)
                {
                    case "/launch":
                        await LaunchGameAsync();
                        break;
                    case "/getversions":
                        await GetVersionsAsync();
                        break;
                }
            }
        }

        public static async Task LaunchGameAsync()
        {
            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = ServiceName;
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;

            Console.WriteLine("Openning App Service connection.");

            AppServiceConnectionStatus result = await connection.OpenAsync();
            if (result == AppServiceConnectionStatus.Success)
            {

                Console.WriteLine("App Service connection established");

                ValueSet valueSet = new ValueSet();
                valueSet.Add("type", "RequestLaunchMessage");

                Console.WriteLine("Sending launch message request to app");

                AppServiceResponse response = await connection.SendMessageAsync(valueSet);
                if (response.Status == AppServiceResponseStatus.Success)
                {

                    Console.WriteLine("Received response from app");

                    switch (response.Message["type"].ToString())
                    {
                        case "Launch":
                            {
                                string json = response.Message["message"].ToString();

                                Console.WriteLine("Launch message is :");
                                Console.WriteLine("     " + json);
                                Console.WriteLine("Deserializing launch message");

                                LaunchMessage message = null;
                                try
                                {
                                    message = JsonConvert.DeserializeObject<LaunchMessage>(json);
                                }
                                catch (JsonException e)
                                {
                                    Console.WriteLine("ERROR: " + e.Message);
                                    Console.WriteLine("     " + e.StackTrace);
                                }

                                if (message != null)
                                {
                                    Console.WriteLine("Ready to launch");

                                    LaunchResult launchResult = OneMinecraftLauncher.Core.OneMinecraftLauncher.Launch(message);

                                    if (launchResult.Success)
                                    {
                                        Console.WriteLine("Launch successfully");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Launch failed");
                                        Console.WriteLine("ERROR: " + launchResult.ErrorMessage);
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }

        public async static Task GetVersionsAsync()
        {
            ValueSet valueSet = null;
            string path = null;

            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = ServiceName;
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;

            Console.WriteLine("Openning App Service connection.");

            AppServiceConnectionStatus result = await connection.OpenAsync();
            if (result == AppServiceConnectionStatus.Success)
            {
                Console.WriteLine("App Service connection established");

                valueSet = new ValueSet();
                valueSet.Add("type", "RequestWorkDirForVersionsScan");

                Console.WriteLine("Sending work dir request to app");

                AppServiceResponse response = await connection.SendMessageAsync(valueSet);
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    path = response.Message["value"].ToString();
                }
                if (path == null)
                {
                    return;
                }

                Console.WriteLine("Received work dir message, work dir is " + path);
                Console.WriteLine("Creating KMCCC LauncherCore for versions scan");

                LaunchMessage message = new LaunchMessage { WorkDirPath = path };
                var core = OneMinecraftLauncher.Core.OneMinecraftLauncher.CreateLauncherCore(message);
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

                valueSet = new ValueSet();
                valueSet["type"] = "VersionsList";
                valueSet["value"] = json;

                Console.WriteLine("Sending versions list to app");
                
                //DONT await
                connection.SendMessageAsync(valueSet);
                
            }
            
        }
    }
}
