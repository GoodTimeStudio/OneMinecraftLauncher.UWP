using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Downloading
{
    public class BMCLApiSource : IDownloadSource
    {
        private static readonly string _BMCL = "https://bmclapi.bangbang93.com";

        private static readonly string _LibrariesHost = "libraries.minecraft.net";
        private static readonly string _MetaHost = "launchermeta.mojang.com";
        private static readonly string _VersionHost = "launcher.mojang.com";
        private static readonly string _AssetHost = "resources.download.minecraft.net";

        private static readonly string _LibrariesBaseUrl_BMCL = _BMCL + "/libraries";
        private static readonly string _AssetsBaseUrl_BMCL = _BMCL + "/assets";

        public string SourceName => "BMCLAPI (https://bmclapi.bangbang93.com/)";
        public string Info => "注意：该下载源为第三方下载源";
        public string SourceID => "bmclapi";

        public Uri GetDownloadUrl(Uri sourceUri)
        {
            if (string.Equals(sourceUri.Host, _LibrariesHost))
            {
                return new Uri(_LibrariesBaseUrl_BMCL + sourceUri.LocalPath);
            }
            else if (string.Equals(sourceUri.Host, _MetaHost))
            {
                return new Uri(_BMCL + sourceUri.LocalPath);
            }
            else if (string.Equals(sourceUri.Host, _VersionHost))
            {
                return new Uri(_BMCL + sourceUri.LocalPath);
            }
            else if (string.Equals(sourceUri.Host, _AssetHost))
            {
                return new Uri(_AssetsBaseUrl_BMCL + sourceUri.LocalPath);
            }

            return sourceUri;
        }
    }
}
