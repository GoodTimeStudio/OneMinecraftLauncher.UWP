using AltoHttp;
using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Downloading;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using KMCCC.Authentication;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

            AccountTypes.Mojang.Text = "Mojang账号（正版登陆）";
            AccountTypes.Offline.Text = "离线模式";

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

        public static async Task<AuthenticationInfo> Auth(AccountType type, string user, string password = "", bool refresh = false)
        {
            if (type == AccountTypes.Mojang)
            {
                Guid clientToken;
                if (!Guid.TryParse(Config.INSTANCE.ClientToken, out clientToken))
                {
                    clientToken = Guid.NewGuid();
                    Config.INSTANCE.ClientToken = clientToken.ToString();
                }

                CancellationToken ct = new CancellationToken();
                AuthenticationInfo info = null;
                if (refresh && Guid.TryParse(password, out Guid access))
                {
                    if (Guid.TryParse(Config.INSTANCE.UUID, out Guid uuid))
                    {
                        info = await new YggdrasilValidate(access, clientToken, uuid, Config.INSTANCE.Playername).DoAsync(ct);
                    }
                    if (info == null || !string.IsNullOrEmpty(info.Error))
                    {
                        info = await new YggdrasilRefresh(access, false).DoAsync(ct);
                    }
                }
                else
                {
                    info = await new YggdrasilLogin(user, password, false, clientToken).DoAsync(ct);
                }
                return info;
            }
            else if (type == AccountTypes.Offline)
            {
                OfflineAuthenticator offline = new OfflineAuthenticator(user);
                return offline.Do();
            }
            return null;
        }

        public static IAuthenticator GenAuthenticatorFromAuthInfo(AuthenticationInfo info)
        {
            if (info == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(info.Error))
            {
                return new WarpedAuhenticator(info);
            }
            return null;
        }
    }
}
