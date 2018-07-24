using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    public class MainWindowViewModel : BindableBase
    {
        private Visibility _BackButtonVisibility;
        public Visibility BackButtonVisibility
        {
            get => _BackButtonVisibility;
            set => SetProperty(ref _BackButtonVisibility, value);
        }
    }
}
