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
    public class PacketVersionsList : PacketBase
    {

        public override string GetTypeName()
        {
            return "versionsList";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            List<string> list = new List<string>();
            var vers = Program.Launcher.Core.GetVersions();

            Logger.Info("Found " + vers.Count() + " versions: ");

            foreach (KMCCC.Launcher.Version ver in vers)
            {
                list.Add(ver.Id);

                Logger.Info("    # " + ver.Id);
            }

            Logger.Info("Serializing versions list to json");

            string json = null;
            try
            {
                json = JsonConvert.SerializeObject(list);
            }
            catch (JsonException)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;

            Logger.Info("Versions List json: ");
            Logger.Info("     " + json);

            ValueSet valueSet = new ValueSet();
            valueSet["type"] = "versionsList";
            valueSet["value"] = json;

            Logger.Info("Sending versions list to app");

            return valueSet;
        }
    }
}
