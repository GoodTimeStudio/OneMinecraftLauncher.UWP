using AltoHttp;
using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    public class CoreManager
    {
        public static OneMCL CoreMCL;
        public static LaunchOptionBase Option;

        public static Dictionary<string, KMCCC.Launcher.Version> VersionsIdMap;

        public static readonly MetroDialogSettings DefaultDialogSettings = new MetroDialogSettings
        {
            MaximumBodyHeight = 250
        };

        public static void Initialize()
        {
            CoreMCL = new OneMCL(@".\.minecraft");
            Option = new LaunchOptionBase("one-minecraft-launcher");
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
