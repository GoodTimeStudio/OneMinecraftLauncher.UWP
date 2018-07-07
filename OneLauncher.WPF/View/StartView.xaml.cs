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
                ViewModel.VersionsList = CoreManager.CoreMCL.Core.GetVersions().ToList();
                CoreManager.VersionsIdMap = new Dictionary<string, KMCCC.Launcher.Version>();
                if (ViewModel.VersionsList == null)
                {
                    return;
                }
                foreach (KMCCC.Launcher.Version ver in ViewModel.VersionsList)
                {
                    CoreManager.VersionsIdMap.Add(ver.Id, ver);
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
                if (CoreManager.VersionsIdMap.TryGetValue(id, out KMCCC.Launcher.Version ver))
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

        private async void _BTN_Launch_Click(object sender, RoutedEventArgs e)
        {
            if (isWorking)
            {
                return;
            }
            isWorking = true;
            SaveConfig();
            KMCCC.Launcher.Version kver = _VerBox.SelectedItem as KMCCC.Launcher.Version;
            CoreManager.Option.versionId = kver.Id;
            CoreManager.Option.javaExt = ViewModel.JavaExt;
            CoreManager.Option.javaArgs = ViewModel.JavaArgs;
            if (ViewModel.MaxMemory > 0)
            {
                CoreManager.Option.javaArgs = string.Format("-Xmx{0}M {1}", ViewModel.MaxMemory, CoreManager.Option.javaArgs);
            }
            CoreManager.CoreMCL.UserAuthenticator = new OfflineAuthenticator(ViewModel.Username);

            DownloadDialog dialog = new DownloadDialog();

            // Check Libraries
            ViewModel.LaunchButtonContent = "正在检查核心文件...";
            List<MinecraftAssembly> missing = CoreManager.CoreMCL.CheckLibraries(kver);
            missing?.AddRange(CoreManager.CoreMCL.CheckNatives(kver));
            missing.ForEach(lib =>
            {
                if (Uri.TryCreate(lib.Url, UriKind.Absolute, out Uri uri))
                {
                    DownloadItem item = new DownloadItem
                    {
                        Name = lib.Name,
                        Path = lib.Path,
                        Uri = uri
                    };
                }
            });/*
            if (missing?.Count > 0)
            {
                await MainWindow.Instance.ShowMetroDialogAsync(dialog);
            }*/

            ViewModel.LaunchButtonContent = "正在检查资源文件...";
            var assetResult = CoreManager.CoreMCL.CheckAssets(kver);

            ViewModel.LaunchButtonContent = "正在启动...";
            CoreManager.CoreMCL.Launch(CoreManager.Option);

            ViewModel.LaunchButtonContent = "启动";
            isWorking = false;
            //Dispatcher.InvokeShutdown();
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
