using AltoHttp;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class DownloadItem : BindableBase
    {
        private DownloadManager _manager;

        public string Name { get; set; }

        public string Path { get; set; }

        public Uri Uri { get; set; }

        public HttpDownloader Operation { get; set; }

        public DownloadItem(string name, string path, Uri uri)
        {
            Name = name;
            Path = path;
            Uri = uri;
            Progress = 0;
            Operation = new HttpDownloader(uri.AbsoluteUri, path);
            Operation.DownloadProgressChanged += Operation_DownloadProgressChanged;
            Operation.DownloadErrorOccured += Operation_DownloadErrorOccured;
            Operation.DownloadCompleted += Operation_DownloadCompleted;
        }

        public void Start(DownloadManager manager)
        {
            _manager = manager;
            FileInfo file = new FileInfo(Path);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            File.Create(Path).Dispose();
            Operation.Start();
        }

        private void Operation_DownloadCompleted(object sender, EventArgs e)
        {
            _manager.DownloadNext(this);
        }

        private void Operation_DownloadErrorOccured(object sender, DownloadErrorOccuredEventArgs e)
        {
            ErrorText = e.Exception.Message;
            _manager.DownloadNext(this, false);
        }

        private void Operation_DownloadProgressChanged(object sender, EventArgs e)
        {
            Progress = Math.Round(Operation.ProgressInPercent, 1);
            DisplaySize = Math.Round(Operation.TotalBytesReceived / 1024d / 1024d, 2)  + "/" + Math.Round(Operation.SizeInBytes / 1024d / 1024d, 2)  + " Mb";
            _manager.CurrentSpeedText = "当前速度: " + CoreManager.GetDownloadSpeedFriendlyText(Operation);
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

}
