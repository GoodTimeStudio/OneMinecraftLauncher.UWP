using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class StartPageViewModel : BindableBase
    {
        private ObservableCollection<KMCCC.Launcher.Version> _VersionsList;
        public ObservableCollection<KMCCC.Launcher.Version> VersionsList
        {
            get => _VersionsList;
            set => SetProperty(ref _VersionsList, value);
        }

        private KMCCC.Launcher.Version _SelectedVersion;
        public KMCCC.Launcher.Version SelectedVersion
        {
            get => _SelectedVersion;
            set
            {
                SetProperty(ref _SelectedVersion, value);
                Config.INSTANCE.SelectedVersion = value.Id;
            }
        }

        private ObservableCollection<AccountType> _AccountTypesList;
        public ObservableCollection<AccountType> AccountTypesList
        {
            get => _AccountTypesList;
            set => SetProperty(ref _AccountTypesList, value);
        }

        private AccountType _SelectedAccountType;
        public AccountType SelectedAccountType
        {
            get => _SelectedAccountType;
            set => SetProperty(ref _SelectedAccountType, value);
        }

        private string _User;
        /// <summary>
        /// Username or Email
        /// </summary>
        public string User
        {
            get => _User;
            set => SetProperty(ref _User, value);
        }

        private string _PlayerName;
        public string PlayerName
        {
            get => _PlayerName;
            set => SetProperty(ref _PlayerName, value);
        }

        #region UserDialog
        private string _UserDialogPrimaryButtonContent;
        public string UserDialogPrimaryButtonContent
        {
            get => _UserDialogPrimaryButtonContent;
            set => SetProperty(ref _UserDialogPrimaryButtonContent, value);
        }

        private Visibility _UserDialogPasswordBoxVisibility;
        public Visibility UserDialogPasswordBoxVisibility
        {
            get => _UserDialogPasswordBoxVisibility;
            set => SetProperty(ref _UserDialogPasswordBoxVisibility, value);
        }

        private bool _UserDialogWorking;
        public bool UserDialogWorking
        {
            get => _UserDialogWorking;
            set
            {
                SetProperty(ref _UserDialogWorking, value);
                OnPropertyChanged(nameof(UserDialogNotWorking));
            }
        }

        public bool UserDialogNotWorking
        {
            get => !UserDialogWorking;
        }

        private string _UserDialogResultString;
        public string UserDialogResultString
        {
            get => _UserDialogResultString;
            set => SetProperty(ref _UserDialogResultString, value);
        }

        private bool _UserDialogNotLock;
        public bool UserDialogNotLock
        {
            get => _UserDialogNotLock;
            set
            {
                SetProperty(ref _UserDialogNotLock, value);
            }
        }

        private string _LaunchButtonContent;
        public string LaunchButtonContent
        {
            get => _LaunchButtonContent;
            set => SetProperty(ref _LaunchButtonContent, value);
        }

        public UserDialogState UserDialogCurrentState { get; set; }

        public void SetupUserDialog(UserDialogState state)
        {
            UserDialogCurrentState = state;
            switch (state)
            {
                case UserDialogState.Input:
                    UserDialogNotLock = true;
                    UserDialogPrimaryButtonContent = "登陆";
                    UserDialogPasswordBoxVisibility = Visibility.Visible;
                    UserDialogWorking = false;
                    break;
                case UserDialogState.Logging:
                    UserDialogNotLock = false;
                    UserDialogPrimaryButtonContent = "登陆";
                    UserDialogPasswordBoxVisibility = Visibility.Visible;
                    UserDialogWorking = true;
                    break;
                case UserDialogState.LoggedIn:
                    UserDialogNotLock = false;
                    UserDialogPrimaryButtonContent = "登出";
                    UserDialogPasswordBoxVisibility = Visibility.Collapsed;
                    UserDialogWorking = false;
                    break;
                case UserDialogState.Offline:
                    UserDialogNotLock = true;
                    UserDialogPrimaryButtonContent = "确定";
                    UserDialogPasswordBoxVisibility = Visibility.Collapsed;
                    UserDialogWorking = false;
                    break;
            }
        }

        #endregion
    }

    public enum UserDialogState
    {
        Input,
        Logging,
        LoggedIn,
        Offline,
    }
}
