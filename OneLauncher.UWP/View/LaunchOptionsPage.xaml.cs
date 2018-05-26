using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LaunchOptionsPage : Page
    {
        public LaunchOptionsPage()
        {
            this.InitializeComponent();

            OptionsView.ListView.SelectionChanged += ListView_SelectionChanged;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0 && e.RemovedItems.First() is LaunchOption)
            {
                ((LaunchOption)e.RemovedItems.First()).isPreview = false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            OptionsView.OptListViewModel = CoreManager.OptListViewModel ?? new Models.LaunchOptionListViewModel();
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            CoreManager.OptListViewModel = OptionsView.OptListViewModel;
            await SaveOptions();
        }

        private async Task SaveOptions()
        {
            //Save unnamed configuration
            if (OptionsView.ListView.SelectedItem is LaunchOption)
            {
                var opt = OptionsView.ListView.SelectedItem as LaunchOption;
                opt.isPreview = false;
            }

            //SAVE OPTIONS LIST
            await CoreManager.SaveOptionList(OptionsView.OptListViewModel.OptionList);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            LaunchOption opt = new LaunchOption(CoreManager.GetStringFromResource("/LaunchOptions/UnnamedOption"));
            opt.isPreview = true;

            OptionsView.OptListViewModel.OptionList.Add(opt);
            OptionsView.ListView.SelectedItem = opt;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsView.ListView.SelectedItem is LaunchOption)
            {
                OptionsView.OptListViewModel.OptionList.Remove(OptionsView.ListView.SelectedItem as LaunchOption);
            }
        }

        private void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateTo("download");
        }

        private async void Button_GameDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");

                StorageFolder folder = await folderPicker.PickSingleFolderAsync();

                if (folder != null)
                {
                    LaunchOption opt = OptionsView.ListView.SelectedItem as LaunchOption;
                    opt.gameDir = folder.Path;
                }
            }
            catch (Exception exp)
            {
                await __msgDialog.Show(CoreManager.GetStringFromResource("/LaunchOptions/GameDirSelectError"), exp.Message + "\r\n" + exp.StackTrace);
            }
        }

        private async void Button_JavaExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
                filePicker.FileTypeFilter.Add(".exe");

                StorageFile file = await filePicker.PickSingleFileAsync();

                if (file != null)
                {
                    LaunchOption opt = OptionsView.ListView.SelectedItem as LaunchOption;
                    opt.javaExt = file.Path;
                }
            }
            catch (Exception exp)
            {
                await __msgDialog.Show(CoreManager.GetStringFromResource("/LaunchOptions/JavaExtSelectError"), exp.Message + "\r\n" + exp.StackTrace);
            }
        }
    }

    public class CreateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                return string.Format(CoreManager.GetStringFromResource("/LaunchOptions/CreateTime"), (DateTime)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class LastPlayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                return string.Format(CoreManager.GetStringFromResource("/LaunchOptions/LastPlay"), (DateTime)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
