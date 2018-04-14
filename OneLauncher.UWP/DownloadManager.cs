using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP
{
    public class DownloadManager
    {

        public static ObservableCollection<DownloadItem> DownloadQuene = new ObservableCollection<DownloadItem>();

        public static BackgroundDownloader Downloader = new BackgroundDownloader();

        public static double AllReceivedMb;
        public static double TotalMb;

        // Maximum download 10 files in the same time
        public static void DownloadAll()
        {
            int count = DownloadQuene.Count;
            if (count > 10)
            {
                count = 10;
            }

            for (int i = 0; i < count; i++)
            {
                DownloadItem item = DownloadQuene[i];
                item.Download(Downloader);
            }
        }

        public static void DownloadNext(DownloadItem previousItem)
        {
            if (previousItem.State == DownloadState.Completed)
            {
                DownloadQuene.Remove(previousItem);
            }

            for (int i = 0; i < DownloadQuene.Count; i++)
            {
                DownloadItem item = DownloadQuene[i];
                if (item.State ==  DownloadState.Standby)
                {
                    item.Download(Downloader);
                }
            }
        }

        public static void DebugWriteLine(string str)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(str);
#endif
        }

    }

    public class DownloadItem : BindableBase
    {
        public string Name;

        public string Path;

        public Uri Uri;

        public DownloadOperation Operation;

        public DownloadState State;

        private double progress;
        public double Progress
        {
            get => progress;
            set => this.SetProperty(ref progress, value);
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

        private CancellationTokenSource cts = new CancellationTokenSource();

        public DownloadItem(string name, string path, string url)
        {
            Name = name;
            Path = path;

            Uri source;
            if (!Uri.TryCreate(url, UriKind.Absolute, out source))
            {
                throw new ArgumentException("Invalid uri: " + url);
            }
            Uri = source;
            State = DownloadState.Standby;
        }

        public async void Download(BackgroundDownloader downloader)
        {
            await DownloadAsync(downloader);
        }

        public async Task DownloadAsync(BackgroundDownloader downloader)
        {
            DownloadManager.DebugWriteLine("DownloadMgr: Attempt to download " + this.Name);

            string reletivePath = null;
            if (this.Path.StartsWith(CoreManager.WorkDir.Path))
            {
                reletivePath = this.Path.Replace(CoreManager.WorkDir.Path, "");
                if (reletivePath.StartsWith("/") || reletivePath.StartsWith(@"\"))
                {
                    reletivePath = reletivePath.Substring(1);
                }
            }
            DownloadManager.DebugWriteLine("DownloadMgr: File path: " + reletivePath);
            DownloadManager.DebugWriteLine("DownloadMgr: Creating file");
            StorageFile target;
            try
            {
                target = await CoreManager.WorkDir.CreateFileAsync(reletivePath, CreationCollisionOption.ReplaceExisting);
            }
            catch (IOException)
            {
                return;
            }

            DownloadManager.DebugWriteLine("DownloadMgr: Start to download");
            Operation = downloader.CreateDownload(this.Uri, target);
            await HandleDownloadAsync(true);
        }

        private async Task HandleDownloadAsync(bool start)
        {
            Progress<DownloadOperation> _progressCallback = new Progress<DownloadOperation>(UpdateProgress);

            try
            {
                State = DownloadState.Downloading;

                if (start)
                {
                    // Start the download and attach a progress handler.
                    await Operation.StartAsync().AsTask(cts.Token, _progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await Operation.AttachAsync().AsTask(cts.Token, _progressCallback);
                }

                // Download complete
                State = DownloadState.Completed;
                DownloadManager.DownloadQuene.Remove(this);
            }
            catch (TaskCanceledException)
            {
                State = DownloadState.Failed;
            }
            catch (Exception)
            {
                State = DownloadState.Failed;
            }
            finally
            {
                DownloadManager.DownloadNext(this);
            }
        }

        public async void UpdateProgress(DownloadOperation operation)
        {
            ulong received = Operation.Progress.BytesReceived;
            ulong total = Operation.Progress.TotalBytesToReceive;

            if (received > 0 && total > 0)
            {
                double progress = (received / total) * 100;
                double receivedMb = Math.Round((double)received / 1024 / 1024, 2);
                double totalMb = Math.Round((double)total / 1024 / 1024, 2);

                System.Diagnostics.Debug.WriteLine(string.Format("{0}: received {1}, receivedMb {2}, total {3}, totalMb {4}, progress {5}%", Name, received, receivedMb, total, totalMb, progress));

                await MainPage.Instance.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DisplaySize = receivedMb + " / " + totalMb + " Mb";
                    Progress = progress;
                });
            }
            
        }

    }

    public enum DownloadState
    {
        Standby,
        Downloading,
        Completed,
        Failed
    }


}
