using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class DownloadItem : BindableBase
    {

        public string Name { get; set; }

        public string Path { get; set; }

        public Uri Uri { get; set; }

        public DownloadState State { get; set; }

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
