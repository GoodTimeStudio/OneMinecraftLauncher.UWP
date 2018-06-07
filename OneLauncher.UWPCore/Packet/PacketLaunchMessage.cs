using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Launcher;
using log4net;
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
    public class PacketLaunchMessage : PacketBase
    {

        public override string GetTypeName()
        {
            return "launch";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (Program.Core == null)
                return null;

            string json = args.Request.Message["message"].ToString();

            Logger.Info("Launch message is :");
            Logger.Info("     " + json);
            Logger.Info("Deserializing launch message");

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

                Logger.Error("ERROR: " + e.Message);
                Logger.Error("     " + e.StackTrace);
            }

            if (message != null)
            {
                Logger.Info("Ready to launch");

                LaunchResult launchResult = OneMinecraftLauncher.Core.OneMinecraftLauncher.Launch(Program.Core, message);

                if (launchResult.Success)
                {
                    ret["result"] = true;

                    Logger.Info("Launch successfully");
                }
                else
                {
                    ret["errorMessage"] = launchResult.ErrorMessage;
                    ret["errorStack"] = launchResult.Exception?.StackTrace;

                    Logger.Warn("Launch failed: " + launchResult.ErrorMessage);
                }
            }

            Logger.Info("Sending launch result to app");
            return ret;
        }
    }
}
