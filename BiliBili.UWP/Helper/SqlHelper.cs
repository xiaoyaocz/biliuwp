using SQLite;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliBili.UWP
{
    public static class SqlHelper
    {
        /// <summary>
        /// 数据库文件所在路径
        /// </summary>
        public readonly static string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "RRMJData.db");

        public static SQLiteConnection GetDbConnection()
        {
            // 连接数据库，如果数据库文件不存在则创建一个空数据库。
            var conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);

            conn.CreateTable<HistoryClass>();
            conn.CreateTable<ViewPostHelperClass>();
            conn.CreateTable<DownloadGuidClass>();
            //  conn.CreateTable<CommicCollectHelperClass>();
            return conn;
        }


        #region viewHistory
        public static List<HistoryClass> GetHistoryList(int mode)
        {

            List<HistoryClass> my = new List<HistoryClass>();
            using (var conn = GetDbConnection())
            {
                TableQuery<HistoryClass> dbPerson = null;
                if (mode == 0)
                {
                    dbPerson = conn.Table<HistoryClass>().OrderByDescending(x => x.lookTime).Take(30);
                }
                if (mode == 1)
                {
                    dbPerson = conn.Table<HistoryClass>().OrderByDescending(x => x.lookTime).Take(50);
                }
                if (mode == 2)
                {
                    dbPerson = conn.Table<HistoryClass>().OrderByDescending(x => x.lookTime);
                }
                foreach (HistoryClass item in dbPerson)
                {
                    my.Add(item);
                }
                return my;
            }
        }
        public static HistoryClass GetHistory(string id)
        {
            using (var conn = GetDbConnection())
            {
                //var dbPerson = conn.Table<CommicHistoryClass>();
                TableQuery<HistoryClass> t = conn.Table<HistoryClass>();
                var q = from s in t.AsParallel<HistoryClass>()
                        where s._aid == id
                        select s;
                // 绑定
                if (q.ToList().Count != 0)
                {
                    return q.ToList()[0];
                }
                else
                {
                    return null;
                }
            }
        }
        public static bool AddCommicHistory(HistoryClass mo)
        {
            using (var conn = GetDbConnection())
            {
                // 受影响行数。
                var count = conn.Insert(mo);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool GetComicIsOnHistory(string aid)
        {
            using (var conn = GetDbConnection())
            {
                // 受影响行数。
                var m = from p in conn.Table<HistoryClass>()
                        where p._aid == aid
                        select p;
                if (m.ToList().Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public static bool UpdateComicHistory(HistoryClass mo)
        {
            using (var conn = GetDbConnection())
            {
                var count = conn.Execute("UPDATE HistoryClass SET lookTime=?,title=?,up=?,image=? WHERE _aid=?;", DateTime.Now.ToLocalTime(), mo.title, mo.up, mo.image, mo._aid);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static void ClearHistory()
        {
            using (var conn = GetDbConnection())
            {
                conn.Execute("DELETE FROM HistoryClass");
            }
        }
        #endregion

        #region 观看进度
        public static List<ViewPostHelperClass> GetAllVideoWatchRecord()
        {
            List<ViewPostHelperClass> my = new List<ViewPostHelperClass>();
            using (var conn = GetDbConnection())
            {
                var dbPerson = conn.Table<ViewPostHelperClass>();
                foreach (ViewPostHelperClass item in dbPerson)
                {
                    my.Add(item);
                }
                return my;
            }
        }

        public static ViewPostHelperClass GetVideoWatchRecord(string id)
        {
            using (var conn = GetDbConnection())
            {
                TableQuery<ViewPostHelperClass> t = conn.Table<ViewPostHelperClass>();
                var q = from s in t.AsParallel<ViewPostHelperClass>()
                        where s.epId == id
                        select s;
                if (q.ToList().Count != 0)
                {
                    return q.ToList()[0];
                }
                else
                {
                    return null;
                }
            }
        }
        public static bool AddVideoWatchRecord(ViewPostHelperClass mo)
        {
            using (var conn = GetDbConnection())
            {
                var count = conn.Insert(mo);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool GetVideoHasWatchRecord(string Id)
        {
            using (var conn = GetDbConnection())
            {
                var m = from p in conn.Table<ViewPostHelperClass>()
                        where p.epId == Id
                        select p;
                if (m.ToList().Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public static bool UpdateVideoWatchRecord(ViewPostHelperClass mo)
        {
            using (var conn = GetDbConnection())
            {
                var count = conn.Execute("UPDATE ViewPostHelperClass SET Post=?,viewTime=? WHERE epId=?;", mo.Post, DateTime.Now.ToLocalTime(), mo.epId);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool DeleteVideoWatchRecord(string id)
        {
            using (var conn = GetDbConnection())
            {
                var count = conn.Execute("DELETE FROM ViewPostHelperClass WHERE epId=?", id);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            }
        }

        public static void ClearVideoWatchRecord()
        {
            using (var conn = GetDbConnection())
            {
                conn.Execute("DELETE FROM ViewPostHelperClass");
            }
        }
        #endregion


        public static List<DownloadGuidClass> GetDownloadList(string cid)
        {
            using (var conn = GetDbConnection())
            {
                TableQuery<DownloadGuidClass> dbPerson = conn.Table<DownloadGuidClass>().Where(x => x.cid == cid);
                return dbPerson.ToList();
            }
        }
        public static List<DownloadGuidClass> GetDownloadList()
        {
            using (var conn = GetDbConnection())
            {
                TableQuery<DownloadGuidClass> dbPerson = conn.Table<DownloadGuidClass>();
                return dbPerson.ToList();
            }
        }
        public static DownloadGuidClass GetDownload(string guid)
        {
            using (var conn = GetDbConnection())
            {
                return conn.Table<DownloadGuidClass>().First(x => x.guid == guid);

            }
        }
        public static bool InsertDownload(DownloadGuidClass m)
        {
            using (var conn = GetDbConnection())
            {
                var count = conn.Insert(m);
                if (count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


    }



    public class HistoryClass
    {
        [PrimaryKey]
        public string _aid { get; set; }

        public string title { get; set; }
        public string up { get; set; }
        public string image { get; set; }
        public DateTime lookTime { get; set; }
    }
    public class CommicCollectHelperClass
    {

        [PrimaryKey]
        public string comicId { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string image { get; set; }
        public string image_hor { get; set; }
        public string updateInfo { get; set; }
    }
    public class ViewPostHelperClass
    {
        [PrimaryKey]
        public string epId { get; set; }
        public int Post { get; set; }
        public DateTime viewTime { get; set; }
    }

    public class DownloadGuidClass
    {
        [PrimaryKey]
        public string guid { get; set; }
        public string cid { get; set; }
        public string aid { get; set; }
        public int index { get; set; }
        public string eptitle { get; set; }
        public string title { get; set; }

        public string mode { get; set; }
    }



}
