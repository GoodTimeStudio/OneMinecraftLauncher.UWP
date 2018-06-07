using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public class PacketVersionUrl : PacketBase
    {
        public override string GetTypeName()
        {
            return "version-url";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string versionID = args.Request.Message["version"].ToString();

            if (Program.Core == null)
                return null;

            KMCCC.Launcher.Version ver = Program.Core.GetVersion(versionID);

            if (ver == null)
                return null;

            ValueSet ret = new ValueSet();
            ret["client"] = ver.ClientJarUrl;
            ret["client-sha1"] = ver.ClientJarSHA1;
            ret["server"] = ver.ServerJarUrl;
            ret["server-sha1"] = ver.ServerJarSHA1;

            return ret;
        }
    }
}
