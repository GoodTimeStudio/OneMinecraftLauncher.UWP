using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using KMCCC.Launcher;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
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
            string versionId = args.Request.Message["version"].ToString();
            if (string.IsNullOrEmpty(versionId))
            {
                return null;
            }

            Logger.Info("Checking assets for  # " + versionId);

            KMCCC.Launcher.Version ver = Program.Launcher.Core.GetVersion(versionId);
            if (ver == null)
            {
                return null;
            }

            Logger.Info("AssetIndex: " + ver.Assets);

            string json = File.ReadAllText(string.Format(AssetIndexPath, Program.Launcher.Core.GameRootPath, ver.Assets));
            JObject rootObj = null;
            try
            {
                rootObj = JObject.Parse(json)["objects"]?.ToObject<JObject>();
            }
            catch (JsonException)
            {
                return null;
            }

            if (rootObj == null)
            {
                return null;
            }

            List<MinecraftAsset> ret = new List<MinecraftAsset>();
            var sha1 = new SHA1CryptoServiceProvider();
            foreach (KeyValuePair<string, JToken> prop in rootObj)
            {
                string _hash = prop.Value["hash"].ToString();
                if (string.IsNullOrWhiteSpace(_hash))
                {
                    continue;
                }

                int _size;
                int.TryParse(prop.Value["size"].ToString(), out _size);

                MinecraftAsset asset = new MinecraftAsset
                {
                    Hash = _hash,
                    Size = _size
                };

                if (!Program.Launcher.Core.CheckFileHash(asset.GetPath(), asset.Hash, sha1))
                {
                    ret.Add(asset);

                    Logger.Warn("     Found missing asset: " + asset.Hash);
                }
            }

            Logger.Info(string.Format("Found {0} missing assets", ret.Count));

            json = JsonConvert.SerializeObject(ret);
            ValueSet valueSet = new ValueSet();
            valueSet["value"] = json;
            return valueSet;
        }

    }

    public class PacketAssetIndexCheck : PacketBase
    {
        public override string GetTypeName()
        {
            return "assetIndexCheck";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string versionID = args.Request.Message["version"].ToString();

            KMCCC.Launcher.Version ver = Program.Launcher.Core.GetVersion(versionID);

            if (ver == null)
                return null;

            if (Program.Launcher.Core.CheckFileHash(string.Format(@"{0}\assets\indexes\{1}.json", Program.Launcher.Core.GameRootPath, ver.Assets), ver.AssetIndexInfo.SHA1, new SHA1CryptoServiceProvider()))
                return null;

            ValueSet ret = new ValueSet();
            ret["path"] = string.Format(@"assets\indexes\{0}.json", ver.Assets);
            ret["url"] = ver.AssetIndexInfo.Url;

            return ret;
        }
    }
}
