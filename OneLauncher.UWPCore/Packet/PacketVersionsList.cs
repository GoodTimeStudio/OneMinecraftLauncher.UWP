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
    public class PacketVersionsList : IServicePacket
    {
        public string GetTypeName()
        {
            return "versionsList";
        }

        public ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (Program.Core == null)
                return null;

            List<string> list = new List<string>();
            var vers = Program.Core.GetVersions();

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
                return null;
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;

            Console.WriteLine("Versions List json: ");
            Console.WriteLine("     " + json);

            ValueSet valueSet = new ValueSet();
            valueSet["type"] = "versionsList";
            valueSet["value"] = json;

            Console.WriteLine("Sending versions list to app");

            return valueSet;
        }
    }
}
