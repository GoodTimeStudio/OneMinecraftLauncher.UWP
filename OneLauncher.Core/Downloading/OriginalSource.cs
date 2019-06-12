using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Downloading
{
    public class OriginalSource : IDownloadSource
    {
        public string SourceID => "original";
        public string SourceName => "官方";
        public string Info => string.Empty;

        public Uri GetDownloadUrl(Uri sourceUri)
        {
            return sourceUri;
        }

    }
}
