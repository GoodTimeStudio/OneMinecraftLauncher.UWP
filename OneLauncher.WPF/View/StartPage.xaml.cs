using static GoodTimeStudio.OneMinecraftLauncher.WPF.CoreManager;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KMCCC.Authentication;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using System.IO;
using System.Net.Http;
using KMCCC.Launcher;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System.Collections.ObjectModel;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : UserControl
    {
        private bool isWorking;

        private ProfileSeletorDialog _ProfileDialog;
        private UserDialog _UserDialog;

        public StartPage()
        {
            InitializeComponent();
            Loaded += StartPage_Loaded;
            ViewModel.LaunchButtonContent = "启动";

            ViewModel.AccountTypesList = new ObservableCollection<AccountType>();
            AccountTypes.AllAccountTypes.ForEach(a => { ViewModel.AccountTypesList.Add(a); });

            //Init dialogs
            _ProfileDialog = new ProfileSeletorDialog(ViewModel);
            _UserDialog = new UserDialog(ViewModel);
        }

        private async void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Config.INSTANCE != null) // determine launcher have inited
            {
                ViewModel.VersionsList = VersionsList;

                if (!string.IsNullOrEmpty(Config.INSTANCE.SelectedVersion))
                {
                    foreach (KMCCC.Launcher.Version kver in ViewModel.VersionsList)
                    {
                        if (Equals(kver.Id, Config.INSTANCE.SelectedVersion))
                        {
                            ViewModel.SelectedVersion = kver;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Config.INSTANCE.AccountType))
                {
                    ViewModel.SelectedAccountType = AccountTypes.GetAccountTypeFromTag(Config.INSTANCE.AccountType);
                }

                ViewModel.User = Config.INSTANCE.User;
                ViewModel.PlayerName = Config.INSTANCE.Playername;

                if (CoreMCL.UserAuthenticator == null)
                {
                    ViewModel.UserDialogResultString = string.Empty;
                    bool offline = ViewModel.SelectedAccountType == AccountTypes.Offline;
                    if (offline)
                    {
                        ViewModel.SetupUserDialog(Models.UserDialogState.Offline);
                    }
                    else
                    {
                        ViewModel.SetupUserDialog(Models.UserDialogState.Logging);
                    }

                    AuthenticationInfo info = await Auth(ViewModel.SelectedAccountType, ViewModel.User, Config.INSTANCE.Password, true);
                    IAuthenticator authenticator = GenAuthenticatorFromAuthInfo(info);
                    if (authenticator != null)
                    {
                        CoreMCL.UserAuthenticator = authenticator;
                        ViewModel.PlayerName = info.DisplayName;
                        if (!offline)
                        {
                            ViewModel.SetupUserDialog(Models.UserDialogState.LoggedIn);
                        }
                    }
                    else
                    {
                        ViewModel.UserDialogResultString = "用户验证失败 \r\n " + info?.Error;
                        ViewModel.SetupUserDialog(Models.UserDialogState.Input);
                        await MainWindow.Current.ShowMetroDialogAsync(_UserDialog, DefaultDialogSettings);
                    }
                }
            }
        }

        private async Task<bool> ShowDownloadDialog(DownloadDialog dialog)
        {
            await MainWindow.Current.ShowMetroDialogAsync(dialog, DefaultDialogSettings);
            dialog.StartDownload();
            await dialog.WaitUntilUnloadedAsync();
            return dialog.Cancelled;
        }

        private async void Tile_Profile_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Current.ShowMetroDialogAsync(_ProfileDialog, DefaultDialogSettings);
        }

        private void Tile_Download_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.GoToPage(typeof(DownloadPage));
        }

        private void Tile_Settings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.GoToPage(typeof(SettingsPage));
        }

        private void Button_UserDialog_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.Current.ShowMessageAsync("Test", "asd");
            MainWindow.Current.ShowMetroDialogAsync(_UserDialog, DefaultDialogSettings);
        }

        private async void Tile_Launch_Click(object sender, RoutedEventArgs e)
        {
            if (isWorking)
            {
                return;
            }
            isWorking = true;
            await launch();
            ViewModel.LaunchButtonContent = "启动";
            isWorking = false;
        }

        private async Task launch()
        {
            Config.INSTANCE.User = ViewModel.User;
            Config.SaveConfigToFileAsync();
            KMCCC.Launcher.Version kver = ViewModel.SelectedVersion;
            if (kver == null)
            {
                await MainWindow.Current.ShowMessageAsync("启动失败", "版本未指定，请选择一个要启动的Minecraft版本");
                return;
            }
            if (CoreMCL.UserAuthenticator == null)
            {
                await MainWindow.Current.ShowMessageAsync("启动失败", "未指定用户，请前往账户设置选择要登入Minecraft的用户");
                return;
            }
            Option.versionId = kver.Id;
            Option.javaExt = Config.INSTANCE.JavaExt;
            Option.javaArgs = Config.INSTANCE.JavaArgs;
            if (Config.INSTANCE.MaxMemory > 0)
            {
                Option.javaArgs = string.Format("-Xmx{0}M {1}", Config.INSTANCE.MaxMemory, Option.javaArgs);
            }

            #region Check libraries and natives
            ViewModel.LaunchButtonContent = "正在检查核心文件...";

            List<MinecraftAssembly> missing = null;
            await Task.Run(() =>
            {
                missing = CoreMCL.CheckLibraries(kver);
                missing?.AddRange(CoreMCL.CheckNatives(kver));
            });
            if (missing?.Count > 0)
            {
                ViewModel.LaunchButtonContent = "正在下载核心文件...";
                DownloadDialog dialog = new DownloadDialog("正在下载运行Minecraft所需的文件...");
                missing.ForEach(lib =>
                {
                    if (Uri.TryCreate(lib.Url, UriKind.Absolute, out Uri uri))
                    {
                        DownloadItem item = new DownloadItem(lib.Name, lib.Path, uri);
                        dialog.DownloadQuene.Add(item);
                    }
                });
                dialog.StartDownload();
                if (await ShowDownloadDialog(dialog))
                {
                    return;
                }
            }
            #endregion

            // Check Assets
            ViewModel.LaunchButtonContent = "正在检查资源文件";
            if (!CheckAssetsIndex(kver))
            {
                ViewModel.LaunchButtonContent = "正在获取资源元数据";
                try
                {
                    await Task.Run(async () =>
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string json = await client.GetStringAsync(kver.AssetsIndex.Url);
                            string path = string.Format(@"{0}\assets\indexes\{1}.json", CoreMCL.Core.GameRootPath, kver.Assets);
                            FileInfo fileInfo = new FileInfo(path);
                            if (!fileInfo.Directory.Exists)
                            {
                                fileInfo.Directory.Create();
                            }
                            fileInfo.Create().Dispose();
                            File.WriteAllText(path, json);
                        }
                    });
                }
                catch (HttpRequestException ex)
                {
                    await MainWindow.Current.ShowMessageAsync("获取资源元数据失败", ex.Message + ex.StackTrace, MessageDialogStyle.Affirmative, DefaultDialogSettings);
                    return;
                }
                catch (IOException ex)
                {
                    await MainWindow.Current.ShowMessageAsync("获取资源元数据失败", ex.Message + ex.StackTrace, MessageDialogStyle.Affirmative, DefaultDialogSettings);
                    return;
                }
            }

            ViewModel.LaunchButtonContent = "正在检查资源文件...";
            (bool hasValidIndex, List<MinecraftAsset> missingAssets) assetsResult = (false, null);
            await Task.Run(() =>
            {
                assetsResult = CoreMCL.CheckAssets(kver);
            });
            if (!assetsResult.hasValidIndex)
            {
                await MainWindow.Current.ShowMessageAsync("获取资源元数据失败", "发生未知错误，无法获取有效的资源元数据，我们将为您继续启动游戏，但这可能会导致游戏中出现无翻译和无声音等问题");
            }
            else
            {
                if (assetsResult.missingAssets.Count > 0)
                {
                    DownloadDialog dialog = new DownloadDialog("正在下载资源文件...");
                    assetsResult.missingAssets.ForEach(ass =>
                    {
                        if (Uri.TryCreate(ass.GetDownloadUrl(), UriKind.Absolute, out Uri uri))
                        {
                            DownloadItem item = new DownloadItem("资源: " + ass.Hash, CoreMCL.Core.GameRootPath + "\\" + ass.GetPath(), uri);
                            dialog.DownloadQuene.Add(item);
                        }
                    });
                    if (await ShowDownloadDialog(dialog))
                    {
                        return;
                    }
                }
            }

            ViewModel.LaunchButtonContent = "正在启动...";
            LaunchResult result = CoreMCL.Launch(Option);
            if (!result.Success)
            {
                await MainWindow.Current.ShowMessageAsync("启动失败", result.ErrorMessage + "\r\n" + result.Exception);
            }
        }

        private bool CheckAssetsIndex(KMCCC.Launcher.Version kver)
        {
            var result = CoreMCL.CheckAssets(kver);
            return result.hasValidIndex;
        }

    }
}
