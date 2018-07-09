using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private DownloadManager manager;
        public ObservableCollection<DownloadItem> DownloadQuene;

        public bool Cancelled { get; set; }

        public DownloadDialog()
        {
            InitializeComponent();
            DownloadQuene = new ObservableCollection<DownloadItem>();
            ViewModel.DownloadQuene = DownloadQuene;
        }

        public DownloadDialog(string title) : this()
        {
            ViewModel.Title = title;
        }

        public void StartDownload()
        {
            manager = new DownloadManager(ref DownloadQuene);
            ViewModel.manager = manager;
            manager.DownloadCompleted += Manager_DownloadCompleted;
            manager.StartDownload();
        }

        private void Manager_DownloadCompleted(object sender, EventArgs e)
        {
            DialogManager.HideMetroDialogAsync(MainWindow.Instance, this);
        }

        public void CancelDownload()
        {
            if (manager != null)
            {
                manager.Cancel();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelDownload();
            MainWindow.Instance.HideMetroDialogAsync(this);
        }
    }
}
