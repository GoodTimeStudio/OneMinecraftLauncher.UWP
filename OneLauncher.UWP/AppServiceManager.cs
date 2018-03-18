using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP
{
    public class AppServiceManager
    {
        public static AppServiceConnection appServiceConnection;
        private static BackgroundTaskDeferral appServiceDeferral;
        public static event EventHandler AppServiceConnected;

        public const string Launch_ServiceName = "LaunchAgent";
        public const string VersionList_ServiceName = "LaunchAgent-VersionList";

        //App Service
        // see https://blogs.msdn.microsoft.com/appconsult/2016/12/19/desktop-bridge-the-migrate-phase-invoking-a-win32-process-from-a-uwp-app/
        public static void OnAppServiceActivated(BackgroundActivatedEventArgs args)
        {
            IBackgroundTaskInstance taskInstance = args.TaskInstance;
            AppServiceTriggerDetails details = (AppServiceTriggerDetails)args.TaskInstance.TriggerDetails;
            appServiceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnAppServiceCanceled;
            appServiceConnection = details.AppServiceConnection;

            AppServiceConnected?.Invoke(null, null);
        }

        private static void OnAppServiceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            appServiceDeferral?.Complete();
        }

    }
}
