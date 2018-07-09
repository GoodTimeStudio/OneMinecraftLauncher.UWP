using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    public class DownloadManager : BindableBase
    {
        private ObservableCollection<DownloadItem> _list;
        private bool isDownloading = false;
        private int ItemCount;
        private int DownloadedItemCount;

        private string _CurrentSpeedText;
        public string CurrentSpeedText
        {
            get => _CurrentSpeedText;
            set => SetProperty(ref _CurrentSpeedText, value);
        }

        private double _TotalProgress;
        public double TotalProgress
        {
            get => _TotalProgress;
            set => SetProperty(ref _TotalProgress, value);
        }

        public event EventHandler DownloadCompleted;

        public DownloadManager(ref ObservableCollection<DownloadItem> list)
        {
            _list = list;
            ItemCount = list.Count;
        }

        public void StartDownload()
        {
            if (!isDownloading)
            {
                if (_list.Count > 0)
                {
                    DownloadItem item = _list.First();
                    item.Start(this);
                    isDownloading = true;
                }
            }
        }

        public void Cancel()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                DownloadItem item = _list[i];
                if (item.Operation.State == AltoHttp.DownloadState.Downloading)
                {
                    item.Operation.Cancel();
                    break;
                }
            }
        }

        public void DownloadNext(DownloadItem preItem, bool needRemove = true)
        {
            if (needRemove)
            {
                _list.Remove(preItem);
            }
            if (_list.Count == 0)
            {
                isDownloading = false;
                DownloadCompleted(this, new EventArgs());
                return;
            }
            for (int i = 0; i < _list.Count; i++)
            {
                DownloadedItemCount++;
                TotalProgress = DownloadedItemCount / (double) ItemCount * 100d;

                DownloadItem item = _list[i];
                // Default state is cancelled, even the download operation not start
                if (item.Operation.State == AltoHttp.DownloadState.Cancelled)
                {
                    item.Start(this);
                    break;
                }
            }
        }
    }
}
