using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Media.Core;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace BiliBili.UWP.Helper
{
    public enum NetworkType
    {
        Wifi,
        Unknow,
        Other,
        NotNetwork
    }
    public static class SystemHelper
    {
        public static List<string> GetSystemFontFamilies()
        {
            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            return fonts.OrderBy(x=>x).ToList();
        }
        /// <summary>
        /// 网络是否可用
        /// </summary>
        public static bool IsNetworkAvailable
        {
            get
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                return (profile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            }
        }

        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <returns>IP地址</returns>
        public static string GetIpAddress()
        {
            Guid? networkAdapterId = NetworkInformation.GetInternetConnectionProfile()?.NetworkAdapter?.NetworkAdapterId;
            return (networkAdapterId.HasValue ? NetworkInformation.GetHostNames().FirstOrDefault(hn => hn?.IPInformation?.NetworkAdapter.NetworkAdapterId == networkAdapterId)?.CanonicalName : null);
        }

        /// <summary>
        /// 获取网络运营商信息
        /// </summary>
        /// <returns></returns>
        public static string GetNetworkName()
        {
            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                {
                    if (profile.IsWwanConnectionProfile)
                    {
                        var homeProviderId = profile.WwanConnectionProfileDetails.HomeProviderId;
                        //4600是我手机测试出来的。
                        if (homeProviderId == "46000" || homeProviderId == "46002" || homeProviderId == "4600")
                        {
                            return "中国移动";
                        }
                        //已验证
                        else if (homeProviderId == "46001")
                        {
                            return "中国联通";
                        }
                        //貌似还没win10 电信手机。。待验证
                        else if (homeProviderId == "46003")
                        {
                            return "中国电信";
                        }
                    }
                    else
                    {
                        return "其他";
                    }
                    //也可以用下面的方法，已验证移动和联通
                    //var name = profile.GetNetworkNames().FirstOrDefault();
                    //if (name != null)
                    //{
                    //    name = name.ToUpper();
                    //    if (name == "CMCC")
                    //    {
                    //        return "中国移动";
                    //    }
                    //    else if (name == "UNICOM")
                    //    {
                    //        return "中国联通";
                    //    }
                    //    else if (name == "TELECOM")
                    //    {
                    //        return "中国电信";
                    //    }
                    //}
                    //return "其他";
                }

                return "其他";
            }
            catch (Exception)
            {

                return "其他";
            }

        }


        /// <summary>
        /// 获取网络连接类型
        /// </summary>
        /// <returns></returns>
        public static NetworkType GetNetWorkType()
        {
            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null)
                {
                    return NetworkType.Unknow;
                }
                if (profile.IsWwanConnectionProfile)
                {
                    WwanDataClass connectionClass = profile.WwanConnectionProfileDetails.GetCurrentDataClass();
                    switch (connectionClass)
                    {
                        //2G-equivalent
                        case WwanDataClass.Edge:
                        case WwanDataClass.Gprs:
                            return NetworkType.Other;
                        //3G-equivalent
                        case WwanDataClass.Cdma1xEvdo:
                        case WwanDataClass.Cdma1xEvdoRevA:
                        case WwanDataClass.Cdma1xEvdoRevB:
                        case WwanDataClass.Cdma1xEvdv:
                        case WwanDataClass.Cdma1xRtt:
                        case WwanDataClass.Cdma3xRtt:
                        case WwanDataClass.CdmaUmb:
                        case WwanDataClass.Umts:
                        case WwanDataClass.Hsdpa:
                        case WwanDataClass.Hsupa:
                            return NetworkType.Other;
                        //4G-equivalent
                        case WwanDataClass.LteAdvanced:
                            return NetworkType.Other;

                        //not connected
                        case WwanDataClass.None:
                            return NetworkType.NotNetwork;

                        //unknown
                        case WwanDataClass.Custom:
                        default:
                            return NetworkType.Unknow;
                    }
                }
                else if (profile.IsWlanConnectionProfile)
                {
                    return NetworkType.Wifi;
                }
                return NetworkType.Unknow;
            }
            catch (Exception)
            {
                return NetworkType.Unknow; //as default
            }

        }
        ////此代码非常不稳定，会出现Win32错误引起闪退
        //public static async Task<bool> CheckCodec()
        //{
        //    try
        //    {             
        //        if (GetSystemBuild()>= 17763)
        //        {   
        //            var codecQuery = new CodecQuery();
        //            IReadOnlyList<CodecInfo> result = await codecQuery.FindAllAsync(CodecKind.Video, CodecCategory.Decoder, "");
        //            return result.Any(x => x.DisplayName == "HEVCVideoExtension");
        //        }
        //        else
        //        {
        //            return false;
        //        }  
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        public static ulong SystemBuildVersion = 0;
        public static ulong GetSystemBuild()
        {
            if (SystemBuildVersion==0)
            {
                SystemBuildVersion = (ulong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion) & 0x00000000FFFF0000L) >> 16;
            }
          
            return SystemBuildVersion;
        }
        public static string SystemVersion()
        {
            ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            var OperatingSystemVersion = new OSVersion
            {
                Major = (ushort)((version & 0xFFFF000000000000L) >> 48),
                Minor = (ushort)((version & 0x0000FFFF00000000L) >> 32),
                Build = (ushort)((version & 0x00000000FFFF0000L) >> 16),
                Revision = (ushort)(version & 0x000000000000FFFFL)
            };

            return OperatingSystemVersion.ToString();

        }
    }

}
