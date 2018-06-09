using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.News
{
    public interface INewsSource
    {
        /// <summary>
        /// Get news list
        /// </summary>
        /// <returns></returns>
        Task<List<NewsContent>> GetNewsAsync();

        /// <summary>
        /// News source
        /// </summary>
        /// <returns></returns>
        string GetSourceName();
    }

    public class NewsContent
    {
        /// <summary>
        /// News title
        /// </summary>
        public string Title;

        /// <summary>
        /// News content text
        /// </summary>
        public string Text;

        /// <summary>
        /// News content image
        /// </summary>
        public ImageSource Image;

        /// <summary>
        /// Open this url when clicking on news content
        /// </summary>
        public string Url;
    }
}
