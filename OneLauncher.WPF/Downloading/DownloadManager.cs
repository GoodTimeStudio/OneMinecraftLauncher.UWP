using GoodTimeStudio.OneMinecraftLauncher.Core.Downloading;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading
{
    public class DownloadManager : BindableBase
    {
        /// <summary>
        /// Maximum of the downloading item in the same time
        /// </summary>
        private static readonly int MaxDownloadingCount = 50;

        private ObservableCollection<DownloadItem> _list;
        public IDownloadSource Source;
        private bool isDownloading = false;

        private int _ItemCount;
        public int ItemCount
        {
            get => _ItemCount;
            set
            {
                _ItemCount = value;
                ProgressTextInCount = DownloadedItemCount + " / " + value;
            }
        }

        private int _DownloadedItemCount;
        public int DownloadedItemCount
        {
            get => _DownloadedItemCount;
            set
            {
                _DownloadedItemCount = value;
                ProgressTextInCount = value + " / " + ItemCount;
            }
        }

        private double _TotalProgress;
        public double TotalProgress
        {
            get => _TotalProgress;
            set => SetProperty(ref _TotalProgress, value);
        }

        private string _ProgressTextInCount;
        public string ProgressTextInCount
        {
            get => _ProgressTextInCount;
            set => SetProperty(ref _ProgressTextInCount, value);
        }

        public event EventHandler DownloadCompleted;

        public DownloadManager(ref ObservableCollection<DownloadItem> list, IDownloadSource source)
        {
            _list = list;
            Source = source;
            ItemCount = list.Count;
        }

        public void StartDownload()
        {
            if (!isDownloading)
            {
                Parallel.ForEach(_list, new ParallelOptions { MaxDegreeOfParallelism = MaxDownloadingCount }, Download);
            }
        }

        private void Download(DownloadItem item)
        {
            item.Start(this);
        }

        public void Cancel()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                _list[i].Cancel();
            }
        }

    }
}
