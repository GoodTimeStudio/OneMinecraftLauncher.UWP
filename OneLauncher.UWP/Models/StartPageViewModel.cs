using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{
    public class StartPageViewModel : BindableBase
    {

        public LaunchOptionListViewModel ListModel;

        private string _MsgBoxTitleText;
        public string MsgBoxTitleText
        {
            get => _MsgBoxTitleText;
            set => this.SetProperty(ref _MsgBoxTitleText, value);
        }

        private string _MsgBoxContentText;
        public string MsgBoxContentText
        {
            get => _MsgBoxContentText;
            set => this.SetProperty(ref _MsgBoxContentText, value);
        }

        public StartPageViewModel(LaunchOptionListViewModel listModel)
        {
            ListModel = listModel ?? throw new ArgumentNullException(nameof(listModel));
        }

        public bool ReadyToLaunchHeaderVisbility
        {
            get => ListModel?.SelectedOption != null ? true : false;
        }
    }
}
