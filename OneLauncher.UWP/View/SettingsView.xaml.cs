using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
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
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel;

        public Action CheckAction;

        public SettingsView()
        {
            this.InitializeComponent();
            ViewModel = CoreManager.SettingsModel ?? new SettingsViewModel();

            if (CoreManager.WorkDir != null)
            {
                ViewModel.WorkDir = CoreManager.WorkDir.Path;
            }

            SetupAccountTypeState(ViewModel.GetAccountTypeFromTag(CoreManager.AccountTypeTag));

        }

        private async void Button_WorkDirPicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                folderPicker.FileTypeFilter.Add("*");

                StorageFolder folder = await folderPicker.PickSingleFolderAsync();

                if (folder != null)
                {
                    // Application now has read/write access to all contents in the picked folder
                    // (including other sub-folder contents)
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(CoreManager.WorkDirToken, folder);

                    ViewModel.WorkDir = folder.Path;
                    CoreManager.WorkDir = folder;
                    CheckAction?.Invoke();
                }
            }
            catch (Exception exp)
            {
                ViewModel.ErrDialog_Text = "";
                ViewModel.ErrDialog_Text = exp.Message;
                await ErrDialog_WorkDirPicker.ShowAsync();
            }
        }

        private void AccountTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                var item = e.AddedItems.First();
                if (item != null && item is AccountType)
                {
                    SetupAccountTypeState(item as  AccountType);
                }
            }
        }

        private void SetupAccountTypeState(AccountType accountType)
        {
            VisualStateManager.GoToState(this, accountType.VisualStateName, true);
        }

        private async void Button_JVMPicker_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".exe");
            

            StorageFile jvm = await picker.PickSingleFileAsync();
            ViewModel.GlobalJVMPath = jvm.Path;
        }

        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckAction?.Invoke();
        }
    }
}
