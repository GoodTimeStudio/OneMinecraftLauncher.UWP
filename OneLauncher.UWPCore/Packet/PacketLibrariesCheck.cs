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
    public class PacketLibrariesCheck : IServicePacket
    {
        public string GetTypeName()
        {
            return "librariesCheck";
        }

        public ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (Program.Core == null)
                return null;

            string versionID = args.Request.Message["version"].ToString();
            if (string.IsNullOrWhiteSpace(versionID))
                return null;

            Console.WriteLine("Scanning libraries and natives");
            KMCCC.Launcher.Version ver = Program.Core.GetVersion(versionID);

            List<DLibrary> missing = new List<DLibrary>();
            List<Library> missingLib = Program.Core.CheckLibraries(ver);
            List<Native> missingNative = Program.Core.CheckNatives(ver);

            Console.WriteLine("Found " + missingLib?.Count + " missing libraries");
            foreach (Library lib in missingLib)
            {
                string dName = lib.Url.Substring(lib.Url.LastIndexOf('/') + 1);
                Console.WriteLine("     # " + dName);
                missing.Add(new DLibrary
                {
                    Name = dName,
                    Path = Program.Core.GetLibPath(lib),
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
                    Path = Program.Core.GetNativePath(nav),
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
                return null;
            }

            ValueSet ret = new ValueSet();
            ret["value"] = json;

            Console.WriteLine("Sending list to app");
            return ret;
        }
    }
    
}
