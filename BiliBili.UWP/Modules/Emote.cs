using BiliBili.UWP.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace BiliBili.UWP.Modules
{
    public enum EmoteMode
    {
        //评论
        reply,
        //动态
        dynamic
    }
    public class Emote : IModules
    {
        private EmoteMode _emoteMode;
        public Emote(EmoteMode emoteMode)
        {
            _emoteMode = emoteMode;
        }
        public async Task<ReturnModel<ObservableCollection<EmotePackage>>> LoadEmote(int id = 0)
        {
            try
            {
                var url = ApiHelper.GetSignWithUrl($"https://api.bilibili.com/x/emote/user/panel?access_key={ ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&business={(_emoteMode == EmoteMode.dynamic ? "dynamic" : "reply")}&mobi_app=android&platform=android&ts={ApiHelper.GetTimeSpan}", ApiHelper.AndroidKey);
                if (id != 0)
                {
                    url = ApiHelper.GetSignWithUrl($"https://api.bilibili.com/x/emote/package?access_key={ ApiHelper.access_key}&appkey={ApiHelper.AndroidKey.Appkey}&build={ApiHelper.build}&business={(_emoteMode == EmoteMode.dynamic ? "dynamic" : "reply")}&mobi_app=android&platform=android&ids={id}&ts={ApiHelper.GetTimeSpan}", ApiHelper.AndroidKey);
                }

                var results = await WebClientClass.GetResults(new Uri(url));
                var obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    ObservableCollection<EmotePackage> emotePackages = JsonConvert.DeserializeObject<ObservableCollection<EmotePackage>>(obj["data"]["packages"].ToString());
                    return new ReturnModel<ObservableCollection<EmotePackage>>()
                    {
                        success = true,
                        data = emotePackages
                    };
                }
                else
                {
                    return new ReturnModel<ObservableCollection<EmotePackage>>()
                    {
                        success = false,
                        message = obj["message"].ToString()
                    };
                }

            }
            catch (Exception ex)
            {
                return HandelError<ObservableCollection<EmotePackage>>(ex);
            }
        }

    }

    public class EmotePackage
    {
        public int id { get; set; }
        public int type { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public List<EmoteItem> emote { get; set; }
    }
    public class EmoteItem
    {
        public int id { get; set; }
        public int package_id { get; set; }
        public int type { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public bool isFace
        {
            get
            {
                return type != 4;
            }
        }
        public bool isText
        {
            get
            {
                return type == 4;
            }
        }
    }

}
