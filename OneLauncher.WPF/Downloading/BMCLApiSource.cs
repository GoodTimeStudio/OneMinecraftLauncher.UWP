using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading
{
    public class BMCLApiSource : IDownloadSource
    {
        private static readonly string _BMCL = "https://bmclapi.bangbang93.com/";

        private static readonly string _LibrariesHost = "libraries.minecraft.net";
        private static readonly string _LibrariesBaseUrl_BMCL = _BMCL + "libraries";

        public string SourceName => "BMCLAPI (https://bmclapi.bangbang93.com/)";
        public string Info => "注意：该下载源为第三方下载源";
        public string SourceID => "bmclapi";

        public Uri GetDownloadUrl(Uri sourceUri)
        {
            if (string.Equals(sourceUri.Host, _LibrariesHost))
            {
                return new Uri(_LibrariesBaseUrl_BMCL + sourceUri.LocalPath);
            }

            return sourceUri;
        }
    }
}
