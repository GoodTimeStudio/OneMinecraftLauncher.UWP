using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
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

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// SettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsView_Loaded;
            Unloaded += SettingsView_Unloaded;
        }

        private void SettingsView_Unloaded(object sender, RoutedEventArgs e)
        {
            Config.SaveConfigToFile();
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Config.INSTANCE != null)
            {
                ViewModel.JavaExt = Config.INSTANCE.JavaExt;
                ViewModel.JavaArgs = Config.INSTANCE.JavaArgs;
                ViewModel.MaxMemory = Config.INSTANCE.MaxMemory;
                ViewModel.DownloadSourcesList = CoreManager.DownloadSourcesList;
                ViewModel.DownloadSource = CoreManager.DownloadSource;
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
