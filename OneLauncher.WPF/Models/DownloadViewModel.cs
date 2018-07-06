using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class DownloadViewModel : BindableBase
    {
        private MinecraftVersionsList _AllMinecraftVersions;
        public MinecraftVersionsList AllMinecraftVersions
        {
            set
            {
                _AllMinecraftVersions = value;
                MakeList();
            }
            get => _AllMinecraftVersions;
        }

        private ObservableCollection<MinecraftVersion> _VersionList = new ObservableCollection<MinecraftVersion>();
        public ObservableCollection<MinecraftVersion> VersionsList
        {
            get => _VersionList;
            set => this.SetProperty(ref _VersionList, value);
        }

        private bool _isSnapshotEnabled;
        public bool isSnapshotEnabled
        {
            get => _isSnapshotEnabled;
            set
            {
                this.SetProperty(ref _isSnapshotEnabled, value);
                MakeList();
            }
        }

        private bool _isHistoricalEnabled;
        public bool isHistoricalEnabled
        {
            get => _isHistoricalEnabled;
            set
            {
                this.SetProperty(ref _isHistoricalEnabled, value);
                MakeList();
            }
        }

        private bool _isWorking;
        public bool isWorking
        {
            get => _isWorking;
            set
            {
                SetProperty(ref _isWorking, value);
                OnPropertyChanged(nameof(isNotWorking));
            }
        }

        public bool isNotWorking
        {
            get => !isWorking;
        }

        private void MakeList()
        {
            if (AllMinecraftVersions != null)
            {
                foreach (MinecraftVersion ver in AllMinecraftVersions.versions)
                {
                    if (!string.IsNullOrWhiteSpace(ver.type))
                    {
                        if (ver.type.Equals(MinecraftVersion.Type_Snapshot))
                        {
                            if (isSnapshotEnabled)
                            {
                                VersionsList.Add(ver);
                            }
                        }
                        else if (!ver.type.Equals(MinecraftVersion.Type_Release) && isHistoricalEnabled) // historical version
                        {
                            VersionsList.Add(ver);
                        }
                        else if (ver.type.Equals(MinecraftVersion.Type_Release))
                        {
                            VersionsList.Add(ver);
                        }
                    }

                }
            }
        }

    }
}
