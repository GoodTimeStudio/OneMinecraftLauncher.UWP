using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
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

        private ObservableCollection<MinecraftVersion> _VersionList = new ObservableCollection<MinecraftVersion>();
        public ObservableCollection<MinecraftVersion> VersionsList
        {
            get => _VersionList;
            set => this.SetProperty(ref _VersionList, value);
        }

        private bool _isPaneOpen;
        public bool isPaneOpen
        {
            get => _isPaneOpen;
            set => this.SetProperty(ref _isPaneOpen, value);
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

        public void MakeList()
        {
            if (MinecraftVersionManager.VersionsList != null)
            {
                VersionsList = new ObservableCollection<MinecraftVersion>();
                foreach (MinecraftVersion ver in MinecraftVersionManager.VersionsList.versions)
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
