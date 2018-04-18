using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public class PacketLaunchMessage : IServicePacket
    {
        public string GetTypeName()
        {
            return "launch";
        }

        public ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (Program.Core == null)
                return null;

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

                LaunchResult launchResult = OneMinecraftLauncher.Core.OneMinecraftLauncher.Launch(Program.Core, message);

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
            return ret;
        }
    }
}
