using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Minecraft
{
    public class Asset
    {
        private const string Download_URL = "http://resources.download.minecraft.net/";

        [JsonProperty("hash")]
        public string Hash;

        [JsonProperty("size")]
        public int Size;

        public string GetPath()
        {
            return string.Format(@"assets\objects\{0}\{1}", Hash.Substring(0, 2), Hash);
        }

        public string GetDownloadUrl()
        {
            return string.Format(@"{0}{1}/{2}", Download_URL, Hash.Substring(0, 2), Hash);
        }

    }

}
