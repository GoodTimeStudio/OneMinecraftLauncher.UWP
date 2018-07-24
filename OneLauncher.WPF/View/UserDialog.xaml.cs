using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using KMCCC.Authentication;
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
    public partial class UserDialog : CustomDialog
    {
        private readonly StartPageViewModel ViewModel;

        public UserDialog(StartPageViewModel ViewModel)
        {
            InitializeComponent();
            DataContext = ViewModel;
            this.ViewModel = ViewModel;
        }

        private void setup()
        {
            if (ViewModel.SelectedAccountType?.Authentication == AuthenticationType.Password)
            {
                ViewModel.SetupUserDialog(UserDialogState.Input);
            }
            else
            {
                ViewModel.SetupUserDialog(UserDialogState.Offline);
            }
        }

        private async void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (CoreManager.CoreMCL.UserAuthenticator == null)
            {
                string player = "Steve";
                ViewModel.SelectedAccountType = AccountTypes.Offline;
                ViewModel.User = player;
                ViewModel.PlayerName = player;
                Config.INSTANCE.User = player;
                Config.INSTANCE.Playername = player;
                Config.INSTANCE.AccountType = AccountTypes.Offline.Tag;
                CoreManager.CoreMCL.UserAuthenticator = new OfflineAuthenticator(player);
            }
            else
            {
                ViewModel.SelectedAccountType = AccountTypes.GetAccountTypeFromTag(Config.INSTANCE.AccountType);
                ViewModel.User = Config.INSTANCE.User;
            }

            PasswordBox.Password = string.Empty;
            Config.SaveConfigToFileAsync();
            await MainWindow.Current.HideMetroDialogAsync(this);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setup();
            ViewModel.User = string.Empty;
            ViewModel.UserDialogResultString = string.Empty;
            PasswordBox.Password = string.Empty;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.UserDialogCurrentState == UserDialogState.LoggedIn)
            {
                ViewModel.User = string.Empty;
                ViewModel.PlayerName = string.Empty;
                Config.INSTANCE.User = string.Empty;
                Config.INSTANCE.Playername = string.Empty;
                Config.INSTANCE.Password = string.Empty;
                Config.INSTANCE.UUID = string.Empty;
                Config.SaveConfigToFileAsync();
                CoreManager.CoreMCL.UserAuthenticator = null;

                ViewModel.SetupUserDialog(UserDialogState.Input);
            }
            else
            {
                await Login();
            }
        }

        private async Task Login()
        {
            ViewModel.UserDialogResultString = string.Empty;
            bool offline = ViewModel.SelectedAccountType == AccountTypes.Offline;
            if (offline)
            {
                ViewModel.SetupUserDialog(UserDialogState.Offline);
            }
            else
            {
                ViewModel.SetupUserDialog(UserDialogState.Logging);
            }

            AuthenticationInfo info = await CoreManager.Auth(ViewModel.SelectedAccountType, ViewModel.User, PasswordBox.Password);
            IAuthenticator authenticator = CoreManager.GenAuthenticatorFromAuthInfo(info);

            if (authenticator != null)
            {
                CoreManager.CoreMCL.UserAuthenticator = authenticator;

                ViewModel.PlayerName = info.DisplayName;
                Config.INSTANCE.AccountType = ViewModel.SelectedAccountType.Tag;
                Config.INSTANCE.User = ViewModel.User;
                Config.INSTANCE.Playername = info.DisplayName;
                Config.INSTANCE.UUID = info.UUID.ToString();

                if (!offline)
                {
                    ViewModel.SetupUserDialog(UserDialogState.LoggedIn);
                    PasswordBox.Password = string.Empty;
                    Config.INSTANCE.Password = info.AccessToken.ToString();
                }

                Config.SaveConfigToFileAsync();
                await MainWindow.Current.HideMetroDialogAsync(this);
            }
            else
            {
                ViewModel.UserDialogResultString = "用户验证失败 \r\n " + info?.Error;
                ViewModel.SetupUserDialog(UserDialogState.Input);
            }
        }
    }
}
