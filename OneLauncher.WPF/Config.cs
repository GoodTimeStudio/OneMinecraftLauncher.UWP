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

        public static readonly string CONFIG_FILE = "config.json";
        public static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        public string Username;
        public string JavaExt;
        public string JavaArgs;
        public int MaxMemory;
        public string SelectedVersion;
        public string DownloadSourceId;

        public static void LoadFromFile()
        {
            bool needReset = false;

            if (!File.Exists(CONFIG_FILE))
            {
                File.Create(CONFIG_FILE);
                INSTANCE = new Config();
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
                        File.Create(CONFIG_FILE);
                        INSTANCE = new Config();
                    }
                    catch
                    {
                        INSTANCE = new Config();
                    }
                }
            }

            
        }

        public static void SaveConfigToFile()
        {
            File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(INSTANCE, serializerSettings));
        }

        public static async void SaveConfigToFileAsync()
        {
            await Task.Run(() =>
            {
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(INSTANCE, serializerSettings));
            });
        }
    }
}
