using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.Core.Models
{

    public class LaunchOptionBase : BindableBase
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

        private string _versionId;
        public string versionId
        {
            get { return _versionId; }
            set { this.SetProperty(ref _versionId, value); }
        }

        public LaunchOptionBase(string name)
        {
            this.name = name;
        }

    }

}
