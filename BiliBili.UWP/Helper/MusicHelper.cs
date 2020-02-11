using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace BiliBili.UWP.Helper
{
    public static class MusicHelper
    {
        public enum MusicPlayMode
        {
            listLoop,//列表循环
            songLoop,//单曲循环
            random,//随机播放
            sequence//顺序
        }
        public static event EventHandler<Visibility> DisplayEvent;
        public static event EventHandler<MusicPlayModel> MediaChanged;
        public static event EventHandler<List<MusicPlayModel>> UpdateList;


        public static List<MusicPlayModel> playList;
        //public static MusicPlayMode musicPlayMode;
        public static MediaPlayer _mediaPlayer;
        public static MediaPlaybackList _mediaPlaybackList;

        public static void InitializeMusicPlay()
        {
            playList = new List<MusicPlayModel>();
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.AutoPlay = true;
            _mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            _mediaPlayer.CommandManager.IsEnabled = true;
            

           
            _mediaPlaybackList = new MediaPlaybackList();
            _mediaPlaybackList.AutoRepeatEnabled = true;
            _mediaPlaybackList.CurrentItemChanged += _mediaPlaybackList_CurrentItemChanged;

            _mediaPlayer.Source = _mediaPlaybackList;
        }

        

        public static void AddToPlay(MusicPlayModel item)
        {
            playList.Add(item);
            _mediaPlaybackList.Items.Add(
                   new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(item.url))));
            if (UpdateList!=null)
            {
                UpdateList(null, playList);
            }

            _mediaPlayer.Play();

        }

        public static void SetPlayList(List<MusicPlayModel> list)
        {
           
            foreach (var item in list)
            {
                _mediaPlaybackList.Items.Add(
                    new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(item.url))));
            }

        }

        public static void ClearMediaList()
        {
            if (_mediaPlayer.PlaybackSession.CanPause)
            {
                _mediaPlayer.Pause();
            }
            _mediaPlaybackList.Items.Clear();
            playList.Clear();

            if (DisplayEvent != null)
            {
                DisplayEvent(null, Visibility.Collapsed);
            }

        }


        private static void _mediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {

            //switch (musicPlayMode)
            //{
            //    case MusicPlayMode.listLoop:
            //        _mediaPlaybackList.ShuffleEnabled = false;
            //        _mediaPlaybackList.AutoRepeatEnabled = true;
            //        break;
            //    case MusicPlayMode.songLoop:

            //        _mediaPlaybackList.MoveTo(_mediaPlaybackList.CurrentItemIndex);
            //        break;
            //    case MusicPlayMode.random:
            //        _mediaPlaybackList.ShuffleEnabled = true;
            //        break;
            //    case MusicPlayMode.sequence:
            //        _mediaPlaybackList.ShuffleEnabled = false;
            //        _mediaPlaybackList.AutoRepeatEnabled = false;
            //        break;
            //    default:
            //        break;
            //}
            if (_mediaPlaybackList.Items.Count==0)
            {
                return;
            }
            if (MediaChanged!=null)
            {
                MediaChanged(sender,playList[Convert.ToInt32(_mediaPlaybackList.CurrentItemIndex)]);
            }
            if (DisplayEvent!=null)
            {
                DisplayEvent(null, Visibility.Visible);
            }


            var _systemMediaTransportControls = _mediaPlayer.SystemMediaTransportControls;

            var timelineProperties = new SystemMediaTransportControlsTimelineProperties();
            // _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            // Fill in the data, using the media elements properties 
            SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Artist = playList[Convert.ToInt32(_mediaPlaybackList.CurrentItemIndex)].artist;
            updater.MusicProperties.Title = playList[Convert.ToInt32(_mediaPlaybackList.CurrentItemIndex)].title;
            updater.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(playList[Convert.ToInt32(_mediaPlaybackList.CurrentItemIndex)].pic));

            updater.Update();

            timelineProperties.StartTime = TimeSpan.FromSeconds(0);
            timelineProperties.MinSeekTime = TimeSpan.FromSeconds(0);
            timelineProperties.Position = _mediaPlayer.PlaybackSession.Position;
            timelineProperties.MaxSeekTime = _mediaPlayer.PlaybackSession.NaturalDuration;
            timelineProperties.EndTime = _mediaPlayer.PlaybackSession.NaturalDuration;

            // Update the System Media transport Controls 
            _systemMediaTransportControls.UpdateTimelineProperties(timelineProperties);
        }


        public static void Play()
        {
            _mediaPlayer.Play();
        }
        public static void Pause()
        {
            try
            {
                if (_mediaPlaybackList.Items.Count!=0&&_mediaPlayer.PlaybackSession.CanPause)
                {
                    _mediaPlayer.Pause();
                    _mediaPlayer.SystemMediaTransportControls.DisplayUpdater.Update();
                }

            }
            catch (Exception)
            {
                //会出现各种莫名其妙的错误catch掉算了- -
                //throw;
            }

        }



        public async static Task<string> GetMusicUri(string id,int quality=1)
        {
            try
            {
                string url = string.Format("https://api.bilibili.com/audio/music-service-c/url?access_key={0}&appkey={1}&build=5370000&mid={2}&mobi_app=android&platform=android&privilege=2&quality={5}&songid={3}&ts={4}",
               ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetUserId(), id, ApiHelper.GetTimeSpan, quality);
                url += "&sign=" + ApiHelper.GetSign(url);

                string re = await WebClientClass.GetResults(new Uri(url));

                JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {

                    List<string> ls = JsonConvert.DeserializeObject<List<string>>(obj["data"]["cdns"].ToString());

                    return ls[0];
                    //player.SetMediaPlayer(MusicHelper._mediaPlayer);
                    //player.Source = new Uri(ls[0]);
                }
                else
                {
                  
                    return null;
                }


            }
            catch (Exception)
            {
               
                return null;
            }
        }


    }


    public class MusicPlayModel
    {
        public string songid { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string pic { get; set; }
        public string url { get; set; }
    }


}
