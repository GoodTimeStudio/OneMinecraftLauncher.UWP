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

            KMCCC.Launcher.Version ver = Program.Launcher.Core.GetVersion(versionID);

            if (ver == null)
                return null;

            ValueSet ret = new ValueSet();
            ret["client"] = ver.Downloads.Client.Url;
            ret["client-sha1"] = ver.Downloads.Client.SHA1;
            ret["server"] = ver.Downloads.Server.Url;
            ret["server-sha1"] = ver.Downloads.Server.SHA1;

            return ret;
        }
    }
}
