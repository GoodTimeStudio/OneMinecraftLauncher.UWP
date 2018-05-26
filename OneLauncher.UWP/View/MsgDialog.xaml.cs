using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.View
{
    public sealed partial class MsgDialog : ContentDialog
    {

        public Action Action_PrimaryButtonClick;
        public Action Action_SecondaryButtonClick;

        public MsgDialog()
        {
            this.InitializeComponent();
        }

        public async Task Show(string title, string text)
        {
            TextBlock_Title.Text = title;
            TextBlock_Content.Text = text;
            await ShowAsync();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Action_PrimaryButtonClick?.Invoke();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Action_SecondaryButtonClick?.Invoke();
        }
    }
}
