using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public abstract class PacketBase : IServicePacket
    {
        public ILog Logger;

        public PacketBase()
        {
            Logger = LogManager.GetLogger(GetTypeName());
        }

        public abstract string GetTypeName();
        public abstract ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args);
    }
}
