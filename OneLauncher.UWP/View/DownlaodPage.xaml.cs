using GoodTimeStudio.OneMinecraftLauncher.UWP.Minecraft;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadPage : Page
    {

        public DownlaodPageViewModel ViewModel;

        private MsgDialog dialog;

        public DownloadPage()
        {
            this.InitializeComponent();
            this.ViewModel = CoreManager.DownlaodPageModel;
            dialog = new MsgDialog();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel.DownloadQuene.Count > 0)
            {
                ViewModel.isPaneOpen = true;
            }

            ViewModel.MakeList();
        }

        private void BtnClick_DownloadList(object sender, RoutedEventArgs e)
        {
            ViewModel.isPaneOpen = true;
        }

        private void ToggleSwitch_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel.MakeList();
        }

        private async void BtnClick_MinecraftDownload(object sender, RoutedEventArgs e)
        {
            Btn_MinecraftDownload.IsEnabled = false;
            using (HttpClient client = new HttpClient())
            {

                try
                {
                    MinecraftVersion ver = VersionsList.SelectedItem as MinecraftVersion;
                    if (ver == null)
                    {
                        return;
                    }
                    string json = await client.GetStringAsync(ver.url);
                    string t = ver.GetJsonPath();
                    StorageFile file = await CoreManager.WorkDir.CreateFileAsync(ver.GetJsonPath(), CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, json);

                    ValueSet valueSet = new ValueSet();
                    valueSet["type"] = "version-url";
                    valueSet["version"] = ver.id;

                    AppServiceResponse response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);
                    string url = response.Message["client"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        DownloadItem item = new DownloadItem("Minecraft" + ver.id, ver.GetJarPath(), url);
                        DownloadManager.DownloadQuene.Add(item);
                        DownloadManager.StartDownload();
                        ViewModel.isPaneOpen = true;
                    }

                }
                catch (HttpRequestException exp)
                {
                    await dialog.Show(CoreManager.GetStringFromResource("/DownloadPage/DownloadFailed"), exp.Message + exp.StackTrace);
                }
                catch (IOException exp)
                {
                    await dialog.Show(CoreManager.GetStringFromResource("/DownloadPage/DownloadFailed"), exp.Message + exp.StackTrace);
                }
                finally
                {
                    Btn_MinecraftDownload.IsEnabled = true;
                }

            }
        }
    }
}
