using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher
{
    public class Config
    {
        public static Config INSTANCE;

        public static readonly string CONFIG_FILE = "onemcl.json";
        public static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        private static object _locker = new object();

        public string User;
        public string Playername;
        public string Password;
        public string ClientToken;
        public string UUID; 
        public string AccountType;
        public string JavaExt;
        public string JavaArgs;
        public int MaxMemory;
        public string SelectedVersion;
        public string DownloadSourceId;

        public static void LoadFromFile()
        {
            lock (_locker)
            {
                bool needReset = false;

                if (!File.Exists(CONFIG_FILE))
                {
                    File.Create(CONFIG_FILE).Dispose();
                    INSTANCE = GenerateDefaultConfig();
                }
                else
                {

                    string str = File.ReadAllText(CONFIG_FILE);
                    if (str != null)
                    {
                        try
                        {
                            var t = JsonConvert.DeserializeObject<Config>(str, serializerSettings);
                            if (t != null)
                                INSTANCE = t;
                            else
                                INSTANCE = new Config();
                        }
                        catch
                        {
                            needReset = true;
                        }
                    }

                    if (needReset)
                    {
                        try
                        {
                            File.Delete(CONFIG_FILE);
                            File.Create(CONFIG_FILE).Dispose();
                            INSTANCE = GenerateDefaultConfig();
                        }
                        catch
                        {
                            INSTANCE = GenerateDefaultConfig();
                        }
                    }
                }
            }
        }

        public static void SaveConfigToFile()
        {
            lock (_locker)
            {
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(INSTANCE, serializerSettings));
            }
        }

        public static async void SaveConfigToFileAsync()
        {
            await Task.Run(() =>
            {
                SaveConfigToFile();
            });
        }

        public static Config GenerateDefaultConfig()
        {
            string player = "Steve";
            return new Config
            {
                User = player,
                Playername = player,
                AccountType = AccountTypes.Offline.Tag
            };
        }
    }
}
