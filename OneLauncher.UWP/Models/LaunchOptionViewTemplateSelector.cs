using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.Models
{
    public class LaunchOptionViewTemplateSelector : DataTemplateSelector
    {

        public DataTemplate OptView { get; set; }
        public DataTemplate OptNullView { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item != null)
            {
                return OptView ?? base.SelectTemplateCore(item);
            }
            else
            {
                return OptNullView ?? base.SelectTemplateCore(item);
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                return OptView ?? base.SelectTemplateCore(item, container);
            }
            else
            {
                return OptNullView ?? base.SelectTemplateCore(item, container);
            }
        }
    }
}
