using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneLauncher.UWP.Models
{
    public class LaunchMessage
    {
        public string VersionId;
        public JAuthenticator authenticator;


        #region Optional
        public string GameDirPath;
        public string WorkDirPath;
        public string JavaExtPath;
        public string JavaArgs;
        public string AgentPath;

        public int MaxMemory;
        public int MinMemory;

        //public ServerInfo Server;
        //public WindowSize Size;
        #endregion
    }

    public class JAuthenticator
    {
        public string type;
        public string username;
        public string password;
    }
}
