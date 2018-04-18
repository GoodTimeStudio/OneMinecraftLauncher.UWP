using GoodTimeStudio.OneLauncher.UWP.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Minecraft;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public StartPage()
        {
            this.InitializeComponent();
            OptListView.OptListViewModel = CoreManager.OptListViewModel ?? new LaunchOptionListViewModel();
            this.ViewModel = new StartPageViewModel(OptListView.OptListViewModel);
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
            //Check connection to launch agent
            if (AppServiceManager.appServiceConnection != null)
            {
                if (ViewModel.ListModel?.SelectedOption != null)
                {
                    DebugWriteLine("Generating launch message");

                    JAuthenticator auth = new JAuthenticator();
                    auth.type = CoreManager.AccountTypeTag;
                    auth.username = CoreManager.Username;

                    LaunchOption option = ViewModel.ListModel.SelectedOption;

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

                        // Libraries and natives check 
                        ValueSet valueSet = new ValueSet();
                        valueSet["type"] = "librariesCheck";
                        valueSet["version"] = message.VersionId;
                        AppServiceResponse response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);

                        List<Library> missingLibs = null;
                        string responseJson = response.Message["value"].ToString();
                        try
                        {
                            missingLibs = JsonConvert.DeserializeObject<List<Library>>(responseJson);
                        }
                        catch (JsonException)
                        { }

                        //Found missing libs, go to download page.
                        if (missingLibs != null && missingLibs.Count > 0)
                        {
                            option.isDownloading = true;
                            missingLibs.ForEach(lib =>
                            {
                                DownloadItem item = new DownloadItem(lib.Name, lib.Path, lib.Url);
                                DownloadManager.DownloadQuene.Add(item);
                            });
                            DownloadManager.StartDownload( );
                            CoreManager.DownlaodPageModel.isPaneOpen = true;
                            await MainPage.Instance.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                MainPage.Instance.NavigateTo(MainPage.Tag_Download);
                            });

                            return;
                        }
                        
                        //No missing libs and natives, go through
                        valueSet = new ValueSet();
                        valueSet.Add("type", "launch");
                        valueSet.Add("message", messageJson);
                        response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);

                        //Display error
                        object obj = response.Message["result"];
                        if (obj is bool && !((bool) obj))
                        {
                            ViewModel.MsgBoxTitleText = CoreManager.GetStringFromResource("/StartPage/LaunchFailed");
                            ViewModel.MsgBoxContentText = response.Message["errorMessage"].ToString() + "\r\n" + response.Message["errorStack"];
                            await MsgBox.ShowAsync();
                        }
                    }

                }
                else
                {
                    DebugWriteLine("ERROR: No selected option");
                }
            }
        }

        private static void DebugWriteLine(string str)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(str);
#endif
        }

        private void MsgBox_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.MsgBoxTitleText = "";
            ViewModel.MsgBoxContentText = "";
        }
    }
}
