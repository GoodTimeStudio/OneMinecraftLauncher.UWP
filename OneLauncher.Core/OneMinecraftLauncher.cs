using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using KMCCC.Authentication;
using KMCCC.Launcher;
using KMCCC.Modules.JVersion;
using KMCCC.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.Core
{
    public class OneMinecraftLauncher
    {
        private static LaunchResult FailedResult = new LaunchResult()
        {
            Success = false,
            ErrorType = ErrorType.Unknown
        };

        //Demo
        public static void Main(string[] args)
        {
            LaunchMessage message = new LaunchMessage()
            {
                WorkDirPath = "E:/BestOwl/Desktop/LauncherTest",
                GameDirPath = "E:\\BestOwl\\Desktop\\LauncherTest_GameDir",
                VersionId = "1.12.2",
                authenticator = new OfflineAuthenticator("MicroOwl"),
                JavaExtPath = "C:\\Program Files\\Java\\jre-9.0.4\\bin\\javaw.exe"
            };
            LaunchResult result = Launch(message);
        }

        public static LaunchResult Launch(LaunchMessage message)
        {
            Console.WriteLine("Processing...");

            if (message == null)
            {
                Console.WriteLine("ERROR: Launch message is null");
                FailedResult.ErrorMessage = "Launch message is null";
                return FailedResult;
            }
            if (message.authenticator == null)
            {
                FailedResult.ErrorType = ErrorType.AuthenticationFailed;
                FailedResult.ErrorMessage = "Unknown authenticator, authenticator is null";
                Console.WriteLine("ERROR: Unknown authenticator, authenticator is null");
                return FailedResult;
            }

            Console.WriteLine("Creating KMCCC LaunchCore");

            LauncherCore core = CreateLauncherCore(message);
            LaunchOptions options = new LaunchOptions()
            {
                Version = core.GetVersion(message.VersionId),
                MaxMemory = message.MaxMemory,
                MinMemory = message.MinMemory,
                Authenticator = message.authenticator,
                GameDirPath = message.GameDirPath,
                Server = message.Server,
                Size = message.Size,
                AgentPath = message.AgentPath
            };

            Console.WriteLine("Launching...");

            return core.Launch(options, (Action<MinecraftLaunchArguments>) (args => 
            {
                //TO-DO: add java args to this.
                System.Version osVersion = Environment.OSVersion.Version;
                args.AdvencedArguments.Add("-Dos.name=\"" + GetSystemVersionName(osVersion.Major, osVersion.Minor) + "\"");
                args.AdvencedArguments.Add("-Dos.version=" + osVersion.Major + "." + osVersion.Minor);
                args.AdvencedArguments.Add("-Dminecraft.launcher.brand=one-minecraft-launcher");
                //args.AdvencedArguments.Add("-Dminecraft.launcher.version=");
                args.AdvencedArguments.Add("-Dminecraft.client.jar=" + core.GetVersionJarPath(options.Version));
                //args.AdvencedArguments.Add("-Dlog4j.configurationFile=");
            }));
        }

        public static LauncherCore CreateLauncherCore(LaunchMessage message)
        {
            if (message == null)
                return null;

            LauncherCore core = LauncherCore.Create(new LauncherCoreCreationOption(
                    message.WorkDirPath,
                    message.JavaExtPath,
                    new JVersionLocator()
                ));
            return core;
        }

        //TO-DO: detect the server version of windows
        public static string GetSystemVersionName(int majorVersion, int minorVersion)
        {
            switch (majorVersion)
            {
                case 10:
                    return "Windows 10";
                case 6:
                    {
                        switch (minorVersion)
                        {
                            case 3:
                                return "Windows 8.1";
                            case 2:
                                return "Windows 8";
                            case 1:
                                return "Windows 7";
                            case 0:
                                return "Windows Vista";
                            default:
                                return null;
                        }
                    }
                case 5:
                    {
                        switch (minorVersion)
                        {
                            case 1:
                                return "Windows XP";
                            case 0:
                                return "Windows 2000";
                            default:
                                return null;
                        }
                    }
                default:
                    return null;
            }
        }

    }
}
