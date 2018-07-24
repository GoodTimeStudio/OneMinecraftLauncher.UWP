using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Models
{
    public class AccountType
    {
        public string Text { get; set; }
        public string Tag { get; set; }
        public string VisualStateName { get; set; }
        public AuthenticationType Authentication { get; set; }

        public AccountType(string text, string tag, string VisualStateName, AuthenticationType authentication)
        {
            this.Text = text;
            this.Tag = tag;
            this.VisualStateName = VisualStateName;
            this.Authentication = authentication;
        }

        public override string ToString()
        {
            return this.Text;
        }

    }

    public static class AccountTypes
    {
        public static List<AccountType> AllAccountTypes = new List<AccountType>();

        public static AccountType Mojang = new AccountType(string.Empty, "mojang", "OnlineAccountState", AuthenticationType.Password);
        public static AccountType Offline = new AccountType(string.Empty, "offline", "OfflineAccountState", AuthenticationType.None);

        static AccountTypes()
        {
            AllAccountTypes.Add(Mojang);
            AllAccountTypes.Add(Offline);
        }

        public static AccountType GetAccountTypeFromTag(string tag)
        {
            foreach (AccountType type in AllAccountTypes)
            {
                if (string.Equals(tag, type.Tag))
                    return type;
            }
            return AllAccountTypes.First();
        }
    }

    public enum AuthenticationType
    {
        None,
        Password
    }
}
