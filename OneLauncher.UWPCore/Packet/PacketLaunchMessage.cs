using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Authentication;
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
            string auth_type = args.Request.Message["auth_type"].ToString();
            string auth_username = args.Request.Message["auth_username"].ToString();
            Logger.Info("User type: " + auth_type);
            Logger.Info("Username: " + auth_username);
            switch (auth_type)
            {
                case "offline":
                    Program.Launcher.UserAuthenticator = new OfflineAuthenticator(auth_username);
                    break;
                //case "mojang":
            }
            if (Program.Launcher.UserAuthenticator == null)
            {
                Logger.Error("User authenticator no set or passed a wrong auth massage");
                return null;
            }

            string json = args.Request.Message["launch_option"].ToString();

            Logger.Info("LaunchOption: " + json);
            Logger.Info("Deserializing launch option");

            LaunchOptionBase launchOption = null;
            ValueSet ret = new ValueSet();
            ret["result"] = false;

            try
            {
                launchOption = JsonConvert.DeserializeObject<LaunchOptionBase>(json);
            }
            catch (JsonException e)
            {
                ret["errorMessage"] = e.Message;
                ret["errorStack"] = e.StackTrace;

                Logger.Error("ERROR: " + e.Message);
                Logger.Error("     " + e.StackTrace);
            }

            if (launchOption != null)
            {
                Logger.Info("Ready to launch");

                LaunchResult launchResult = Program.Launcher.Launch(launchOption);

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
