using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft
{
    public class MinecraftVersion
    {
        public const string Type_Release = "release";
        public const string Type_Snapshot = "snapshot";

        public string id;
        public string type;
        public DateTime time;
        public DateTime releaseTime;
        public string url;

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
