using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBili.UWP.Models
{
    public class ReturnModel<T>
    {
        public bool success { get; set; }
        public string message { get; set; }

        public T data { get; set; }
    }
    public class ReturnModel
    {
        public bool success { get; set; }
        public string message { get; set; }

        public dynamic data { get; set; }
    }



}
