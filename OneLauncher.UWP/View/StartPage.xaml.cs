using GoodTimeStudio.OneLauncher.UWP.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Minecraft;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class StartPage : Page
    {
        public StartPageViewModel ViewModel;
        public INewsSource NewsSource;

        private MsgDialog _msgDialog;

        public StartPage()
        {
            this.InitializeComponent();
            OptListView.OptListViewModel = CoreManager.OptListViewModel ?? new LaunchOptionListViewModel();
            ViewModel = new StartPageViewModel(OptListView.OptListViewModel);
            _msgDialog = new MsgDialog();
            NewsSource = new OfficialNews();
            GetNews();
        }

        private async void GetNews()
        {
            ViewModel.NewsList = await NewsSource.GetNewsAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //WebView.Navigate(new Uri("https://minecraft.net"));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void Button_Launch_Click(object sender, RoutedEventArgs e)
        {
            LaunchOption option = ViewModel.ListModel.SelectedOption;
            if (option == null)
            {
                return;
            }

            option.isReady = false;
            option.lastUsed = DateTime.Now;
            await Launch(option);
            option.isReady = true;
            await CoreManager.SaveOptionList(ViewModel.ListModel.OptionList);
        }

        private async Task Launch(LaunchOption option)
        {
            //Check connection to launch agent
            if (AppServiceManager.appServiceConnection == null)
            {
                return;
            }

            List<Library> missingLibs = null;  //include missing natives
            List<Asset> missingAssets = new List<Asset>();

            #region Libraries and natives check
            ValueSet valueSet = new ValueSet();
            valueSet["type"] = "librariesCheck";
            valueSet["version"] = option.lastVersionId;
            AppServiceResponse response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);

            string responseJson = response.Message["value"].ToString();
            try
            {
                missingLibs = JsonConvert.DeserializeObject<List<Library>>(responseJson);
            }
            catch (JsonException)
            { }
            #endregion

            #region Assets check
            valueSet = new ValueSet();
            valueSet["type"] = "assetIndexCheck";
            valueSet["version"] = option.lastVersionId;
            response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);

            object obj = null;
            response.Message.TryGetValue("path", out obj);
            if (obj != null)
            {
                string path = obj.ToString();
                string url = response.Message["url"].ToString();

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string json = await client.GetStringAsync(url);
                        StorageFile file = await CoreManager.WorkDir.CreateFileAsync(path, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, json);
                    }
                }
                catch (Exception e)
                {
                    await _msgDialog.Show(
                        CoreManager.GetStringFromResource("/StartPage/LaunchFailed"),
                        "Cannot fetch asset index \r\n " + e.Message + "\r\n" + e.StackTrace
                        );
                    return;
                }
            }

            valueSet = new ValueSet();
            valueSet["type"] = "assetsCheck";
            valueSet["version"] = option.lastVersionId;
            response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);

            responseJson = response.Message["value"].ToString();
            try
            {
                missingAssets = JsonConvert.DeserializeObject<List<Asset>>(responseJson);
            }
            catch (JsonException) { }
            #endregion

            //Found missing libs, go to download.
            if ((missingLibs != null && missingLibs.Count > 0) || (missingAssets != null && missingAssets.Count > 0))
            {
                missingLibs.ForEach(lib =>
                {
                    DownloadItem item = new DownloadItem(lib.Name, lib.Path, lib.Url);
                    DownloadManager.DownloadQuene.Add(item);
                });
                missingAssets.ForEach(ass =>
                {
                    DownloadItem item = new DownloadItem(
                            string.Format("{0}: {1}", CoreManager.GetStringFromResource("/Resources/Asset"), ass.Hash),
                            ass.GetPath(),
                            ass.GetDownloadUrl()
                        );
                    DownloadManager.DownloadQuene.Add(item);
                });

                DownloadManager.StartDownload();
                await DownloadDialog.ShowAsync();

                return;
            }

            DebugWriteLine("Generating launch message");

            JAuthenticator auth = new JAuthenticator();
            auth.type = CoreManager.AccountTypeTag;
            auth.username = CoreManager.Username;

            LaunchMessage message = new LaunchMessage()
            {
                VersionId = option.lastVersionId,
                authenticator = auth,
                WorkDirPath = CoreManager.WorkDir?.Path,
                GameDirPath = string.IsNullOrWhiteSpace(option.gameDir) ? CoreManager.WorkDir?.Path : option.gameDir,
                JavaExtPath = string.IsNullOrWhiteSpace(option.javaExt) ? CoreManager.GlobalJVMPath : option.javaExt,
                JavaArgs = option.javaArgs
            };

            DebugWriteLine("Serializing launch message to json");

            string messageJson;
            try
            {
                messageJson = JsonConvert.SerializeObject(message);
            }
            catch (JsonSerializationException exp)
            {
                DebugWriteLine("ERROR: " + exp.Message);
                return;
            }

            DebugWriteLine(messageJson);

            //Check if the launch message was successfully generated
            if (!string.IsNullOrWhiteSpace(messageJson))
            {
                valueSet = new ValueSet();
                valueSet.Add("type", "launch");
                valueSet.Add("message", messageJson);
                response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);

                //Display error
                obj = response.Message["result"];
                if (obj is bool && !((bool)obj))
                {
                    await _msgDialog.Show(
                        CoreManager.GetStringFromResource("/StartPage/LaunchFailed"), 
                        response.Message["errorMessage"].ToString() + "\r\n" + response.Message["errorStack"]
                        );
                }
            }

        }

        private static void DebugWriteLine(string str)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(str);
#endif
        }

        private void DownloadDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DownloadManager.CancelAllDownload();
        }
    }
}
