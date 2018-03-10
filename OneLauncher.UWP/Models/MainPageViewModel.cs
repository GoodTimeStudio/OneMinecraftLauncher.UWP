using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{
    public class MainPageViewModel : BindableBase
    {
        private bool _IsNavItemEnable = true;
        public bool IsNavItemEnable
        {
            get => _IsNavItemEnable;
            set => this.SetProperty(ref _IsNavItemEnable, value);
        }
    }
}
