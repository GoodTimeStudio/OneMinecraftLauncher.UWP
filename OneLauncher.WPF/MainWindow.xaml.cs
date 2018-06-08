using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Authentication;
using KMCCC.Launcher;
using KMCCC.Modules.JVersion;
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

        private bool isLogin;

        public MainWindow()
        {
            InitializeComponent();

            LoadConfig();
        }

        public async void LoadConfig()
        {
            await Config.LoadFromFileAsync();

            Dispatcher.Invoke(() =>
            {
                _TB_UserName.Text = Config.INSTANCE.Username;
                _TB_JavaPath.Text = Config.INSTANCE.Javapath;
                _TB_MaxMemory.Text = Config.INSTANCE.Maxmemory.ToString();
            });
        }

        public void SaveConfig()
        {
            Config.INSTANCE.Username = _TB_UserName.Text;
            Config.INSTANCE.Javapath = _TB_JavaPath.Text;
            Config.INSTANCE.Maxmemory = int.Parse(_TB_MaxMemory.Text);
            Config.SaveConfigToFile();
        }

        private void _BTN_Launch_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            LoginScreen.Visibility = Visibility.Visible;
            _TB_LoginScreenTip.Text = "正在登陆...";

            LauncherCore core = LauncherCore.Create(new LauncherCoreCreationOption(
                    "./game",
                    Config.INSTANCE.Javapath,
                    new JVersionLocator()
                ));

            var version = core.GetVersion("1.10.2-forge1.10.2-12.18.3.2185");

            var options = new LaunchOptions()
            {
                Version = version,
                MaxMemory = int.Parse(_TB_MaxMemory.Text),
                Mode = LaunchMode.MCLauncher,
            };

            core.Launch(options);

            Dispatcher.InvokeShutdown();
        }
    }
}
