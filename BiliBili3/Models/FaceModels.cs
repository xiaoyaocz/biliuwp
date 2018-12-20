using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiliBili3.Models
{
    public class FaceModel
    {
        public int code
        {
            get; set;
        }
        public List<FaceModel> data { get; set; }
        public int pid { get; set; }
        public string pname { get; set; }
        public int pstate { get; set; }
        public string purl { get; set; }
        public List<EmojiModel> emojis { get; set; }
    }
    public class EmojiModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int state { get; set; }
        public string url { get; set; }
        public string remark { get; set; }
        public string vname
        {
            get
            {
                return Regex.Match(name, @"\[.*?_(.*?)\]").Groups[1].Value;
            }
        }
    }
}
