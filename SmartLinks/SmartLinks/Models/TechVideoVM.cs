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
        }

        public TechVideoVM(string vid,string sub, string des, string path, string time,string uper)
        {
            VID = vid;
            VSubject = sub;
            VDescription = des;
            VPath = path;
            UpdateTime = time;
            Updater = uper;
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
                sql = "select VID,VSubject,VDescription,VPath,UpdateTime,Updater from TechVideoVM order by UpdateTime desc";
            }
            else
            {
                sql = "select VID,VSubject,VDescription,VPath,UpdateTime,Updater from TechVideoVM where VSubject like '%<searchkey>%' or VDescription like '%<searchkey>%' order by UpdateTime desc";
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
    }
}