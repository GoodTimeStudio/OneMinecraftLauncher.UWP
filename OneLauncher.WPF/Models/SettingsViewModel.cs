using GoodTimeStudio.OneMinecraftLauncher.Core.Downloading;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class SettingsViewModel : BindableBase
    {
        private string _JavaExtPath;
        public string JavaExt
        {
            get => _JavaExtPath;
            set
            {
                SetProperty(ref _JavaExtPath, value);
                Config.INSTANCE.JavaExt = value;
            }
        }

        private string _JavaArgs;
        public string JavaArgs
        {
            get => _JavaArgs;
            set
            {
                SetProperty(ref _JavaArgs, value);
                Config.INSTANCE.JavaArgs = value;
            }
        }

        private int _MaxMemory;
        public int MaxMemory
        {
            get => _MaxMemory;
            set
            {
                SetProperty(ref _MaxMemory, value);
                Config.INSTANCE.MaxMemory = value;
                OnPropertyChanged(nameof(MaxMemoryStr));
            }
        }

        public string MaxMemoryStr
        {
            get => _MaxMemory.ToString();
            set
            {
                if (int.TryParse(value, out int i))
                {
                    MaxMemory = i;
                }
            }
        }

        private IDownloadSource _DownloadSource;
        public IDownloadSource DownloadSource
        {
            get => _DownloadSource;
            set
            {
                SetProperty(ref _DownloadSource, value);
                CoreManager.DownloadSource = value;
                Config.INSTANCE.DownloadSourceId = value.SourceID;
                Config.SaveConfigToFile();
            }
        }

        private List<IDownloadSource> _DownloadSourcesList;
        public List<IDownloadSource> DownloadSourcesList
        {
            get => _DownloadSourcesList;
            set => SetProperty(ref _DownloadSourcesList, value);
        }
    }
}
