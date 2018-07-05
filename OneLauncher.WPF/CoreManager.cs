using GoodTimeStudio.OneMinecraftLauncher.Core;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    public class CoreManager
    {
        public static OneMCL CoreMCL;
        public static LaunchOptionBase Option;

        public static Dictionary<string, KMCCC.Launcher.Version> VersionsIdMap;

        public static void Initialize()
        {
            CoreMCL = new OneMCL(@".\.minecraft");
            Option = new LaunchOptionBase("one-minecraft-launcher");
        }
    }
}
