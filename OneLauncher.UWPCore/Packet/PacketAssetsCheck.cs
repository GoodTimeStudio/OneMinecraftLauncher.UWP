using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public class PacketAssetsCheck : PacketBase
    {

        public const string AssetIndexPath = @"{0}\assets\indexes\{1}.json";

        public override string GetTypeName()
        {
            return "assetsCheck";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet valueSet = new ValueSet();

            string versionId = args.Request.Message["version"].ToString();
            if (string.IsNullOrEmpty(versionId))
            {
                return null;
            }

            Logger.Info("Checking asset index for  # " + versionId);
            KMCCC.Launcher.Version ver = Program.Launcher.Core.GetVersion(versionId);
            if (ver == null)
            {
                Logger.ErrorFormat("Version {0} dose not exist", versionId);
                return null;
            }

            Logger.Info("AssetIndex: " + ver.Assets);
            var assetsResult = Program.Launcher.CheckAssets(ver);
            if (!assetsResult.hasValidIndex)
            {
                Logger.Warn("Asset index dose not exist or invalid");
                valueSet["index_url"] = ver.AssetsIndex.Url;
                valueSet["index_path"] = string.Format(@"assets\indexes\{0}.json", ver.Assets);
                return valueSet;
            }

            Logger.Info(string.Format("Found {0} missing assets", assetsResult.missingAssets.Count));
            string json = JsonConvert.SerializeObject(assetsResult.missingAssets);
            valueSet["missing_assets"] = json;
            return valueSet;
        }

    }

}
