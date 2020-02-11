using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;

namespace BiliBili.UWP.Helper
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public static class LogHelper
    {
        public  static void WriteLog(Exception exception)
        {
            try
            {
                if (IsNetworkError(exception))
                {
                    return;
                }
               
            }
            catch (Exception)
            {
            }

        }

        public  static bool IsNetworkError(Exception ex)
        {
            if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
            {
                MessageCenter.SendNetworkError(ex.Message);
                return true;
            }
            {
                return false;
            }
        }

    }
}
