using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class DownloadDialogViewModel : BindableBase
    {

        private DownloadManager _manager;
        public DownloadManager manager
        {
            get => _manager;
            set => SetProperty(ref _manager, value);
        }

        private ObservableCollection<DownloadItem> _DownloadQuene;
        public ObservableCollection<DownloadItem> DownloadQuene
        {
            get => _DownloadQuene;
            set => SetProperty(ref _DownloadQuene, value);
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

    }
}
