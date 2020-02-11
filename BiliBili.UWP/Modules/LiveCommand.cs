using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.Live;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules
{
    public class LiveCommand
    {
        public LiveCommand()
        {
            NavigationInfoPageCommand = new RelayCommand<string>(NavigationInfoPage);
            HandelLiveUrlNavigationCommand = new RelayCommand<string>(HandelLiveUrl);
            OpenLiveRoomCommand = new RelayCommand<int>(OpenLiveRoom);
        }
        public ICommand NavigationInfoPageCommand { get; private set; }
        public ICommand HandelLiveUrlNavigationCommand { get; private set; }
        public ICommand OpenLiveRoomCommand { get; private set; }
        public virtual void NavigationInfoPage(string pageName)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, Type.GetType(pageName));
        }

        public virtual async void HandelLiveUrl(string url)
        {
            if (await MessageCenter.HandelUrl(url))
            {
                return;
            }
            if (url.Contains("app/all-live"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LiveAllPage));
                return;
            }
            if (url.Contains("app/area"))
            {
               
                var name = Regex.Match(url+"&", "&area_name=(.*?)&",RegexOptions.Singleline).Groups[1].Value;
                if (name.Length==0)
                {
                    name = Regex.Match(url + "&", "parent_area_name=(.*?)&", RegexOptions.Singleline).Groups[1].Value;
                }
                
                //http://live.bilibili.com/app/area?parent_area_id=5&parent_area_name=%E7%94%B5%E5%8F%B0&area_id=0&area_name=
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LivePartInfoPage),new object[] {
                    Regex.Match(url, @"parent_area_id=(\d+)").Groups[1].Value,
                    Regex.Match(url, @"&area_id=(\d+)").Groups[1].Value,
                    name
                });
                return;
            }
            if (url.Contains("app/mytag"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(LivePartPage));
                return;
            }

            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), url);
        }
        public virtual void OpenLiveRoom(int roomid)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPC), roomid);
        }
    }
}
