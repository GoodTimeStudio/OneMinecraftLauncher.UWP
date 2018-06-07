using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
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
    public class PacketLibrariesCheck : PacketBase
    {

        public override string GetTypeName()
        {
            return "librariesCheck";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {

            string versionID = args.Request.Message["version"].ToString();
            if (string.IsNullOrWhiteSpace(versionID))
                return null;

            Logger.Info("Scanning libraries and natives");
            KMCCC.Launcher.Version ver = Program.Launcher.Core.GetVersion(versionID);

            List<MinecraftAssembly> missing = new List<MinecraftAssembly>();
            List<MinecraftAssembly> missingLib = Program.Launcher.CheckLibraries(ver);
            List<MinecraftAssembly> missingNative = Program.Launcher.CheckNatives(ver);

            Logger.Info("Found " + missingLib?.Count + " missing libraries");
            foreach (MinecraftAssembly lib in missingLib)
            {
                Logger.Warn("     # " + lib.Name);
            }

            Logger.Info("Found " + missingNative?.Count + " missing natives");
            foreach (MinecraftAssembly nav in missingNative)
            {
                Logger.Warn("     # " + nav.Name);
            }

            missing.AddRange(missingLib);
            missing.AddRange(missingNative);

            Logger.Info("Serializing list to json");
            string json = null;
            try
            {
                json = JsonConvert.SerializeObject(missing);
            }
            catch (JsonException)
            {
                return null;
            }

            ValueSet ret = new ValueSet();
            ret["value"] = json;

            Logger.Info("Sending list to app");
            return ret;
        }
    }
    
}
