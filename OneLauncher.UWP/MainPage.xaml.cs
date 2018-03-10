using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace GoodTimeStudio.OneMinecraftLauncher.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            setupTitleBar();
            InitializingPage.InitializationCompleteEvent += OnInitializationComplete;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (CoreManager.needInit == true)
            {
                MainContent.Navigate(typeof(InitializingPage));
                ViewModel.IsNavItemEnable = false;
            }
            else
            {
                MainContent.Navigate(typeof(StartPage));
            }
        }

        private async void OnInitializationComplete(object sender, EventArgs e)
        {
            CoreManager.needInit = false;
            await CoreManager.InitAppAsync();
            MainContent.Navigate(typeof(StartPage));
            ViewModel.IsNavItemEnable = true;
        }

        private void setupTitleBar()
        {
            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = Colors.Transparent;

                    //Light Theme or Dark Theme
                    var theme = (Window.Current.Content as FrameworkElement).ActualTheme;

                    if (theme == ElementTheme.Dark)
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                    }
                    else if (theme == ElementTheme.Light)
                    {
                        titleBar.ButtonForegroundColor = Colors.Black;
                    }
                    //titleBar.ButtonInactiveBackgroundColor
                }
            }
         }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (CoreManager.needInit)
            {
            }
            else
            {
                if (args.IsSettingsSelected)
                {
                    MainContent.Navigate(typeof(SettingsPage));
                }
                else
                {
                    NavigationViewItem item = args.SelectedItem as NavigationViewItem;
                    switch (item.Tag)
                    {
                        case "start":
                            MainContent.Navigate(typeof(StartPage));
                            break;
                        case "launch_options":
                            MainContent.Navigate(typeof(LaunchOptionsPage));
                            break;
                    }
                }
            }
        }
    }

}
