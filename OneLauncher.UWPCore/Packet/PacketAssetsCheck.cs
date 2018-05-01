using KMCCC.Launcher;
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
    public class PacketAssetsCheck : IServicePacket
    {
        public const string AssetIndexPath = @"{0}\assets\indexes\{1}.json";

        public string GetTypeName()
        {
            return "assetsCheck";
        }

        public ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (Program.Core == null)
            {
                return null;
            }

            string versionId = args.Request.Message["version"].ToString();
            if (string.IsNullOrEmpty(versionId))
            {
                return null;
            }

            Console.WriteLine("Checking assets for  # " + versionId);

            KMCCC.Launcher.Version ver = Program.Core.GetVersion(versionId);
            if (ver == null)
            {
                return null;
            }

            string json = File.ReadAllText(string.Format(AssetIndexPath, Program.Core.GameRootPath, ver.Assets));
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

            List<JAsset> ret = new List<JAsset>();
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

                JAsset asset = new JAsset
                {
                    hash = _hash,
                    size = _size
                };

                if (!Program.Core.CheckFileHash(asset.GetPath(), asset.hash, sha1))
                {
                    ret.Add(asset);

                    Console.WriteLine("     Found missing asset: " + asset.hash);
                }
            }

            json = JsonConvert.SerializeObject(ret);
            ValueSet valueSet = new ValueSet();
            valueSet["value"] = json;
            return valueSet;
        }

        public class JAsset
        {
            public string hash;

            public int size;

            public string GetPath()
            {
                return string.Format(@"{0}\assets\objects\{1}\{2}", Program.Core.GameRootPath, hash.Substring(0, 2), hash);
            }

        }
    }

    public class PacketAssetIndexCheck : IServicePacket
    {
        public string GetTypeName()
        {
            return "assetIndexCheck";
        }

        public ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string versionID = args.Request.Message["version"].ToString();

            if (Program.Core == null)
                return null;

            KMCCC.Launcher.Version ver = Program.Core.GetVersion(versionID);

            if (ver == null)
                return null;

            if (Program.Core.CheckFileHash(string.Format(@"{0}\assets\indexes\{1}.json", Program.Core.GameRootPath, ver.Assets), ver.AssetIndexInfo.SHA1, new SHA1CryptoServiceProvider()))
                return null;

            ValueSet ret = new ValueSet();
            ret["path"] = string.Format(@"assets\indexes\{0}.json", ver.Assets);
            ret["url"] = ver.AssetIndexInfo.Url;

            return ret;
        }
    }
}
