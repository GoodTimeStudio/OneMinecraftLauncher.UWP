using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public class PacketInit : IServicePacket
    {
        public string GetTypeName()
        {
            return "init";
        }

        public ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string workDir = args.Request.Message["workDir"].ToString();
            if (string.IsNullOrWhiteSpace(workDir))
            {
                return null;
            }

            Console.WriteLine("Work dir is " + workDir);
            Console.WriteLine("Creating KMCCC LauncherCore");

            LaunchMessage message = new LaunchMessage { WorkDirPath = workDir };
            Program.Core = OneMinecraftLauncher.Core.OneMinecraftLauncher.CreateLauncherCore(message);
            return null;
        }
    }
}
