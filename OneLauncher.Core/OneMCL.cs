using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using KMCCC.Authentication;
using KMCCC.Launcher;
using KMCCC.Modules.JVersion;
using System;
using System.Collections.Generic;

namespace GoodTimeStudio.OneMinecraftLauncher.Core
{
    public class OneMCL
    {
        public LauncherCore Core;
        public IAuthenticator UserAuthenticator;

        private LaunchResult FailedResult = new LaunchResult()
        {
            Success = false,
            ErrorType = ErrorType.Unknown
        };

        //Demo
        public static void Main(string[] args)
        {
            OneMCL mcl = new OneMCL("E:/BestOwl/Desktop/LauncherTest");
            mcl.UserAuthenticator = new OfflineAuthenticator("MicroOwl");
            LaunchOptionBase message = new LaunchOptionBase("demo")
            {
                gameDir = "E:\\BestOwl\\Desktop\\LauncherTest_GameDir",
                versionId = "1.12.2",
                javaExt = "C:\\Program Files\\Java\\jre-9.0.4\\bin\\javaw.exe"
            };
            LaunchResult result = mcl.Launch(message);
        }

        public OneMCL(string WorkDirPath)
        {
            Core = LauncherCore.Create(WorkDirPath);
        }

        public LaunchResult Launch(LaunchOptionBase launchOption)
        {
            if (UserAuthenticator == null)
            {
                FailedResult.ErrorType = ErrorType.AuthenticationFailed;
                FailedResult.ErrorMessage = "User authenticator no set";
                Console.WriteLine("ERROR: User authenticator no set");
                return FailedResult;
            }
            if (launchOption == null)
            {
                Console.WriteLine("ERROR: Launch message is null");
                FailedResult.ErrorMessage = "Launch message is null";
                return FailedResult;
            }

            Core.JavaPath = launchOption.javaExt;

            //LaunchOptions in KMCCC, different with LaunchOption in OneMCL.Core
            LaunchOptions options = new LaunchOptions()
            {
                Version = Core.GetVersion(launchOption.versionId),
                Authenticator = UserAuthenticator,
                GameDirPath = launchOption.gameDir,
            };

            Console.WriteLine("Launching...");

            return Core.Launch(options, (Action<MinecraftLaunchArguments>)(args =>
            {
                args.AdvencedArguments.Add(launchOption.javaArgs);
                System.Version osVersion = Environment.OSVersion.Version;
                args.AdvencedArguments.Add("-Dos.name=\"" + GetSystemVersionName(osVersion.Major, osVersion.Minor) + "\"");
                args.AdvencedArguments.Add("-Dos.version=" + osVersion.Major + "." + osVersion.Minor);
                args.AdvencedArguments.Add("-Dminecraft.launcher.brand=one-minecraft-launcher");
                //args.AdvencedArguments.Add("-Dminecraft.launcher.version=");
                args.AdvencedArguments.Add("-Dminecraft.client.jar=" + Core.GetVersionJarPath(options.Version));
                //args.AdvencedArguments.Add("-Dlog4j.configurationFile=");
            }));
        }

        public List<MinecraftAssembly> CheckLibraries(KMCCC.Launcher.Version version)
        {
            List<MinecraftAssembly> missing = new List<MinecraftAssembly>();
            List<Library> missingLib = Core.CheckLibraries(version);

            foreach (Library lib in missingLib)
            {
                string dName = lib.Url.Substring(lib.Url.LastIndexOf('/') + 1);
                missing.Add(new MinecraftAssembly
                {
                    Name = dName,
                    Path = Core.GetLibPath(lib),
                    Url = lib.Url
                });
            }

            return missing;
        }

        public List<MinecraftAssembly> CheckNatives(KMCCC.Launcher.Version version)
        {
            List<MinecraftAssembly> missing = new List<MinecraftAssembly>();
            List<Native> missingNav = Core.CheckNatives(version);

            foreach (Native nav in missingNav)
            {
                string dName = nav.Url.Substring(nav.Url.LastIndexOf('/') + 1);
                missing.Add(new MinecraftAssembly
                {
                    Name = dName,
                    Path = Core.GetNativePath(nav),
                    Url = nav.Url
                });
            }

            return missing;
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
