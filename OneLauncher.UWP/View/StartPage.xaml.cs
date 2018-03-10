using GoodTimeStudio.OneLauncher.UWP.Models;
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
            CoreManager.AppServiceEstablishedEvent += CoreManager_OnAppServiceEstablishedEvent;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //WebView.Navigate(new Uri("https://minecraft.net"));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            CoreManager.AppServiceEstablishedEvent -= CoreManager_OnAppServiceEstablishedEvent;
        }

        private void CoreManager_OnAppServiceEstablishedEvent(AppServiceConnection connection)
        {
            DebugWriteLine("App Service connection established");
            connection.RequestReceived += AppServiceConnection_OnRequestReceived;
        }

        private async void AppServiceConnection_OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            DebugWriteLine("Received request");

            var deferral = args.GetDeferral(); //keep the connection alive

            DebugWriteLine("Processing request message");

            if (args.Request.Message["type"].ToString() == "RequestLaunchMessage")
            {
                if (ViewModel.ListModel?.SelectedOption != null)
                {
                    DebugWriteLine("Generating launch message");

                    JAuthenticator auth = new JAuthenticator();
                    auth.type = CoreManager.AccountTypeTag;
                    auth.username = CoreManager.Username;

                    LaunchOption option = ViewModel.ListModel.SelectedOption;

                    string t = CoreManager.GlobalJVMPath;

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

                    string json;
                    try
                    {
                        json = JsonConvert.SerializeObject(message);
                    }
                    catch (JsonSerializationException e)
                    {
                        DebugWriteLine("ERROR: " + e.Message);

                        deferral.Complete();
                        return;
                    }

                    DebugWriteLine(json);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        DebugWriteLine("Sending response");

                        ValueSet valueSet = new ValueSet();
                        valueSet.Add("type", "Launch");
                        valueSet.Add("message", json);
                        await args.Request.SendResponseAsync(valueSet);
                    }
                }
                else
                {
                    DebugWriteLine("ERROR: No selected option");
                }

                deferral.Complete();
            }
        }

        private async void Button_Launch_Click(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Launch");
        }

        private static void DebugWriteLine(string str)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(str);
#endif
        }
    }
}
