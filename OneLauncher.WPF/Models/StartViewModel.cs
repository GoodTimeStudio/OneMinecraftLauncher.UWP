using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.ObjectModel;
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

        private ObservableCollection<KMCCC.Launcher.Version> _VersionsList;
        public ObservableCollection<KMCCC.Launcher.Version> VersionsList
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
