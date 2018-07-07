using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class StartViewModel : BindableBase
    {
        private string _Username;
        public string Username
        {
            get => _Username;
            set => SetProperty(ref _Username, value);
        }

        private string _JavaExt;
        public string JavaExt
        {
            get => _JavaExt;
            set => SetProperty(ref _JavaExt, value);
        }

        private int _MaxMemory;
        public int MaxMemory
        {
            get => _MaxMemory;
            set
            {
                SetProperty(ref _MaxMemory, value);
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

        private string _JavaArgs;
        public string JavaArgs
        {
            get => _JavaArgs;
            set => SetProperty(ref _JavaArgs, value);
        }

        private List<KMCCC.Launcher.Version> _VersionsList;
        public List<KMCCC.Launcher.Version> VersionsList
        {
            get => _VersionsList;
            set => SetProperty(ref _VersionsList, value);
        }

        private string _LaunchButtonContent;
        public string LaunchButtonContent
        {
            get => _LaunchButtonContent;
            set => SetProperty(ref _LaunchButtonContent, value);
        }
    }
}
