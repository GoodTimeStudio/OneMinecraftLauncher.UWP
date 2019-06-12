using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Downloading
{
    public interface IDownloadSource
    {
        /// <summary>
        /// Internal name
        /// </summary>
        string SourceID { get; }

        /// <summary>
        /// Get source name
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Redirect download uri to third-party source
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        Uri GetDownloadUrl(Uri sourceUri);

        /// <summary>
        /// Infomation display in settings page when switch download source
        /// </summary>
        string Info { get; }
    }
}
