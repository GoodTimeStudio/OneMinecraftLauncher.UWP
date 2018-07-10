using AltoHttp;
using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    public class CoreManager
    {
        public static OneMCL CoreMCL;
        public static LaunchOptionBase Option;

        public static ObservableCollection<KMCCC.Launcher.Version> VersionsList;
        public static List<IDownloadSource> DownloadSourcesList;
        public static IDownloadSource DownloadSource;

        public static readonly MetroDialogSettings DefaultDialogSettings = new MetroDialogSettings
        {
            MaximumBodyHeight = 250
        };

        public static void Initialize()
        {
            CoreMCL = new OneMCL(@".\.minecraft");
            Option = new LaunchOptionBase("one-minecraft-launcher");

            Config.LoadFromFile();
            VersionsList = new ObservableCollection<KMCCC.Launcher.Version>();
            RefreshVersionsList(CoreMCL.Core.GetVersions());

            DownloadSourcesList = new List<IDownloadSource>();
            var tmp = new OriginalSource();
            DownloadSourcesList.Add(tmp);
            DownloadSourcesList.Add(new BMCLApiSource());
            DownloadSource = tmp;
            foreach (IDownloadSource source in DownloadSourcesList)
            {
                if (Equals(source.SourceID, Config.INSTANCE.DownloadSourceId))
                {
                    DownloadSource = source;
                }
            }
        }

        public static void RefreshVersionsList(IEnumerable<KMCCC.Launcher.Version> versions)
        {
            VersionsList.Clear();
            foreach (KMCCC.Launcher.Version kver in versions)
            {
                VersionsList.Add(kver);
            }
        }

        public static string GetDownloadSpeedFriendlyText(HttpDownloader downloader)
        {
            if (downloader == null && downloader.State != DownloadState.Downloading)
            {
                return string.Empty;
            }
            double speed = downloader.SpeedInBytes / 1024d;
            if (speed <= 1000)
            {
                return Math.Round(speed, 1) + " Kb/s";
            }
            else
            {
                return Math.Round(speed / 1024d, 2) + "Mb/s";
            }
        }
    }
}
