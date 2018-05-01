using GoodTimeStudio.OneMinecraftLauncher.UWP.News;
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

        private List<NewsContent> _NewsList;
        public List<NewsContent> NewsList
        {
            get => _NewsList;
            set => this.SetProperty(ref _NewsList, value);
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
