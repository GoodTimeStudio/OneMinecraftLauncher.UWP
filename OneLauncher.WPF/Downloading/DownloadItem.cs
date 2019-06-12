using AltoHttp;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading
{
    public class DownloadItem : BindableBase
    {
        private DownloadManager _manager;

        private Timer _timer;

        public string Name { get; set; }

        public string Path { get; set; }

        public Uri Uri { get; set; }

        public HttpDownloader Operation { get; set; }

        public ItemState State { get; set; }

        public DownloadItem(string name, string path, Uri uri)
        {
            Name = name;
            Path = path;
            Uri = uri;
            Progress = 0;
            Operation = new HttpDownloader(uri.AbsoluteUri, path);
            State = ItemState.Standby;
            _timer = new Timer(100);
            _timer.Elapsed += _timer_Elapsed;
        }

        public void Start(DownloadManager manager)
        {
            if (State != ItemState.Standby)
            {
                return;
            }

            State = ItemState.Downloading;
            Console.WriteLine("Attempt to download: " + Name);


            _manager = manager;
            if (_manager.Source != null)
            {
                Uri = _manager.Source.GetDownloadUrl(Uri);
            }
            FileInfo file = new FileInfo(Path);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            File.Create(Path).Dispose();
            _timer.Start();
            Operation.Start();

            Console.WriteLine("Download completed: " + Name);
        }

        public void Cancel()
        {
            if (Operation.State == DownloadState.Downloading)
            {
                Operation.Cancel();
                _timer.Stop();
            }
            Console.WriteLine("Download canceled: " + Name);
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Progress = Math.Round(Operation.ProgressInPercent, 1);
            DisplaySize = Math.Round(Operation.TotalBytesReceived / 1024d / 1024d, 2) + "/" + Math.Round(Operation.SizeInBytes / 1024d / 1024d, 2) + " Mb";
        }

        private double progress;
        public double Progress
        {
            get => progress;
            set
            {
                this.SetProperty(ref progress, value);
                this.OnPropertyChanged(nameof(ProgressText));
            }
        }

        private string displaySize;
        public string DisplaySize
        {
            get => displaySize;
            set => this.SetProperty(ref displaySize, value);
        }

        public string ProgressText
        {
            get => Progress + "%";
        }

        private string errText;
        public string ErrorText
        {
            get => errText;
            set => this.SetProperty(ref errText, value);
        }

    }

    public enum ItemState
    {
        Standby,
        Downloading,
        Completed,
        Pause,
        Failed
    }

}
