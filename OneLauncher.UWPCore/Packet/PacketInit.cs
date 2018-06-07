using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public class PacketInit : PacketBase
    {

        public override string GetTypeName()
        {
            return "init";
        }

        public override ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string workDir = args.Request.Message["workDir"].ToString();
            if (string.IsNullOrWhiteSpace(workDir))
            {
                return null;
            }

            Logger.Info("Work dir is " + workDir);
            Logger.Info("Creating KMCCC LauncherCore");

            LaunchMessage message = new LaunchMessage { WorkDirPath = workDir };
            Program.Core = OneMinecraftLauncher.Core.OneMinecraftLauncher.CreateLauncherCore(message);

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyFileVersionAttribute ver = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));
            
            Logger.Info("Version " + ver.Version);
            ValueSet ret = new ValueSet();
            ret["core-version"] = ver.Version;
            return ret;
        }
    }
}
