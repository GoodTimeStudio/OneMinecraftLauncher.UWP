using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// ProfileSeletorDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProfileSeletorDialog : CustomDialog
    {
        public ProfileSeletorDialog()
        {
            InitializeComponent();
        }

        public ProfileSeletorDialog(object ViewModel) : this()
        {
            DataContext = ViewModel;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Current.HideMetroDialogAsync(this);
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.SaveConfigToFile();
            if (IsVisible)
            {
                await MainWindow.Current.HideMetroDialogAsync(this);
            }
        }
    }
}
