using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Core.Packet
{
    public interface IServicePacket
    {

        string GetTypeName();

        ValueSet OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args);

    }
}
