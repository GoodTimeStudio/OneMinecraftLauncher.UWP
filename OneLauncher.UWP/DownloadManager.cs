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
using Windows.Web;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP
{
    public class DownloadManager
    {
        public static ObservableCollection<DownloadItem> DownloadQuene = new ObservableCollection<DownloadItem>();

        private readonly static BackgroundDownloader _downloader = new BackgroundDownloader();

        public static double AllReceivedMb;
        public static double TotalMb;

        private static bool isDownloading;

        // Maximum download 5 files in the same time
        public async static void StartDownload()
        {
            if (!isDownloading)
            {
                int count = DownloadQuene.Count;

                if (count > 5)
                {
                    count = 5;
                }

                for (int i = 0; i < count; i++)
                {
                    DownloadItem item = DownloadQuene[i];
                    item.Download(_downloader);
                }

                isDownloading = true;
            }
        }

        public static void CancelAllDownload()
        {
            foreach (DownloadItem item in DownloadQuene)
            {
                item.Cancel();
            }
        }

        public static void DownloadNext(DownloadItem previousItem)
        {
            if (previousItem.State == DownloadState.Completed)
            {
                DownloadQuene.Remove(previousItem);
            }

            if (DownloadQuene.Count == 0)
            {
                isDownloading = false;
                return;
            }

            for (int i = 0; i < DownloadQuene.Count; i++)
            {
                DownloadItem item = DownloadQuene[i];
                if (item.State ==  DownloadState.Standby)
                {
                    item.Download(_downloader);
                    break;
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
            get => Progress > 0 ? Progress + "%" : string.Empty;
        }

        private string errText;
        public string ErrorText
        {
            get => errText;
            set => this.SetProperty(ref errText, value);
        }

        private CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// solution for UWP API defect
        /// See https://wpdev.uservoice.com/forums/110705-universal-windows-platform/suggestions/31640206-make-downloadoperation-progress-property-update-fa
        /// See https://social.msdn.microsoft.com/Forums/windowsapps/en-US/fc0bd6b5-9934-4f52-9b75-9d63154f39f7/downloadoperation-progress-updated-every-1mb-downloaded?forum=wpdevelop
        /// </summary>
        private int firstStart = 2;

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
            if (State != DownloadState.Standby)
            {
                return;
            }
            State = DownloadState.Downloading;

            DownloadManager.DebugWriteLine("DownloadMgr: Attempt to download " + this.Name);

            string reletivePath = this.Path;
            if (reletivePath.StartsWith(CoreManager.WorkDir.Path))
            {
                reletivePath = reletivePath.Replace(CoreManager.WorkDir.Path, "");
                if (reletivePath.StartsWith("/") || reletivePath.StartsWith(@"\"))
                {
                    reletivePath = reletivePath.Substring(1);
                }
            }
            DownloadManager.DebugWriteLine("DownloadMgr: File path: " + reletivePath);
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

        public void Pause()
        {
            if (State == DownloadState.Downloading)
            {
                Operation.Pause();
                State = DownloadState.Pause;
            }
        }

        public void Cancel()
        {
            if (State == DownloadState.Downloading)
            {
                Pause();
                Operation = null;
                DownloadManager.DownloadQuene.Remove(this);
            }
        }

        public void Resume()
        {
            if (State == DownloadState.Pause)
            {
                Operation.Resume();
            }
        }

        private async Task HandleDownloadAsync(bool start)
        {
            Progress<DownloadOperation> _progressCallback = new Progress<DownloadOperation>(UpdateProgress);

            try
            {
                if (start)
                {
                    // Start the download and attach a progress handler.
                    try
                    {
                        await Operation.StartAsync().AsTask(cts.Token, _progressCallback);
                    }
                    catch (Exception e)
                    {
                        if (!HandleExpection(e))
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    try
                    {
                        // The download was already running when the application started, re-attach the progress handler.
                        await Operation.AttachAsync().AsTask(cts.Token, _progressCallback);
                    }
                    catch (Exception e)
                    {
                        if (!HandleExpection(e))
                        {
                            throw;
                        }
                    }
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

        private bool HandleExpection(Exception ex)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                this.Progress = 0;
                DisplaySize = string.Empty;
                ErrorText = ex.Message;
                return false;
            }

            return true;
        }

        public async void UpdateProgress(DownloadOperation operation)
        {
            /* 
             * ugly code thanks for UWP API
             * pause and resume download make Progress update more frequently
             */
            if (firstStart == 2)
            {
                /*
                 * Note: this step is important
                 * If the file we need to download is small enough, the download time will be very short, 
                 * Operation.Progress.Status isn't update quickly when download is already complete
                 * so we need to finish this progress update.
                 */
                firstStart--;
            }
            else if (firstStart == 1 && Operation.Progress.Status == BackgroundTransferStatus.Running && State == DownloadState.Downloading)
            {
                Operation.Pause();
                await Task.Delay(100);
                Operation.Resume();
                firstStart--;
            }

            double received = Operation.Progress.BytesReceived;
            double total = Operation.Progress.TotalBytesToReceive;

            if (received > 0 && total > 0)
            {
                double progress = Math.Round(received / total * 100d, 2);
                double receivedMb = Math.Round(received / 1024 / 1024, 2);
                double totalMb = Math.Round(total / 1024 / 1024, 2);

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
        Pause,
        Failed
    }


}
