using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft
{
    public class MinecraftVersion
    {
        public const string Type_Release = "release";
        public const string Type_Snapshot = "snapshot";

        public string id { get; set; }
        public string type { get; set; }
        public DateTime time { get; set; }
        public DateTime releaseTime { get; set; }
        public string url { get; set; }

        public string GetPath()
        {
            return string.Format(@"versions\{0}\", id);
        }

        public string GetJsonPath()
        {
            return GetPath() + id + ".json";
        }

        public string GetJarPath()
        {
            return GetPath() + id + ".jar";
        }
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
