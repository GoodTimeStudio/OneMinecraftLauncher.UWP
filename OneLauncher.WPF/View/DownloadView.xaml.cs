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

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// DownloadView.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadView : UserControl
    {
        private static readonly string VersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public DownloadView()
        {
            InitializeComponent();
            Loaded += DownloadView_Loaded;
        }

        private async void DownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.isWorking = true;

            // Get minecraft version json 
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
                await MainWindow.Instance.ShowMessageAsync("获取版本数据失败", "无法从服务器获取元文件\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            catch (JsonException ex)
            {
                await MainWindow.Instance.ShowMessageAsync("获取版本数据失败", "未知错误\r\n" + ex.Message + "\r\n" + ex.StackTrace);
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
            _verList.IsEnabled = false;

            try
            {
                string jsonPath = version.GetJsonPath();
                using (HttpClient client = new HttpClient())
                {
                    File.WriteAllText(jsonPath, await client.GetStringAsync(version.url));   
                }

            }
            catch (IOException ex)
            {

            }
        }
    }
}
