using AltoHttp;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

using static GoodTimeStudio.OneMinecraftLauncher.WPF.CoreManager;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// DownloadView.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : UserControl
    {
        private static readonly string VersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        private bool firstLoad;

        public DownloadPage()
        {
            InitializeComponent();
            Loaded += DownloadView_Loaded;
            firstLoad = true;
        }

        private async void DownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!firstLoad)
            {
                return;
            }
            firstLoad = false;
            ViewModel.isWorking = true;
            try
            {
                string json;
                using (HttpClient client = new HttpClient())
                {
                     json = await client.GetStringAsync(VersionManifestUrl);
                }

                if (!string.IsNullOrEmpty(json))
                {
                    ViewModel.AllMinecraftVersions = JsonConvert.DeserializeObject<MinecraftVersionsList>(json);
                }
            }
            catch (HttpRequestException ex)
            {
                await MainWindow.Current.ShowMessageAsync("获取版本数据失败", "无法从服务器获取元文件" + ex.Message + ex.StackTrace, 
                    MessageDialogStyle.Affirmative, DefaultDialogSettings);
            }
            catch (JsonException ex)
            {
                await MainWindow.Current.ShowMessageAsync("获取版本数据失败", "未知错误" + ex.Message + ex.StackTrace,
                    MessageDialogStyle.Affirmative, DefaultDialogSettings);
            }

            ViewModel.isWorking = false;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            MinecraftVersion version = _verList.SelectedItem as MinecraftVersion;
            if (version == null)
            {
                return;
            }

            ViewModel.isWorking = true;

            try
            {
                string jsonPath = CoreManager.CoreMCL.Core.GameRootPath + "\\" + version.GetJsonPath();
                using (HttpClient client = new HttpClient())
                {
                    FileInfo file = new FileInfo(jsonPath);
                    if (!file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }
                    File.Create(jsonPath).Dispose();
                    File.WriteAllText(jsonPath, await client.GetStringAsync(version.url));

                    string jarPath;
                    KMCCC.Launcher.Version kver = CoreManager.CoreMCL.Core.GetVersion(version.id);
                    jarPath = CoreManager.CoreMCL.Core.GameRootPath + "\\" + version.GetJarPath();

                    file = new FileInfo(jarPath);
                    if (!file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }
                    File.Create(jarPath);

                    HttpDownloader downloader = new HttpDownloader(kver.Downloads.Client.Url, jarPath);
                    var progressController = await MainWindow.Current.ShowProgressAsync("正在下载:  " + kver.Id, "", true, DefaultDialogSettings);
                    downloader.DownloadProgressChanged += async delegate
                    {
                        if (downloader.ProgressInPercent == 100)
                        {
                            await progressController.CloseAsync();
                        }
                        else
                        {
                            progressController.SetProgress(downloader.ProgressInPercent / 100);
                            progressController.SetMessage("下载速度: " + GetDownloadSpeedFriendlyText(downloader));
                        }
                    };
                    progressController.Canceled += delegate
                    {
                        if (downloader.State != DownloadState.Completed)
                        {
                            downloader.Cancel();
                        }
                    };
                    downloader.Start();
                }

            }
            catch (IOException ex)
            {
                await MainWindow.Current.ShowMessageAsync("下载失败", ex.Message + ex.StackTrace);
            }
            catch (HttpRequestException ex)
            {
                await MainWindow.Current.ShowMessageAsync("下载失败", ex.Message + ex.StackTrace);
            }

            ViewModel.isWorking = false;
        }

    }
}
