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

        //TO-DO: Move to WPF project
        public static readonly string CONFIG_FILE = "config.json";

        public string Username = "";
        public string Javapath = "";
        public int Maxmemory = 2048;

        public Config()
        {
            INSTANCE = this;
        }

        public static async Task LoadFromFileAsync()
        {
            bool needReset = false;

            if (!File.Exists(CONFIG_FILE))
            {
                File.Create(CONFIG_FILE);
                INSTANCE = new Config();
            }
            else
            {
                using (FileStream stream = File.OpenRead(CONFIG_FILE))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string str = await reader.ReadToEndAsync();
                        if (str != null)
                        {
                            try
                            {
                                var t = JsonConvert.DeserializeObject<Config>(str);
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

        public static async void SaveConfigToFile()
        {
            using (FileStream stream = File.OpenWrite(CONFIG_FILE))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string str = JsonConvert.SerializeObject(INSTANCE);
                    await writer.WriteLineAsync(str);
                }
            }
        }

    }
}
