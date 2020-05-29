using BiliBili.UWP.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliBili.UWP.Modules.User
{
    public class MyFollowVideoVM : IModules
    {
        readonly Api.User.FollowAPI followAPI;
        public MyFollowVideoVM()
        {
            followAPI = new Api.User.FollowAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _Nothing = false;
        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        private bool _ShowLoadMore = false;
        public bool ShowLoadMore
        {
            get { return _ShowLoadMore; }
            set { _ShowLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
        }

        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private ObservableCollection<FavoriteItemModel> _myFavorite;
        public ObservableCollection<FavoriteItemModel> MyFavorite
        {
            get { return _myFavorite; }
            set { _myFavorite = value; DoPropertyChanged("MyFavorite"); }
        }

        private FavoriteItemModel _currentFavorite;
        public FavoriteItemModel CurrentFavorite
        {
            get { return _currentFavorite; }
            set { _currentFavorite = value; }
        }


        private ObservableCollection<FavoriteItemModel> _collectFavorite;
        public ObservableCollection<FavoriteItemModel> CollectFavorite
        {
            get { return _collectFavorite; }
            set { _collectFavorite = value; DoPropertyChanged("CollectFavorite"); }
        }
        private FavoriteInfoModel _FavoriteInfo;
        public FavoriteInfoModel FavoriteInfo
        {
            get { return _FavoriteInfo; }
            set { _FavoriteInfo = value; DoPropertyChanged("FavoriteInfo"); }
        }
        private ObservableCollection<FavoriteInfoVideoItemModel> _videos;
        public ObservableCollection<FavoriteInfoVideoItemModel> Videos
        {
            get { return _videos; }
            set { _videos = value; DoPropertyChanged("Videos"); }
        }

        public async Task LoadFavorite()
        {
            try
            {
                Loading = true;

                var results = await followAPI.MyFavorite().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JArray>>();
                    if (data.success)
                    {
                        if (data.data[0]["mediaListResponse"] != null)
                        {
                            MyFavorite = await data.data[0]["mediaListResponse"]["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
                            CurrentFavorite = MyFavorite[0];
                            DoPropertyChanged("CurrentFavorite");
                            LoadFavoriteVideos();
                        }
                        if (data.data[1]["mediaListResponse"] != null)
                        {
                            CollectFavorite = await data.data[1]["mediaListResponse"]["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
        public async Task LoadFavoriteVideos()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await followAPI.FavoriteInfo(CurrentFavorite.id, "", Page).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<FavoriteDetailModel>>();
                    if (data.success)
                    {
                        if (Page == 1)
                        {
                            FavoriteInfo = data.data.info;
                            if (data.data.medias == null || data.data.medias.Count == 0)
                            {
                                Nothing = true;
                                return;
                            }
                            Videos = data.data.medias;

                        }
                        else
                        {
                            if (data.data.medias != null)
                            {
                                foreach (var item in data.data.medias)
                                {
                                    Videos.Add(item);
                                }
                            }
                        }
                        if (Videos.Count != FavoriteInfo.media_count)
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
        public async Task<bool> RemoveFavoriteVideo( FavoriteInfoVideoItemModel item)
        {
            try
            {

                var results = await followAPI.RemoveFavorite(CurrentFavorite.fid, item.id).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        Videos.Remove(item);
                        return true;
                    }
                    else
                    {
                        Utils.ShowMessageToast(results.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError(ex);
                Utils.ShowMessageToast(handel.message);
                
            }
            return false;
        }

        public int Page { get; set; } = 1;
        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteVideos();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Videos == null || Videos.Count == 0)
            {
                return;
            }
            await LoadFavoriteVideos();
        }
    }

    public class FavoriteItemModel : INotifyPropertyChanged
    {
        public string cover { get; set; }
        public int attr { get; set; }
        public bool privacy
        {
            get
            {
                return attr == 2;
            }
        }

        public string fid { get; set; }
        public string id { get; set; }
        public int like_state { get; set; }

        public string mid { get; set; }
        public string title { get; set; }
        public int type { get; set; }


        private int _media_count;
        public int media_count
        {
            get { return _media_count; }
            set { _media_count = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("media_count")); }
        }
        public int fav_state { get; set; }
        public bool is_fav
        {
            get
            {
                return fav_state == 1;
            }
            set
            {
                if (value)
                {
                    fav_state = 1;
                }
                else
                {
                    fav_state = 0;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("is_fav"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("fav_state"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class FavoriteInfoVideoItemModel
    {
        public string id { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public FavoriteInfoVideoItemUpperModel upper { get; set; }
        public FavoriteInfoVideoItemStatModel cnt_info { get; set; }
    }
    public class FavoriteDetailModel
    {
        public FavoriteInfoModel info { get; set; }
        public ObservableCollection<FavoriteInfoVideoItemModel> medias { get; set; }
    }
    public class FavoriteInfoModel
    {
        public string cover { get; set; }
        public int attr { get; set; }
        public bool privacy
        {
            get
            {
                return attr == 2;
            }
        }
        public string fid { get; set; }
        public string id { get; set; }
        public int like_state { get; set; }
        public string mid { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public int media_count { get; set; }
        public FavoriteInfoVideoItemUpperModel upper { get; set; }
    }
    public class FavoriteInfoVideoItemUpperModel
    {
        public string face { get; set; }
        public string name { get; set; }
        public string mid { get; set; }
    }
    public class FavoriteInfoVideoItemStatModel
    {
        public int coin { get; set; }
        public int collect { get; set; }
        public int danmaku { get; set; }
        public int play { get; set; }
        public int reply { get; set; }
        public int share { get; set; }
    }
}
