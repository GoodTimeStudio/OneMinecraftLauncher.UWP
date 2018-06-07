using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP
{
    public class CoreManager
    {
        private static readonly JsonSerializerSettings _LaunchOptSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = new LaunchOptionTypesBinder(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        //DO NOT CHANGE
        public static readonly string WorkDirToken = "OneMinecraftLauncher_WorkDir_Token";

        public static readonly string OptionsListJsonFilePath = "options_list.json";

        public static bool needInit = false;

        public static LaunchOptionListViewModel OptListViewModel;

        public static SettingsViewModel SettingsModel;

        public static DownlaodPageViewModel DownlaodPageModel = new DownlaodPageViewModel();

        public static StorageFolder AppDir = ApplicationData.Current.LocalFolder;

        public static List<string> LocalAvailableVersionsList;

        #region User Settings
        public static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static StorageFolder WorkDir;

        //User Infomation
        public static string Username
        {
            get  { return GetLocalSettingAsString("username"); }
            set { localSettings.Values["username"] = value; }
        }
        public static string AccountTypeTag
        {
            get { return GetLocalSettingAsString("account_type_tag"); }
            set { localSettings.Values["account_type_tag"] = value; }
        }

        //Global Launch Option
        public static string GlobalJVMPath
        {
            get { return GetLocalSettingAsString("globalopt_jvmpath"); }
            set { localSettings.Values["globalopt_jvmpath"] = value; }
        }
        #endregion

        public static PackageVersion AppVersion = Package.Current.Id.Version;
        public static string CoreVersion;
        public static string AppVersionString
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}", AppVersion.Major, AppVersion.Minor, AppVersion.Build, AppVersion.Revision);
            }
        }

        public static async Task InitAppAsync()
        {
            #region WorkDir
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(WorkDirToken))
            {
                WorkDir = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(WorkDirToken);
            }

            if (WorkDir == null)
            {
                needInit = true;
            }

            if (needInit == true)
            {
                return;
            }
            #endregion

            MinecraftVersionManager.Init();

            #region OptionsList
            OptListViewModel = new LaunchOptionListViewModel();
            ObservableCollection<LaunchOption> tmp = await LoadOptionList();
            if (tmp != null)
            {
                OptListViewModel.OptionList = tmp;
            }
            if (OptListViewModel.OptionList.Count < 1)
            {
                OptListViewModel.OptionList.Add(new DefaultLaunchOption());
            }
            #endregion
            
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            AppServiceManager.AppServiceConnected += AppServiceManager_AppServiceConnected;
        }

        private async static void AppServiceManager_AppServiceConnected(object sender, EventArgs e)
        {
            ValueSet value = new ValueSet();
            value["type"] = "init";
            value["workDir"] = WorkDir?.Path;
            AppServiceResponse response = await AppServiceManager.appServiceConnection.SendMessageAsync(value);
            object _obj = null;
            response.Message.TryGetValue("core-version", out _obj);
            CoreVersion = _obj?.ToString();

            LocalAvailableVersionsList = await GetLocalAvailableVersionsAsync();
        }

        private static string GetLocalSettingAsString(string key)
        {
            string ret = "";
            var tmp = localSettings.Values[key];
            if (tmp != null)
                ret = tmp.ToString();
            return ret;
        }


        #region Options List Loader
        public async static Task SaveOptionList(ObservableCollection<LaunchOption> list)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(WorkDir.Path + @"\" + OptionsListJsonFilePath);

                StorageFile file = await WorkDir.TryGetItemAsync(OptionsListJsonFilePath) as StorageFile;
                if (file == null)
                {
                    file = await WorkDir.CreateFileAsync(OptionsListJsonFilePath);
                }

                string json = JsonConvert.SerializeObject(list, _LaunchOptSerializerSettings);

                if (json == null)
                    return;

                //new JsonTextWriter().WriteRawAsync()
                await FileIO.WriteTextAsync(file, json);
            }
            catch { }
        }

        public async static Task<ObservableCollection<LaunchOption>> LoadOptionList()
        {
            try
            {
                StorageFile file = await WorkDir.GetFileAsync(OptionsListJsonFilePath);
                string json = await FileIO.ReadTextAsync(file);
                return JsonConvert.DeserializeObject<ObservableCollection<LaunchOption>>(json, _LaunchOptSerializerSettings);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (JsonException)
            {
                await WorkDir.CreateFileAsync(OptionsListJsonFilePath, CreationCollisionOption.ReplaceExisting);
                return null;
            }
        }
        #endregion

        public static string GetStringFromResource(string path)
        {
            var loader = ResourceLoader.GetForCurrentView();
            return loader.GetString(path);
        }

        public static async Task<List<string>> GetLocalAvailableVersionsAsync()
        {
            if (AppServiceManager.appServiceConnection != null)
            {
                List<string> ret = new List<string>();

                ValueSet valueSet = new ValueSet();
                valueSet["type"] = "versionsList";

                AppServiceResponse response = await AppServiceManager.appServiceConnection.SendMessageAsync(valueSet);
                string json = response.Message["value"].ToString();
                try
                {
                    ret = JsonConvert.DeserializeObject<List<string>>(json);
                }
                catch (JsonException)
                { }

                return ret;
            }

            return null;
        }

      
    }
    
}
