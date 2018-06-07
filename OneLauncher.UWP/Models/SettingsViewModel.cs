using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{
    public class SettingsViewModel : BindableBase
    {

        public List<AccountType> AllAccountTypes = CreateAccountTypesList();
        public AccountType SelectedAccountType
        {
            get { return GetAccountTypeFromTag(CoreManager.AccountTypeTag); }
            set { CoreManager.AccountTypeTag = value.Tag; }
        }

        private string __ErrDialog_Text;
        public string ErrDialog_Text
        {
            get { return this.__ErrDialog_Text; }
            set { this.SetProperty(ref this.__ErrDialog_Text, value); }
        }

        private string __WorkDir;
        public string WorkDir
        {
            get { return this.__WorkDir; }
            set { this.SetProperty(ref this.__WorkDir, value); }
        }
        
        //One Way To Source Binding
        public string Username
        {
            get { return CoreManager.Username; }
            set { CoreManager.Username = value; }
        }

        public string GlobalJVMPath
        {
            get { return CoreManager.GlobalJVMPath; }
            set
            {
                CoreManager.GlobalJVMPath = value;
                this.OnPropertyChanged();
            }
        }

        #region AccountType Helper
        private static List<AccountType> CreateAccountTypesList()
        {
            List<AccountType> types = new List<AccountType>();
            types.Add(new AccountType(CoreManager.GetStringFromResource("/SettingsPage/AccountType_Mojang"), "mojang", "OnlineAccountState"));
            types.Add(new AccountType(CoreManager.GetStringFromResource("/SettingsPage/AccountType_Offline"), "offline", "OfflineAccountState"));
            return types;
        }

        public AccountType GetAccountTypeFromTag(string tag)
        {
            foreach (AccountType type in AllAccountTypes)
            {
                if (string.Equals(tag, type.Tag))
                    return type;
            }
            return AllAccountTypes.First();
        }

        #endregion

    }

    public class AccountType
    {
        public string Text;
        public string Tag;
        public string VisualStateName;

        public AccountType(string text, string tag, string VisualStateName)
        {
            this.Text = text;
            this.Tag = tag;
            this.VisualStateName = VisualStateName;
        }

        public override string ToString()
        {
            return this.Text;
        }

    }
}
