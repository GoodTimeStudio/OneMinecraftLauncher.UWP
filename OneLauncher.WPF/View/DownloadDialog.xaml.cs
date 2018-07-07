using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using MahApps.Metro.Controls.Dialogs;
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
    /// DownloadDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadDialog : CustomDialog
    {
        public DownloadDialog()
        {
            InitializeComponent();
        }

        public void StartDownload()
        {
            CoreManager.Downloader.Clear();
            foreach (DownloadItem item in ViewModel.DownloadQuene)
            {
                CoreManager.Downloader.Add(item.Uri.ToString(), item.Path);
            }
            CoreManager.Downloader.StartAsync();
        }
    }
}
