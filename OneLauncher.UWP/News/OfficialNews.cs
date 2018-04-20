using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.UWP.News
{
    // format 1
    public class OfficialNews : INewsSource
    {
        const string SourceUrl = "http://launchermeta.mojang.com/mc/news.json";

        public List<NewsContent> GetNews()
        {
            throw new NotImplementedException();
        }

        public async Task<List<NewsContent>> GetNewsAsync()
        {
            JNews jNews = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(SourceUrl);
                    jNews = JsonConvert.DeserializeObject<JNews>(json);
                }
            }
            catch (JsonException) { }
            catch (HttpRequestException) { }

            if (jNews == null)
            {
                return null;
            }

            if (jNews.format != 1)
            {
                return null;
            }

            List<NewsContent> ret = new List<NewsContent>();
            foreach (JContent con in jNews.entries)
            {
                if (con.tags.Contains("demo"))
                {
                    continue;
                }
                if (con.content == null)
                {
                    continue;
                }

                NewsContent news = new NewsContent
                {
                    Title = con.content.title,
                    Text = con.content.text,
                    Url = con.content.action
                };
                news.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                Uri uri = null;
                Uri.TryCreate(con.content.action, UriKind.Absolute, out uri);
                if (uri != null)
                {
                    news.Image.UriSource = uri;
                }

                ret.Add(news);
            }

            return ret;
        }

        public string GetSourceName()
        {
            return "Offcial";
        }

        public class JNews
        {
            public int format;
            public List<JContent> entries;
        }

        public class JContent
        {
            public string actionType;
            public List<string> tags;
            /// <summary>
            /// Default language (English - US)
            /// </summary>
            [JsonProperty("en-us")]
            public JNewsContent content;
        }

        public class JNewsContent
        {
            public string action;
            public string image;
            public string title;
            public string text;
        }
    }

}
