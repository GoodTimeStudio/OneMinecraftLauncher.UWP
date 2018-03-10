using KMCCC.Authentication;
using KMCCC.Launcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Models
{
    public class LaunchMessage
    {
        public string VersionId;
        [JsonConverter(typeof(AuthenticatorConverter))]
        public IAuthenticator authenticator;


        #region Optional
        public string GameDirPath;
        public string WorkDirPath;
        public string JavaExtPath;
        public string JavaArgs;
        public string AgentPath;

        public int MaxMemory;
        public int MinMemory;

        public ServerInfo Server;
        public WindowSize Size;
        #endregion

    }

    public class AuthenticatorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IAuthenticator authenticator = null;
            JObject obj = JObject.Load(reader);
            string type = obj["type"].ToString();
            string username = obj["username"].ToString();
            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(username))
            {
                switch (type)
                {
                    case "offline":
                        authenticator = new OfflineAuthenticator(username);
                        break;
                    //case "mojang":
                }
            }
            return authenticator;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class AuthenticatorTypeBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = string.Empty;
            typeName = string.Empty;
            if (serializedType == typeof(OfflineAuthenticator))
            {
                typeName = "offline";
            }
            else if (serializedType == typeof(YggdrasilLogin))
            {
                typeName = "mojang";
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            switch (typeName)
            {
                case "offline": return typeof(OfflineAuthenticator);
                case "mojang": return typeof(YggdrasilLogin);
                default: return typeof(OfflineAuthenticator);
            }
        }
    }
}
