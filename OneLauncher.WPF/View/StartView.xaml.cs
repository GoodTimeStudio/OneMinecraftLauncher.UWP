using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using MahApps.Metro.Controls.Dialogs;
using KMCCC.Authentication;
using Microsoft.Win32;
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
using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;

using static GoodTimeStudio.OneMinecraftLauncher.WPF.CoreManager;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.IO;
using KMCCC.Launcher;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// StartView.xaml 的交互逻辑
    /// </summary>
    public partial class StartView : UserControl
    {
        private bool isWorking;

        public StartView()
        {
            InitializeComponent();
            LoadConfig();
            ViewModel.LaunchButtonContent = "启动";
        }

        public async void LoadConfig()
        {
            await Task.Run(() =>
            {
                ViewModel.VersionsList = CoreMCL.Core.GetVersions().ToList();
                VersionsIdMap = new Dictionary<string, KMCCC.Launcher.Version>();
                if (ViewModel.VersionsList == null)
                {
                    return;
                }
                foreach (KMCCC.Launcher.Version ver in ViewModel.VersionsList)
                {
                    VersionsIdMap.Add(ver.Id, ver);
                }
            });
            await Config.LoadFromFileAsync();

            ViewModel.Username = Config.INSTANCE.Username;
            ViewModel.JavaExt = Config.INSTANCE.JavaExt;
            ViewModel.JavaArgs = Config.INSTANCE.JavaArgs;
            ViewModel.MaxMemory = Config.INSTANCE.MaxMemory;

            string id = Config.INSTANCE.SelectedVersion;
            if (!string.IsNullOrWhiteSpace(id))
            {
                if (VersionsIdMap.TryGetValue(id, out KMCCC.Launcher.Version ver))
                {
                    Dispatcher.Invoke(() => _VerBox.SelectedItem = ver);
                }
            }

        }

        public void SaveConfig()
        {
            Config.INSTANCE.Username = ViewModel.Username;
            Config.INSTANCE.JavaExt = ViewModel.JavaExt;
            Config.INSTANCE.JavaArgs = ViewModel.JavaArgs;
            Config.INSTANCE.MaxMemory = ViewModel.MaxMemory;

            Config.SaveConfigToFile();
        }

        private async Task<bool> ShowDownloadDialog(DownloadDialog dialog)
        {
            await MainWindow.Instance.ShowMetroDialogAsync(dialog, DefaultDialogSettings);
            await dialog.WaitUntilUnloadedAsync();
            return dialog.Cancelled;
        }

        private async void _BTN_Launch_Click(object sender, RoutedEventArgs e)
        {
            if (isWorking)
            {
                return;
            }
            isWorking = true;
            SaveConfig();
            KMCCC.Launcher.Version kver = _VerBox.SelectedItem as KMCCC.Launcher.Version;
            Option.versionId = kver.Id;
            Option.javaExt = ViewModel.JavaExt;
            Option.javaArgs = ViewModel.JavaArgs;
            if (ViewModel.MaxMemory > 0)
            {
                Option.javaArgs = string.Format("-Xmx{0}M {1}", ViewModel.MaxMemory, Option.javaArgs);
            }
            CoreMCL.UserAuthenticator = new OfflineAuthenticator(ViewModel.Username);
            

            // Check Libraries and Natives
            ViewModel.LaunchButtonContent = "正在检查核心文件...";
            List<MinecraftAssembly> missing = CoreMCL.CheckLibraries(kver);
            missing?.AddRange(CoreMCL.CheckNatives(kver));
            
            if (missing?.Count > 0)
            {
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
                if (!await ShowDownloadDialog(dialog))
                {
                    return;
                }
            }

            // Check Assets
            ViewModel.LaunchButtonContent = "正在检查资源文件";
            if (!CheckAssetsIndex(kver))
            {
                ViewModel.LaunchButtonContent = "正在获取资源元数据";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string json = await client.GetStringAsync(kver.AssetIndexInfo.Url);
                        string path= string.Format(@"{0}\assets\indexes\{1}.json", CoreMCL.Core.GameRootPath, kver.Assets);
                        FileInfo fileInfo = new FileInfo(path);
                        if (!fileInfo.Directory.Exists)
                        {
                            fileInfo.Directory.Create();
                        }
                        fileInfo.Create().Dispose();
                        File.WriteAllText(path, json);
                    }
                }
                catch (HttpRequestException ex)
                {
                    await MainWindow.Instance.ShowMessageAsync("获取资源元数据失败", ex.Message + ex.StackTrace, MessageDialogStyle.Affirmative, DefaultDialogSettings);
                    ViewModel.LaunchButtonContent = "启动";
                    return;
                }
                catch (IOException ex)
                {
                    await MainWindow.Instance.ShowMessageAsync("获取资源元数据失败", ex.Message + ex.StackTrace, MessageDialogStyle.Affirmative, DefaultDialogSettings);
                    ViewModel.LaunchButtonContent = "启动";
                    return;
                }
            }
            ViewModel.LaunchButtonContent = "正在检查资源文件...";
            var assetsResult = CoreMCL.CheckAssets(kver);
            if (!assetsResult.hasValidIndex)
            {
                ViewModel.LaunchButtonContent = "启动";
                await MainWindow.Instance.ShowMessageAsync("获取资源元数据失败", "发生未知错误，无法获取有效的资源元数据，我们将为您继续启动游戏，但这可能会导致游戏中出现无翻译和无声音等问题");
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
                    dialog.StartDownload();
                    if (!await ShowDownloadDialog(dialog))
                    {
                        return;
                    }
                }
            }

            ViewModel.LaunchButtonContent = "正在启动...";
            LaunchResult result = CoreMCL.Launch(Option);
            if (!result.Success)
            {
                await MainWindow.Instance.ShowMessageAsync("启动失败", result.ErrorMessage + "\r\n" + result.Exception);
            }

            ViewModel.LaunchButtonContent = "启动";
            isWorking = false;
            //Dispatcher.InvokeShutdown();
        }

        private bool CheckAssetsIndex(KMCCC.Launcher.Version kver)
        {
            var result = CoreMCL.CheckAssets(kver);
            return result.hasValidIndex;
        }

        private void _VerBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?[0] is KMCCC.Launcher.Version)
            {
                Config.INSTANCE.SelectedVersion = (e.AddedItems[0] as KMCCC.Launcher.Version).Id;
                Config.SaveConfigToFile();
            }
        }

        private void _BTN_JavaExtPicker_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = "javaw";
            fileDialog.DefaultExt = ".exe";
            fileDialog.Filter = "Java Runtime|javaw.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*";

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                ViewModel.JavaExt = fileDialog.FileName;
            }
        }
    }
}
