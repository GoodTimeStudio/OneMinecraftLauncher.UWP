using GoodTimeStudio.OneLauncher.UWP.Models;
using GoodTimeStudio.OneMinecraftLauncher.UWP.McVersions;
using GoodTimeStudio.OneMinecraftLauncher.UWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Resources;
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
            NullValueHandling = NullValueHandling.Ignore
        };

        public delegate void AppServiceEstablishedHandler(AppServiceConnection connection);
        public static event AppServiceEstablishedHandler AppServiceEstablishedEvent;

        //DO NOT CHANGE
        public static readonly string WorkDirToken = "OneMinecraftLauncher_RunDir_Token";

        public static readonly string OptionsListJsonFileName = "options_list.json";

        public static bool needInit = false;

        public static LaunchOptionListViewModel OptListViewModel;

        public static SettingsViewModel SettingsModel;

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

        public static async Task InitAppAsync()
        {
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

            AppServiceEstablishedEvent += CoreManager_GetVersionsList_AppServiceEstablishedEvent;
            GetLocalAvailableVersionsAsync();
            var list = LocalAvailableVersionsList;

        }

        //App Service
        // see https://blogs.msdn.microsoft.com/appconsult/2016/12/19/desktop-bridge-the-migrate-phase-invoking-a-win32-process-from-a-uwp-app/
        public static void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails)
            {
                args.TaskInstance.GetDeferral();
                var connection = ((AppServiceTriggerDetails)args.TaskInstance.TriggerDetails).AppServiceConnection;
                AppServiceEstablishedEvent(connection);
            }
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
                FileInfo fileInfo = new FileInfo(WorkDir.Path + @"\" + OptionsListJsonFileName);

                StorageFile file = await WorkDir.TryGetItemAsync(OptionsListJsonFileName) as StorageFile;
                if (file == null)
                {
                    await WorkDir.CreateFileAsync(OptionsListJsonFileName);
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
                StorageFile file = await WorkDir.GetFileAsync(OptionsListJsonFileName);
                string json = await FileIO.ReadTextAsync(file);
                return JsonConvert.DeserializeObject<ObservableCollection<LaunchOption>>(json, _LaunchOptSerializerSettings);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (JsonException)
            {
                await WorkDir.CreateFileAsync(OptionsListJsonFileName, CreationCollisionOption.ReplaceExisting);
                return null;
            }
        }
        #endregion

        #region Resource Helper
        public static string GetStringFromResource(string path)
        {
            var loader = ResourceLoader.GetForCurrentView();
            return loader.GetString(path);
        }
        #endregion

        #region Get local available versions
        public static async void GetLocalAvailableVersionsAsync()
        {
            LocalAvailableVersionsList = new List<string>();
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("GetVersions");
        }

        private static void CoreManager_GetVersionsList_AppServiceEstablishedEvent(AppServiceConnection connection)
        {
            connection.RequestReceived += Connection_GetVersionsList_RequestReceived;
        }

        private async static void Connection_GetVersionsList_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            if (args.Request.Message["type"].ToString() == "RequestWorkDirForVersionsScan")
            {
                ValueSet valueSet = new ValueSet();
                valueSet["value"] = WorkDir.Path;
                AppServiceResponseStatus status = await args.Request.SendResponseAsync(valueSet);
                deferral.Complete();
            }
            else if (args.Request.Message["type"].ToString() == "VersionsList")
            {
                string json = args.Request.Message["value"].ToString();
                try
                {
                    LocalAvailableVersionsList = JsonConvert.DeserializeObject<List<string>>(json);
                }
                catch (JsonException)
                { }
                deferral.Complete();
            }
            
        }
        #endregion
    }
}
