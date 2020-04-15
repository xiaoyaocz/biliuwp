using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using System.ComponentModel;
using Windows.UI.Xaml.Media;
using Windows.UI;
using BiliBili.UWP.Api.Home;
using BiliBili.UWP.Api;

namespace BiliBili.UWP.Modules.Home
{
    public class HomeVM : IModules
    {
        readonly HomeAPI homeAPI;
        RecommendVM recommendVM;
        public ObservableCollection<HomeTabItem> Tabs { get; set; }
        public HomeVM()
        {
            homeAPI = new HomeAPI();
            recommendVM = new RecommendVM();
            Tabs = new ObservableCollection<HomeTabItem>() { 
                new HomeTabItem()
                {
                    name="推荐",
                    item=new RecommendVM()
                },
                new HomeTabItem()
                {
                    name="热门",
                    item=new HotVM()
                }
            };
        }

     

        /// <summary>
        /// 读取首页TAB信息
        /// </summary>
        /// <returns></returns>
        public async Task GetTab()
        {
            try
            {
                var result =await homeAPI.Tab().Request();
                var model = await result.GetData<JObject>();
                if (model.code == 0)
                {
                    var tabs = JsonConvert.DeserializeObject<ObservableCollection<HomeTabItem>>(model.data["tab"].ToString());
                    foreach (var item in tabs.Where(x=>x.tab_id.ToInt32()!=0))
                    {
                        item.item = new TopicVM();
                        Tabs.Add(item);
                    }
                }
                else
                {
                    Utils.ShowMessageToast("读取主页TAB失败:"+model.message);
                    
                }

            }
            catch (Exception ex)
            {
               var result= HandelError(ex);
                Utils.ShowMessageToast(result.message);
            }
        }

  
    }
    
    public class HomeTabItem
    {
        public IModules item { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string tab_id { get; set; }
    }
    
  
  

  
}
