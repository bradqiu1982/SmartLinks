using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class VTestScore
    {
        public VTestScore()
        {
            MACHINE =  "";
            UserName =  "";
            VID =  "";
            VSubject =  "";
            CorrectiveAnswer =  "";
            UserAnswer =  "";
            UserScore =  "";
            UserRank =  "";
            UpdateTime =  "";
        }

        public VTestScore(string ma,string name,string vid,string vname,string cans,string uans,string usc,string up,string rank)
        {
            MACHINE = ma;
            UserName = name;
            VID = vid;
            VSubject = vname;
            CorrectiveAnswer = cans;
            UserAnswer = uans;
            UserScore = usc;
            UserRank = rank;
            UpdateTime = up;
        }

        private static List<VTestScore> ScoreExist(string username, string vid)
        {
            var ret = new List<VTestScore>();
            var sql = "select MACHINE from VTestScore where UserName = '<UserName>' and VID = '<VID>'";
            sql = sql.Replace("<UserName>", username).Replace("<VID>",vid);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(new VTestScore());
            }
            return ret;
        }

        public static List<VTestScore> RetrieveSoreWithRank(string username = "", string vid = "")
        {
            var ret = new List<VTestScore>();
            
            var sql = @"select MACHINE,v.UserName,VID,VSubject,CorrectiveAnswer,UserAnswer,UserScore,v.UpdateTime,r.UserRank from VTestScore v
                        left join VTestRank r on r.UserName = v.UserName where 1 = 1";
            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " and v.UserName = '<UserName>'";
                sql = sql.Replace("<UserName>", username);
            }
            if (!string.IsNullOrEmpty(vid))
            {
                sql = sql + " and v.VID = '<VID>'";
                sql = sql.Replace("<VID>", vid);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(new VTestScore(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToDateTime(line[7]).ToString("yyyy-MM-dd HH:mm:ss"), Convert.ToString(line[8])));
            }
            return ret;
        }

        public static void StoreUserScore(string ma, string iname, string vid, string cans, string uans, string usc)
        {
            var uname = iname.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();
            MachineUserMap.TryAddMachineUserMap(ma, uname);

            if (ScoreExist(uname, vid).Count == 0)
            {
                var videos = TechVideoVM.RetrieveVideoByID(vid);
                if (videos.Count > 0)
                {
                    UpdateUserRank(uname, usc);

                    var sql = @"insert into VTestScore(MACHINE,UserName,VID,VSubject,CorrectiveAnswer,UserAnswer,UserScore,UpdateTime)  
                                values(@MACHINE,@UserName,@VID,@VSubject,@CorrectiveAnswer,@UserAnswer,@UserScore,@UpdateTime)";
                    var param = new Dictionary<string, string>();
                    param.Add("@MACHINE",ma);
                    param.Add("@UserName",uname);
                    param.Add("@VID",vid);
                    param.Add("@VSubject",videos[0].VSubject);
                    param.Add("@CorrectiveAnswer",cans);
                    param.Add("@UserAnswer",uans);
                    param.Add("@UserScore",usc);
                    param.Add("@UpdateTime",DateTime.Now.ToString());
                    DBUtility.ExeLocalSqlNoRes(sql,param);
                }
            }
        }

        public static void UpdateUserRank(string UserName, string UserRank)
        {
            var sql = "select UserRank from VTestRank where UserName = '<UserName>'";
            sql = sql.Replace("<UserName>", UserName);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count == 0)
            {
                sql = "insert into VTestRank(UserName,UserRank,UpdateTime) values(@UserName,@UserRank,@UpdateTime)";
                var param = new Dictionary<string, string>();
                param.Add("@UserName", UserName);
                param.Add("@UserRank", UserRank);
                param.Add("@UpdateTime",DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql,param);
            }
            else {
                var rank = Convert.ToInt32(dbret[0][0]);
                var urank = Convert.ToInt32(UserRank);
                var totalrank = rank + urank;
                if (totalrank < 0) totalrank = 0;

                sql = "update VTestRank set UserRank = @UserRank,UpdateTime = @UpdateTime where UserName = @UserName";
                var param = new Dictionary<string, string>();
                param.Add("@UserName", UserName);
                param.Add("@UserRank", totalrank.ToString());
                param.Add("@UpdateTime", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql,param);
            }
        }

        public string MACHINE { set; get; }
        public string UserName { set; get; }
        public string VID { set; get; }
        public string VSubject { set; get; }
        public string CorrectiveAnswer { set; get; }
        public string UserAnswer { set; get; }
        public string UserScore { set; get; }
        public string UserRank { set; get; }
        public string UpdateTime { set; get; }
    }
}