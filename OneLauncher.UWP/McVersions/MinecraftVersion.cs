using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.McVersions
{
    public class MinecraftVersionManager
    {
        public static readonly string VersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public static readonly string VersionManifestFileName = "version_manifest.json";

        public static MinecraftVersionsList VersionsList;

        public async static Task<MinecraftVersionsList> GetMinecraftVersionsAsync()
        {
            IStorageItem item = await CoreManager.AppDir.TryGetItemAsync(VersionManifestFileName);
            StorageFile file = null;
            if (item is StorageFile)
            {
                file = item as StorageFile;
            }

            string json = null;
            if (file == null)
            {
                file = await CoreManager.AppDir.CreateFileAsync(VersionManifestFileName);
                json = await DownloadMinecraftVersionManifestAsync(file);
            }
            else
            {
                json = await FileIO.ReadTextAsync(file);
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                VersionsList = JsonConvert.DeserializeObject<MinecraftVersionsList>(json);
                return VersionsList;
            }
            catch (JsonException)
            {
                await file.DeleteAsync();
                return null;
            }

        }

        public static List<MinecraftVersion> GetReleaseVersionsList()
        {
            if (VersionsList != null)
            {
                return VersionsList.versions.Where(t => t.type == "release").ToList();
            }
            return null;
        }

        /// <summary>
        /// Downlaod minecraft version manifest to local storage.
        /// </summary>
        /// <returns>version manifest strings</returns>
        private async static Task<string> DownloadMinecraftVersionManifestAsync(StorageFile file)
        {
            if (file == null)
                return null;

            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(VersionManifestUrl);
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;
            
            await FileIO.WriteTextAsync(file, json);
            return json;
        }
    }

    public class MinecraftVersion
    {
        public string id;
        public string type;
        public DateTime time;
        public DateTime releaseTime;
        public string url;
    }

    public class MinecraftVersionsList
    {
        public LatestMinecraftVersionInfo latest;
        public List<MinecraftVersion> versions;
    }

    public class LatestMinecraftVersionInfo
    {
        public string release;
        public string snapshot;
    }
}
