using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{
    public class DownlaodPageViewModel : BindableBase
    {

        public ObservableCollection<DownloadItem> DownloadQuene;

        private bool _isPaneOpen;
        public bool isPaneOpen
        {
            get => _isPaneOpen;
            set => this.SetProperty(ref _isPaneOpen, value);
        }

        public DownlaodPageViewModel()
        {
            DownloadQuene = DownloadManager.DownloadQuene;
        }
    }

}
