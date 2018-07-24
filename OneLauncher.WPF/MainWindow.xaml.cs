using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.View;
using KMCCC.Authentication;
using MahApps.Metro;
using MahApps.Metro.Controls;
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

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Current;

        private StartPage _StartPage;
        private DownloadPage _DownloadPage;
        private SettingsPage _SettingsPage;

        public MainWindow()
        {
            InitializeComponent();
            Current = this;
            TitleBlock.Text = Title;
            BackButtonVisable = false;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GoToPage(typeof(StartPage));
        }

        public bool BackButtonVisable
        {
            get
            {
                if (ViewModel.BackButtonVisibility == Visibility.Visible)
                {
                    return true;
                }
                return false;
            }
            set
            {
                if (value)
                {
                    ViewModel.BackButtonVisibility = Visibility.Visible;
                }
                else
                {
                    ViewModel.BackButtonVisibility = Visibility.Collapsed;
                }
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            GoToPage(typeof(StartPage));
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void Button_Min_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        public void GoToPage(Type typeOfPage)
        {
            if (typeOfPage == typeof(StartPage))
            {
                rootContent.Content = _StartPage ?? new StartPage();
                BackButtonVisable = false;
                return;
            }
            else if (typeOfPage == typeof(DownloadPage))
            {
                rootContent.Content = _DownloadPage ?? new DownloadPage();
            }
            else if (typeOfPage == typeof(SettingsPage))
            {
                rootContent.Content = _SettingsPage ?? new SettingsPage();
            }
            BackButtonVisable = true;
        }

    }
}
