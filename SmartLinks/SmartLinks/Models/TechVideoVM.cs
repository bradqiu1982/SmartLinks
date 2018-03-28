using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class TechVideoVM
    {
        public TechVideoVM()
        {
            VID = "";
            VSubject = "";
            VDescription = "";
            VPath = "";
            UpdateTime = "";
            Updater = "";
            testlist.Clear();
        }

        public TechVideoVM(string vid,string sub, string des, string path, string time,string uper)
        {
            VID = vid;
            VSubject = sub;
            VDescription = des;
            VPath = path;
            UpdateTime = time;
            Updater = uper;
            testlist.Clear();
        }

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static void StoreVideo(string sub,string des,string path,string uper)
        {
            var sql = "insert into TechVideoVM(VID,VSubject,VDescription,VPath,UpdateTime,Updater) values(@VID,@VSubject,@VDescription,@VPath,@UpdateTime,@Updater)";
            var param = new Dictionary<string, string>();
            param.Add("@VID", GetUniqKey());
            param.Add("@VSubject",sub);
            param.Add("@VDescription", des);
            param.Add("@VPath", path);
            param.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@Updater", uper);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<TechVideoVM> RetrieveVideo(string searchkey)
        {
            var ret = new List<TechVideoVM>();

            var sql = "";
            if (string.IsNullOrEmpty(searchkey))
            {
                sql = @"select VID,VSubject,VDescription,VPath,UpdateTime,Updater,tmp_log.ViewCnt
                        from TechVideoVM as tv 
                        left join (
                            select VideoID, count(VideoID) as ViewCnt from VideoLog group by VideoID
                        ) as tmp_log on tmp_log.VideoID = tv.VID
                        order by tmp_log.ViewCnt desc, UpdateTime desc";
            }
            else
            {
                sql = @"select VID,VSubject,VDescription,VPath,UpdateTime,Updater,tmp_log.ViewCnt
                        from TechVideoVM as tv 
                        left join (
                            select VideoID, count(VideoID) as ViewCnt from VideoLog group by VideoID
                        ) as tmp_log on tmp_log.VideoID = tv.VID where VSubject like '%<searchkey>%' or VDescription like '%<searchkey>%' 
                        order by tmp_log.ViewCnt desc, UpdateTime desc";
                sql = sql.Replace("<searchkey>", searchkey);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            foreach (var line in dbret)
            {
                ret.Add(new TechVideoVM(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2]), Convert.ToString(line[3]), Convert.ToDateTime(line[4]).ToString("yyyy-MM-dd"), Convert.ToString(line[5])));
            }
            return ret;
        }

        public string VID { set; get; }
        public string VSubject { set; get; }
        public string VDescription { set; get; }
        public string VPath { set; get; }
        public string UpdateTime { set; get; }
        public string Updater { set; get; }
        public string IPath { set; get; }

        private List<VTestVM> testlist = new List<VTestVM>();
        public List<VTestVM> TestList {
            set { testlist.Clear();testlist.AddRange(value); }
            get { return testlist; }
        }

    }

    public class UserAnswer {
        public string q_id { set; get; }
        public string q_type { set; get; }
        public string answer { set; get; }
    }

}