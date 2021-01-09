﻿using BiliBili.UWP.Controls;
using BiliBili.UWP.Helper;
using BiliBili.UWP.Models;
using BiliBili.UWP.Modules;
using BiliBili.UWP.Pages;
using BiliBili.UWP.Pages.FindMore;
using BiliBili.UWP.Pages.Home;
using BiliBili.UWP.Pages.Music;
using BiliBili.UWP.Pages.User;
using BiliBili.UWP.Views;
using Microsoft.Graphics.Canvas.Effects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace BiliBili.UWP
{
	public enum StartTypes
	{
		None,
		Video,
		Live,
		Bangumi,
		MiniVideo,
		Web,
		File,
		Article,
		Music,
		Album,
		User,
		HandelUri
	}

	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private bool _InBangumi = false;

		private Account account;

		private bool Can_Nav = true;

		private bool IsClicks = false;

		private bool isSetMusic = false;

		private MessageModel message = new MessageModel();

		private DispatcherTimer timer;

		public MainPage()
		{
			this.InitializeComponent();
			MusicHelper.InitializeMusicPlay();
			music.Visibility = Visibility.Collapsed;
			MusicHelper.MediaChanged += MusicHelper_MediaChanged;
			MusicHelper.DisplayEvent += MusicHelper_DisplayEvent;
			MusicHelper.UpdateList += MusicHelper_UpdateList1;
			//ls_music.ItemsSource = MusicHelper.playList;
			musicplayer.SetMediaPlayer(MusicHelper._mediaPlayer);
			MessageCenter.NetworkError += MessageCenter_NetworkError;
			MessageCenter.ShowError += MessageCenter_ShowError;
			SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
			Window.Current.Content.PointerPressed += MainPage_PointerEntered;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (availableSize.Width >= 800)
			{
				SetWideUI();
			}
			else
			{
				SetNarrowUI();
			}
			return base.MeasureOverride(availableSize);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			MessageCenter.ChanageThemeEvent -= MessageCenter_ChanageThemeEvent;
			MessageCenter.MianNavigateToEvent -= MessageCenter_MianNavigateToEvent;
			MessageCenter.InfoNavigateToEvent -= MessageCenter_InfoNavigateToEvent;
			MessageCenter.PlayNavigateToEvent -= MessageCenter_PlayNavigateToEvent;
			MessageCenter.HomeNavigateToEvent -= MessageCenter_HomeNavigateToEvent;
			MessageCenter.BgNavigateToEvent -= MessageCenter_BgNavigateToEvent; ;
			MessageCenter.Logined -= MessageCenter_Logined;

			MessageCenter.ChangeBg -= MessageCenter_ChangeBg;
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (SettingHelper.IsPc())
			{
				sp_View.DisplayMode = SplitViewDisplayMode.CompactOverlay;
			}
			else
			{
				sp_View.DisplayMode = SplitViewDisplayMode.Overlay;
			}
			ChangeTheme();
			timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(0, 0, 5);
			timer.Start();
			timer.Tick += Timer_Tick;
			MessageCenter.ChanageThemeEvent += MessageCenter_ChanageThemeEvent;
			MessageCenter.MianNavigateToEvent += MessageCenter_MianNavigateToEvent;
			MessageCenter.InfoNavigateToEvent += MessageCenter_InfoNavigateToEvent;
			MessageCenter.PlayNavigateToEvent += MessageCenter_PlayNavigateToEvent;
			MessageCenter.HomeNavigateToEvent += MessageCenter_HomeNavigateToEvent;
			MessageCenter.BgNavigateToEvent += MessageCenter_BgNavigateToEvent; ;
			MessageCenter.Logined += MessageCenter_Logined;

			MessageCenter.ChangeBg += MessageCenter_ChangeBg;
			//main_frame.Navigate(typeof(ChannelPage));
			MessageCenter_ChangeBg();
			main_frame.Visibility = Visibility.Visible;
			menu_List.SelectedIndex = 0;
			Can_Nav = false;
			Can_Nav = true;
			frame.Visibility = Visibility.Visible;
			frame.Navigate(typeof(BlankPage));

			play_frame.Visibility = Visibility.Visible;
			play_frame.Navigate(typeof(BlankPage));

			//LoadPlayApiInfo();

			if (e.Parameter != null)
			{
				var m = e.Parameter as StartModel;
				switch (m.StartType)
				{
					case StartTypes.None:
						break;

					case StartTypes.Video:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), m.Par1);
						break;

					case StartTypes.Live:
						MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(LiveRoomPage), m.Par1);
						break;

					case StartTypes.Bangumi:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(BanInfoPage), m.Par1);
						break;

					case StartTypes.MiniVideo:
						//MessageCenter.ShowMiniVideo(m.Par1);
						//MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "http://vc.bilibili.com/mobile/detail?vc=" + m.Par1);
						break;

					case StartTypes.Web:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), m.Par1);
						break;

					case StartTypes.Album:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicInfoPage), m.Par1);
						break;

					case StartTypes.Article:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ArticleContentPage), m.Par1);
						break;

					case StartTypes.Music:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), m.Par1);
						break;

					case StartTypes.User:
						MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(UserCenterPage), m.Par1);
						break;

					case StartTypes.File:
						var files = m.Par3 as IReadOnlyList<IStorageItem>;
						List<PlayerModel> ls = new List<PlayerModel>();
						int i = 1;
						foreach (StorageFile file in files)
						{
							ls.Add(new PlayerModel() { Mode = PlayMode.FormLocal, No = i.ToString(), VideoTitle = "", Title = file.DisplayName, Parameter = file, Aid = file.DisplayName, Mid = file.Path });
							i++;
						}
						play_frame.Navigate(typeof(PlayerPage), new object[] { ls, 0 });
						// MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(PlayerPage), new
						// object[] { ls, 0 });
						break;

					case StartTypes.HandelUri:
						if (!await MessageCenter.HandelUrl(m.Par1))
						{
							ContentDialog contentDialog = new ContentDialog()
							{
								PrimaryButtonText = "确定",
								Title = "不支持跳转的地址"
							};
							TextBlock textBlock = new TextBlock()
							{
								Text = m.Par1,
								IsTextSelectionEnabled = true
							};
							contentDialog.Content = textBlock;
							contentDialog.ShowAsync();
						}
						break;

					default:
						break;
				}
			}

			if (SettingHelper.Get_First())
			{
				TextBlock tx = new TextBlock()
				{
					Text = string.Format(@"{0}", AppHelper.GetLastVersionStr()),
					IsTextSelectionEnabled = true,
					TextWrapping = TextWrapping.Wrap
				};
				await new ContentDialog() { Content = tx, PrimaryButtonText = "知道了" }.ShowAsync();

				SettingHelper.Set_First(false);
			}

			new AppHelper().GetDeveloperMessage();

			account = new Account();
			//检查登录状态
			if (SettingHelper.Get_Access_key() != "")
			{
				if ((await account.CheckLoginState(ApiHelper.access_key)).success)
				{
					MessageCenter_Logined();
					await account.SSO(ApiHelper.access_key);
				}
				else
				{
					var data = await account.RefreshToken(SettingHelper.Get_Access_key(), SettingHelper.Get_Refresh_Token());
					if (!data.success)
					{
						UserManage.Logout();
						Utils.ShowMessageToast("登录过期，请重新登录");
						await Utils.ShowLoginDialog();
					}
				}
			}
		}

		private void btn_CG_Click(object sender, RoutedEventArgs e)
		{
			GC.Collect();
		}

		private void btn_ClearMedia_Click(object sender, RoutedEventArgs e)
		{
			MusicHelper.ClearMediaList();
		}

		private void btn_Down_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(Download2Page));
		}

		private void btn_IKonwn_Click(object sender, RoutedEventArgs e)
		{
			network_error.Visibility = Visibility.Collapsed;
		}

		private async void btn_Login_Click(object sender, RoutedEventArgs e)
		{
			//frame.Navigate(typeof(LoginPage));

			LoginDialog loginDialog = new LoginDialog();
			await loginDialog.ShowAsync();
			fy.Hide();
		}

		private void btn_LogOut_Click(object sender, RoutedEventArgs e)
		{
			UserManage.Logout();
			btn_Login.Visibility = Visibility.Visible;
			btn_UserInfo.Visibility = Visibility.Collapsed;
			gv_User.Visibility = Visibility.Collapsed;
			fy.Hide();
		}

		private void btn_MiniPlayer_Click(object sender, RoutedEventArgs e)
		{
			MessageCenter.SendNavigateTo(NavigateMode.Play, typeof(MusicMiniPlayerPage));
		}

		private void btn_music_List_Click(object sender, RoutedEventArgs e)
		{
			MusicHelper._mediaPlaybackList.ShuffleEnabled = false;
			btn_music_Shuffle.Visibility = Visibility.Visible;
			btn_music_List.Visibility = Visibility.Collapsed;
		}

		private void btn_music_Shuffle_Click(object sender, RoutedEventArgs e)
		{
			MusicHelper._mediaPlaybackList.ShuffleEnabled = true;
			btn_music_Shuffle.Visibility = Visibility.Collapsed;
			btn_music_List.Visibility = Visibility.Visible;
		}

		private void btn_Qr_Click(object sender, RoutedEventArgs e)
		{
			play_frame.Navigate(typeof(QRPage));
		}

		private void btn_Search_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(SearchV2Page));
		}

		private void btn_showMusicInfo_Click(object sender, RoutedEventArgs e)
		{
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(MusicInfoPage), (sender as AppBarButton).Tag.ToString());
		}

		private void btn_Test_Click(object sender, RoutedEventArgs e)
		{
		}

		private void btn_user_moviecollect_Click(object sender, RoutedEventArgs e)
		{
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(FollowSeasonPage), Modules.SeasonType.cinema);
			fy.Hide();
		}

		private void btn_user_mychistory_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(MyHistroryPage));
			fy.Hide();
		}

		private void btn_user_mycollect_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(MyCollectPage));
			fy.Hide();
		}

		private void btn_user_myGuanzhu_Click(object sender, RoutedEventArgs e)
		{
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), "https://space.bilibili.com/h5/follow");
			//frame.Navigate(typeof(UserInfoPage), new object[] { null, 2 });
			fy.Hide();
		}

		private void btn_user_mymessage_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(MyMessagePage));
			fy.Hide();
		}

		private void btn_user_myvip_Click(object sender, RoutedEventArgs e)
		{
			//http://big.bilibili.com/site/big.html
			frame.Navigate(typeof(WebPage), new object[] { "https://big.bilibili.com/mobile/home" });
			fy.Hide();
		}

		private void btn_user_mywallet_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(MyWalletPage));
			fy.Hide();
		}

		private void btn_user_Qr_Click(object sender, RoutedEventArgs e)
		{
			//var info = gv_user.DataContext as UserInfoModel;

			frame.Navigate(typeof(MyQrPage), new object[] { new MyqrModel() {
				  name=Account.myInfo.name,
				  photo=Account.myInfo.face,
				  qr="http://space.bilibili.com/"+ApiHelper.GetUserId(),
				  sex=Account.myInfo.Sex
			} });
			fy.Hide();
		}

		private void btn_user_seasoncollect_Click(object sender, RoutedEventArgs e)
		{
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(FollowSeasonPage), Modules.SeasonType.bangumi);
			fy.Hide();
		}

		private void btn_user_toView_Click(object sender, RoutedEventArgs e)
		{
			MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ToViewPage));
		}

		private void btn_UserInfo_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(UserCenterPage), ApiHelper.GetUserId());
			fy.Hide();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MusicHelper._mediaPlayer.SystemMediaTransportControls.DisplayUpdater.Update();
		}

		private void ChangeTheme()
		{
			switch (SettingHelper.Get_Rigth())
			{
				case 1:
					bg_Frame.Navigate(typeof(FastNavigatePage));
					break;

				case 2:
					bg_Frame.Navigate(typeof(ChannelPage));
					break;

				case 3:
					bg_Frame.Navigate(typeof(RankPage));
					break;

				case 4:
					bg_Frame.Navigate(typeof(TimelinePage));
					break;

				case 5:
					bg_Frame.Navigate(typeof(LiveAllPage));
					break;

				default:
					bg_Frame.Navigate(typeof(BlankPage));
					break;
			}

			string ThemeName = SettingHelper.Get_Theme();
			if (ThemeName == "Dark")
			{
				RequestedTheme = ElementTheme.Dark;
			}
			else
			{
				ResourceDictionary newDictionary = new ResourceDictionary();
				newDictionary.Source = new Uri($"ms-appx:///Theme/{ThemeName}Theme.xaml", UriKind.RelativeOrAbsolute);
				Application.Current.Resources.ThemeDictionaries["Light"] = newDictionary;
				RequestedTheme = ElementTheme.Dark;
				RequestedTheme = ElementTheme.Light;
			}
			//tuic.To = this.ActualWidth;
			//storyboardPopOut.Begin();
			ChangeTitbarColor();
		}

		private void ChangeTitbarColor()
		{
			var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
			titleBar.BackgroundColor = ((SolidColorBrush)grid_Top.Background).Color;
			titleBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);//Colors.White纯白用不了。。。
			titleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)sp_View.PaneBackground).Color;
			titleBar.ButtonBackgroundColor = ((SolidColorBrush)grid_Top.Background).Color;
			titleBar.ButtonForegroundColor = Color.FromArgb(255, 254, 254, 254);
			titleBar.InactiveBackgroundColor = ((SolidColorBrush)grid_Top.Background).Color;
			titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)grid_Top.Background).Color;
		}

		private void dtzz_Click(object sender, RoutedEventArgs e)
		{
			frame.Navigate(typeof(WebPage), "https://account.bilibili.com/answer/base");
			fy.Hide();
		}

		private void frame_Navigated(object sender, NavigationEventArgs e)
		{
			if (frame.CanGoBack)
			{
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			}
			else
			{
				if (!play_frame.CanGoBack)
				{
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
				}
			}

			//if ((frame.Content as Page).Tag == null)
			//{
			//    frame.Background = App.Current.Resources["Bili-Background"] as SolidColorBrush;
			//    return;
			//}

			//if ((frame.Content as Page).Tag.ToString()!= "blank")
			//{
			//    frame.Background = App.Current.Resources["Bili-Background"] as SolidColorBrush;
			//}
			//else
			//{
			//    frame.Background =null;
			//}
			if ((main_frame.Content as Page).Tag == null)
			{
				return;
			}
			switch ((main_frame.Content as Page).Tag.ToString())
			{
				case "Cn":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "国漫";
					break;

				case "Jp":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "番剧";
					break;

				case "Music":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "音频";
					break;

				case "Article":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "专栏";
					break;

				default:
					break;
			}

			//switch ((frame.Content as Page).Tag.ToString())
			//{
			//    case "blank":c
			//        //Background="{ThemeResource Bili-Background}"

			//        break;
			//}
		}

		private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
		{
			if (this.ActualWidth >= 800)
			{
				if (e.SourcePageType != typeof(BlankPage))
				{
					SetTwoColumn();
				}
				else if (main_frame.SourcePageType == typeof(HomePage))
				{
					if (SettingHelper.Get_ColunmHome())
					{
						SetTwoColumn();
					}
					else
					{
						SetOneColumn();
					}
				}
				else
				{
					SetTwoColumn();
				}
			}
		}

		private async Task<bool> HasMessage()
		{
			try
			{
				if (!ApiHelper.IsLogin())
				{
					return false;
				}
				// http://message.bilibili.com/api/msg/query.room.list.do?access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&page_size=100&platform=android&ts=1461404884000&sign=5e212e424761aa497a75b0fb7fbde775
				string url = string.Format("http://message.bilibili.com/api/notify/query.notify.count.do?_device=wp&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=5250000&platform=android&ts={2}", ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan);
				url += "&sign=" + ApiHelper.GetSign(url);
				string results = await WebClientClass.GetResults(new Uri(url));
				MessageModel model = JsonConvert.DeserializeObject<MessageModel>(results);

				if (model.code == 0)
				{
					MessageModel list = JsonConvert.DeserializeObject<MessageModel>(model.data.ToString());
					message = list;
					if (list.reply_me != 0 || list.chat_me != 0 || list.notify_me != 0 || list.praise_me != 0 || list.at_me != 0)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
				//Utils.ShowMessageToast("读取通知失败", 3000);
			}
		}

		private void InitializedFrostedGlass(UIElement glassHost, int d)
		{
			Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
			Compositor compositor = hostVisual.Compositor;

			// Create a glass effect, requires Win2D NuGet package
			var glassEffect = new GaussianBlurEffect
			{
				BlurAmount = d * 5.0f,
				BorderMode = EffectBorderMode.Hard,
				Source = new ArithmeticCompositeEffect
				{
					MultiplyAmount = 0,
					Source1Amount = 0.5f,
					Source2Amount = 0.5f,
					Source1 = new CompositionEffectSourceParameter("backdropBrush"),
					Source2 = new ColorSourceEffect
					{
						Color = Color.FromArgb(255, 245, 245, 245)
					}
				}
			};

			// Create an instance of the effect and set its source to a CompositionBackdropBrush
			var effectFactory = compositor.CreateEffectFactory(glassEffect);
			var backdropBrush = compositor.CreateBackdropBrush();
			var effectBrush = effectFactory.CreateBrush();

			effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

			// Create a Visual to contain the frosted glass effect
			var glassVisual = compositor.CreateSpriteVisual();
			glassVisual.Brush = effectBrush;

			// Add the blur as a child of the host in the visual tree
			ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);

			// Make sure size of glass host and glass visual always stay in sync
			var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
			bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

			glassVisual.StartAnimation("Size", bindSizeAnimation);
		}

		private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = (e.ClickedItem as StackPanel).Tag.ToString();
			if (item == "ToView")
			{
				if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
				{
					Utils.ShowMessageToast("请先登录");
					return;
				}
				MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(ToViewPage));
			}
			else if (item == "Test")
			{
			}
			else
			{
				MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(SettingPage));
			}
		}

		private void ls_music_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!isSetMusic)
			{
				MusicHelper._mediaPlaybackList.MoveTo(Convert.ToUInt32(ls_music.SelectedIndex));
			}
		}

		private void main_frame_Navigated(object sender, NavigationEventArgs e)
		{
			if ((main_frame.Content as Page).Tag == null)
			{
				return;
			}
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			Can_Nav = false;
			_InBangumi = false;
			switch ((main_frame.Content as Page).Tag.ToString())
			{
				case "首页":
					menu_List.SelectedIndex = 0;
					txt_Header.Text = "首页";
					break;

				case "频道":
					menu_List.SelectedIndex = 1;
					txt_Header.Text = "频道";
					break;

				case "直播":
					menu_List.SelectedIndex = 2;
					txt_Header.Text = "直播";
					break;

				case "番剧":
					menu_List.SelectedIndex = 3;

					txt_Header.Text = "番剧";
					break;

				case "动态":
					menu_List.SelectedIndex = 4;

					//menu_List.SelectedIndex = 3;
					//bottom.SelectedIndex = 3;
					txt_Header.Text = "动态";
					break;

				case "发现":
					menu_List.SelectedIndex = 5;

					//menu_List.SelectedIndex = 4;
					//bottom.SelectedIndex = 4;
					txt_Header.Text = "发现";
					break;

				case "设置":
					menu_List.SelectedIndex = 6;
					txt_Header.Text = "设置";
					break;

				case "Cn":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "国漫";
					break;

				case "Jp":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "番剧";
					break;

				case "Music":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "音频";
					break;

				case "Article":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "专栏";
					break;

				default:
					break;
			}
			Can_Nav = true;
			// }
		}

		private void main_frame_Navigating(object sender, NavigatingCancelEventArgs e)
		{
			if (this.ActualWidth >= 800)
			{
				if (e.SourcePageType != typeof(HomePage))
				{
					SetTwoColumn();
				}
				else if (frame.SourcePageType != typeof(BlankPage))
				{
					SetTwoColumn();
				}
				else
				{
					if (SettingHelper.Get_ColunmHome())
					{
						SetTwoColumn();
					}
					else
					{
						SetOneColumn();
					}
				}
			}
		}

		private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
		{
			if (play_frame.CanGoBack)
			{
				e.Handled = true;
				play_frame.GoBack();
			}
			else
			{
				if (frame.CanGoBack)
				{
					e.Handled = true;
					frame.GoBack();
				}
				else
				{
					if (_InBangumi)
					{
						e.Handled = true;
						main_frame.GoBack();
					}
					else
					{
						if (e.Handled == false)
						{
							if (IsClicks)
							{
								Application.Current.Exit();
							}
							else
							{
								IsClicks = true;
								e.Handled = true;
								Utils.ShowMessageToast("再按一次退出应用", 1500);
								await Task.Delay(1500);
								IsClicks = false;
							}
						}
					}
				}
			}
		}

		private async void MainPage_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			var par = e.GetCurrentPoint(sender as Frame).Properties.PointerUpdateKind;
			if (par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed || par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed)
			{
				if (!SettingHelper.Get_MouseBack())
				{
					return;
				}
				if (play_frame.CanGoBack)
				{
					e.Handled = true;
					play_frame.GoBack();
				}
				else
				{
					if (frame.CanGoBack)
					{
						e.Handled = true;
						frame.GoBack();
					}
					else
					{
						if (_InBangumi)
						{
							e.Handled = true;
							main_frame.GoBack();
						}
						else
						{
							if (e.Handled == false)
							{
								if (IsClicks)
								{
									Application.Current.Exit();
								}
								else
								{
									IsClicks = true;
									e.Handled = true;
									Utils.ShowMessageToast("再按一次退出应用", 1500);
									await Task.Delay(1500);
									IsClicks = false;
								}
							}
						}
					}
				}
			}
		}

		private void menu_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!Can_Nav)
			{
				return;
			}

			switch (menu_List.SelectedIndex)
			{
				case 0:
					//if (SettingHelper.Get_NewFeed())
					//{
					//
					//}
					//else
					//{
					main_frame.Navigate(typeof(HomePage));
					//}
					//if (!Reg_OpenVideo)
					//{
					//    (main_frame.Content as ChannelPage).OpenVideo += MainPage_OpenVideo;
					//    Reg_OpenVideo = true;
					//}
					txt_Header.Text = "首页";
					break;

				case 1:
					main_frame.Navigate(typeof(ChannelPage));

					txt_Header.Text = "频道";
					break;

				case 2:
					main_frame.Navigate(typeof(LiveV2Page));

					txt_Header.Text = "直播";
					break;

				case 3:
					main_frame.Navigate(typeof(BangumiPage));

					txt_Header.Text = "番剧";
					break;

				case 4:
					main_frame.Navigate(typeof(AttentionPage));

					txt_Header.Text = "动态";
					break;

				case 5:
					main_frame.Navigate(typeof(FindPage));

					txt_Header.Text = "发现";
					break;
				//case 4:
				//    main_frame.Navigate(typeof(SettingPage));

				// txt_Header.Text = "设置"; break;
				default:
					break;
			}
			sp_View.IsPaneOpen = false;
		}

		private void MessageCenter_BgNavigateToEvent(Type page, params object[] par)
		{
			bg_Frame.Navigate(page, par);
		}

		private void MessageCenter_ChanageThemeEvent(object par, params object[] par1)
		{
			ChangeTheme();
		}

		private async void MessageCenter_ChangeBg()
		{
			if (SettingHelper.Get_CustomBG() && SettingHelper.Get_BGPath().Length != 0)
			{
				StorageFile file = await StorageFile.GetFileFromPathAsync(SettingHelper.Get_BGPath());
				if (file != null)
				{
					img_bg.Stretch = (Stretch)SettingHelper.Get_BGStretch();
					img_bg.HorizontalAlignment = (HorizontalAlignment)SettingHelper.Get__BGHor();
					img_bg.VerticalAlignment = (VerticalAlignment)SettingHelper.Get_BGVer();
					img_bg.Opacity = Convert.ToDouble(SettingHelper.Get_BGOpacity()) / 10;

					if (SettingHelper.Get_BGMaxWidth() != 0)
					{
						img_bg.MaxWidth = SettingHelper.Get_BGMaxWidth();
					}
					else
					{
						img_bg.MaxWidth = double.PositiveInfinity;
					}
					if (SettingHelper.Get_BGMaxHeight() != 0)
					{
						img_bg.MaxHeight = SettingHelper.Get_BGMaxHeight();
					}
					else
					{
						img_bg.MaxHeight = double.PositiveInfinity;
					}

					var st = await file.OpenReadAsync();
					BitmapImage bit = new BitmapImage();
					await bit.SetSourceAsync(st);
					img_bg.Source = bit;
					if (SettingHelper.Get_FrostedGlass() != 0)
					{
						GlassHost.Visibility = Visibility.Visible;
						InitializedFrostedGlass(GlassHost, SettingHelper.Get_FrostedGlass());
					}
					else
					{
						GlassHost.Visibility = Visibility.Collapsed;
					}
				}
				else
				{
				}
			}
			else
			{
				img_bg.Source = null;
			}
		}

		private void MessageCenter_HomeNavigateToEvent(Type page, params object[] par)
		{
			main_frame.Navigate(page, par);
		}

		private void MessageCenter_InfoNavigateToEvent(Type page, params object[] par)
		{
			frame.Navigate(page, par);
		}

		private async void MessageCenter_Logined()
		{
			btn_Login.Visibility = Visibility.Collapsed;
			btn_UserInfo.Visibility = Visibility.Visible;
			gv_User.Visibility = Visibility.Visible;
			try
			{
				var data = await account.GetMyInfo();
				if (data.success)
				{
					var m = data.data;
					gv_user.DataContext = data.data;
					if (m.rank == 0 || m.rank == 5000)
					{
						dtzz.Visibility = Visibility.Visible;
					}
					else
					{
						dtzz.Visibility = Visibility.Collapsed;
					}
					if (m.vip != null && m.vip.type != 0)
					{
						img_VIP.Visibility = Visibility.Visible;
						SettingHelper.Set_UserIsVip(true);
					}
					else
					{
						img_VIP.Visibility = Visibility.Collapsed;
						SettingHelper.Set_UserIsVip(false);
					}
				}
				else
				{
					Utils.ShowMessageToast(data.message);
				}
			}
			catch (Exception)
			{
			}
		}

		private void MessageCenter_MianNavigateToEvent(Type page, params object[] par)
		{
			this.Frame.Navigate(page, par);
		}

		private void MessageCenter_NetworkError(object sender, string e)
		{
			network_error.Visibility = Visibility.Visible;
		}

		private void MessageCenter_PlayNavigateToEvent(Type page, params object[] par)
		{
			if (SettingHelper.Get_NewWindow())
			{
				MessageCenter.OpenNewWindow(page, par);
			}
			else
			{
				play_frame.Navigate(page, par);
			}
		}

		private void MessageCenter_ShowError(object sender, Exception e)
		{
			//弹窗体验太差
			//try
			//{
			//    //ErrorDialog errorDialog = new ErrorDialog(e);
			//    //await errorDialog.ShowAsync();
			//}
			//catch (Exception)
			//{
			//}
		}

		private async void MusicHelper_DisplayEvent(object sender, Visibility e)
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				music.Visibility = e;
			});
		}

		private async void MusicHelper_MediaChanged(object sender, MusicPlayModel e)
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				isSetMusic = true;
				btn_showMusicInfo.Tag = e.songid;
				music_img.Source = new BitmapImage(new Uri(e.pic));
				txt_musicInfo.Text = e.title + " - " + e.artist;
				ls_music.SelectedIndex = MusicHelper._mediaPlaybackList.CurrentItemIndex.ToInt32();
				isSetMusic = false;
			});
		}

		private async void MusicHelper_UpdateList1(object sender, List<MusicPlayModel> e)
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				//btn_MediaList.Flyout.ShowAt(btn_MediaList);
				isSetMusic = true;

				//ls_music.ItemsSource =e; 设置ItemsSource会出现bug
				ls_music.Items.Clear();
				foreach (var item in e)
				{
					ls_music.Items.Add(item);
				}
				ls_music.SelectedIndex = MusicHelper._mediaPlaybackList.CurrentItemIndex.ToInt32();
				ls_music.UpdateLayout();
				isSetMusic = false;
				//btn_MediaList.Flyout.Hide();
			});
		}

		private void piv_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
		}

		private void play_frame_Navigated(object sender, NavigationEventArgs e)
		{
			if (play_frame.CanGoBack)
			{
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			}
			else
			{
				if (!frame.CanGoBack)
				{
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
				}
			}
			if ((main_frame.Content as Page).Tag == null)
			{
				return;
			}
			switch ((main_frame.Content as Page).Tag.ToString())
			{
				case "Cn":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "国漫";
					break;

				case "Jp":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "番剧";
					break;

				case "Music":
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
					_InBangumi = true;
					txt_Header.Text = "音频";
					break;

				default:
					break;
			}
		}

		//}
		//设置宅布局
		private void SetNarrowUI()
		{
			grid_o.BorderThickness = new Thickness(0, 0, 1, 0);

			bg.Visibility = Visibility.Collapsed;
			sp_View.DisplayMode = SplitViewDisplayMode.CompactOverlay;
			Grid.SetColumn(frame, 0);
			btn_OpenMenu.Visibility = Visibility.Visible;
			SetOneColumn();
		}

		//显示单列
		private void SetOneColumn()
		{
			column_left.MaxWidth = double.MaxValue;
			column_right.Width = new GridLength(0);
		}

		//显示双列
		private void SetTwoColumn()
		{
			column_left.MaxWidth = 500;
			column_left.Width = new GridLength(1, GridUnitType.Star);
			column_right.Width = new GridLength(1, GridUnitType.Star);
		}

		//        if (menu_List.SelectedIndex == 4)
		//        {
		//            menu_List.SelectedIndex = 0;
		//        }
		//        else
		//        {
		//            menu_List.SelectedIndex = menu_List.SelectedIndex + 1;
		//        }
		//        await Task.Delay(200);
		//        piv.SelectedIndex = 1;
		//        return;
		//    }
		//设置宽布局
		private void SetWideUI()
		{
			grid_o.BorderThickness = new Thickness(0, 0, 1, 0);

			sp_View.DisplayMode = SplitViewDisplayMode.CompactOverlay;
			Grid.SetColumn(frame, 1);
			bg.Visibility = Visibility.Visible;
			btn_OpenMenu.Visibility = Visibility.Visible;
			if (frame.CurrentSourcePageType == typeof(BlankPage) && main_frame.CurrentSourcePageType == typeof(HomePage))
			{
				if (SettingHelper.Get_ColunmHome())
				{
					SetTwoColumn();
				}
				else
				{
					SetOneColumn();
				}
			}
			else
			{
				SetTwoColumn();
			}
		}

		private void Storyboard_Completed(object sender, object e)
		{
			row_bottom.Height = new GridLength(0);
		}

		/// <summary>
		/// 重新是否有新的信息
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Timer_Tick(object sender, object e)
		{
			//if (ApiHelper.IsLogin())
			//{
			if (await HasMessage())
			{
				await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					//menu_bor_HasMessage

					bor_TZ.Visibility = Visibility.Visible;
				});
			}
			else
			{
				await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					bor_TZ.Visibility = Visibility.Collapsed;
				});
			}
		}

		//private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//    //piv.SelectedIndex += 1;

		// if (piv.SelectedIndex == 0) { if (menu_List.SelectedIndex == 0) { menu_List.SelectedIndex
		// = 4; } else { menu_List.SelectedIndex = menu_List.SelectedIndex - 1; } await
		// Task.Delay(200); piv.SelectedIndex = 1; return;

		// } if (piv.SelectedIndex == 2) {
	}

	public class StartModel
	{
		public string Par1 { get; set; }
		public string Par2 { get; set; }
		public object Par3 { get; set; }
		public StartTypes StartType { get; set; }
	}
}