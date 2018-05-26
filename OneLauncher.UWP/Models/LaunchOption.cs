using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{

    public class LaunchOption : BindableBase
    {
        private string _name;
        public string name
        {
            get { return _name; }
            set { this.SetProperty(ref _name, value); }
        }

        private string _gameDir;
        public string gameDir
        {
            get { return _gameDir; }
            set { this.SetProperty(ref _gameDir, value); }
        }

        private string _javaExt;
        public string javaExt
        {
            get { return _javaExt; }
            set { this.SetProperty(ref _javaExt, value); }
        }

        private string _javaArgs;
        public string javaArgs
        {
            get { return _javaArgs; }
            set { this.SetProperty(ref _javaArgs, value); }
        }

        private DateTime _lastUsed;
        public DateTime lastUsed
        {
            get { return _lastUsed; }
            set
            {
                this.SetProperty(ref _lastUsed, value);
                this.OnPropertyChanged("LastUsedVisbility");
            }
        }

        [JsonIgnore]
        public bool LastUsedVisbility
        {
            get => lastUsed.ToFileTime() > 0;
        }

        private string _lastVersionId;
        public string lastVersionId
        {
            get { return _lastVersionId; }
            set { this.SetProperty(ref _lastVersionId, value); }
        }

        private DateTime _created;
        public DateTime created
        {
            get { return this._created; }
            set
            {
                this.SetProperty(ref _created, value);
                this.OnPropertyChanged("CreateTimeVisbility");
            }
        }

        [JsonIgnore]
        public bool CreateTimeVisbility
        {
            get => created.ToFileTime() > 0;
        }

        [JsonIgnore] public bool isPreview;

        private bool _isReady;
        [JsonIgnore]
        public bool isReady
        {
            get => _isReady;
            set
            {
                this.SetProperty(ref _isReady, value);
                this.OnPropertyChanged(nameof(isNotReady));
            }
        }

        [JsonIgnore]
        public bool isNotReady
        {
            get => !isReady;
        }

        /*
        private bool _isDownloading;
        [JsonIgnore]
        public bool isDownloading
        {
            get => _isDownloading;
            set
            {
                this.SetProperty(ref _isDownloading, value);
                this.OnPropertyChanged(nameof(isNotDownloading));
            }
        }

        [JsonIgnore]
        public bool isNotDownloading
        {
            get => !isDownloading;
        }*/

        public LaunchOption(string name)
        {
            this.name = name;
            this.created = DateTime.Now;
            this.isPreview = false;
            this.isReady = true;
            //this.isDownloading = false;
        }

        //public string icon; TO-DO

    }

    public class DefaultLaunchOption : LaunchOption
    {
        public DefaultLaunchOption() : base(CoreManager.GetStringFromResource("/LaunchOptions/DefaultOptions"))
        {
        }
    }

    public class LaunchOptionTypesBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = string.Empty;
            if (serializedType == typeof(LaunchOption))
            {
                typeName = "custom";
            }
            else if (serializedType == typeof(DefaultLaunchOption))
            {
                typeName = "latest_release";
            }
            else
            {
                typeName = string.Empty;
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            switch(typeName)
            {
                case "latest_release":
                    return typeof(DefaultLaunchOption);
                default:
                    return typeof(LaunchOption);
            }
        }
    }

}
