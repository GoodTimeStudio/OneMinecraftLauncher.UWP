using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{
    public class LaunchOptionListViewModel : BindableBase
    {
        public ObservableCollection<LaunchOption> OptionList = new ObservableCollection<LaunchOption>();

        private LaunchOption _SelectedOption;
        public LaunchOption SelectedOption
        {
            get => _SelectedOption;
            set => this.SetProperty(ref _SelectedOption, value);
        }

        public LaunchOptionListViewModel()
        {
            //OptionList.Add(LaunchOption.CreateDefaultOption());
        }
    }
}
