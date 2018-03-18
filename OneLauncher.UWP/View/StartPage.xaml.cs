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

                    string json;
                    try
                    {
                        json = JsonConvert.SerializeObject(message);
                    }
                    catch (JsonSerializationException exp)
                    {
                        DebugWriteLine("ERROR: " + exp.Message);
                        return;
                    }

                    DebugWriteLine(json);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        DebugWriteLine("Sending response");

                        ValueSet valueSet = new ValueSet();
                        valueSet.Add("type", "launch");
                        valueSet.Add("message", json);
                        await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);
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
    }
}
