using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.News
{
    public abstract class RssNews : INewsSource
    {
        public abstract Task<List<NewsContent>> GetNewsAsync();
        public abstract string GetSourceName();
    }
}
