using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class VTestVM
    {
        public VTestVM()
        {
            OptionalAnswerCount = 5;
            VID = "";
            TestID = "";
            TestContent = "";
            Answer = "";
            TestNotice = "";
            GiftOffer = "";
            GiftPath = "";
            optionalanswerlist.Clear();
        }

        public VTestVM(string vid,string tid,string tc,string ans,string tn,string gfer,string gfph)
        {
            OptionalAnswerCount = 5;
            VID = vid;
            TestID = tid;
            TestContent = tc;
            Answer = ans;
            TestNotice = tn;
            GiftOffer = gfer;
            GiftPath = gfph;
            optionalanswerlist.Clear();
        }

        public void AddOptionalAnswer(string ans)
        {
            if (AnswerList.Count < OptionalAnswerCount)
            {
                AnswerList.Add(ans);
            }
        }

        private void CompleteAnswer()
        {
            var ascount = AnswerList.Count;
            for (var idx = ascount; idx < OptionalAnswerCount; idx++)
            {
                AnswerList.Add("");
            }
        }

        public void StoreTestVM()
        {
            CompleteAnswer();
            var sql = "delete from VTestVM where VID = '<VID>'";
            sql = sql.Replace("<VID>", VID);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = @"insert into VTestVM(VID,TestID,TestContent,Answer,TestNotice,GiftOffer,GiftPath,OptionalAnswer0,OptionalAnswer1,OptionalAnswer2,OptionalAnswer3,OptionalAnswer4) 
                        values(@VID,@TestID,@TestContent,@Answer,@TestNotice,@GiftOffer,@GiftPath,@OptionalAnswer0,@OptionalAnswer1,@OptionalAnswer2,@OptionalAnswer3,@OptionalAnswer4)";

            var param = new Dictionary<string, string>();
            param.Add("@VID", VID);
            param.Add("@TestID", TestID);
            param.Add("@TestContent", TestContent);
            param.Add("@Answer", Answer);
            param.Add("@TestNotice", TestNotice);
            param.Add("@GiftOffer", GiftOffer);
            param.Add("@GiftPath", GiftPath);
            for(var idx = 0;idx < OptionalAnswerCount;idx++)
            {
                param.Add("@OptionalAnswer" + idx, AnswerList[idx]);
            }
            DBUtility.ExeLocalSqlNoRes(sql,param);
        }

        public static List<VTestVM> RetrieveTest(string vid)
        {
            var ret = new List<VTestVM>();
            var sql = "select VID,TestID,TestContent,Answer,TestNotice,GiftOffer,GiftPath,OptionalAnswer0,OptionalAnswer1,OptionalAnswer2,OptionalAnswer3,OptionalAnswer4 from VTestVM where VID = '<VID>'";
            sql = sql.Replace("<VID>", vid);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var temp = new VTestVM(Convert.ToString(line[0]),Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5]), Convert.ToString(line[6]));
                temp.AddOptionalAnswer(Convert.ToString(line[7]));
                temp.AddOptionalAnswer(Convert.ToString(line[8]));
                temp.AddOptionalAnswer(Convert.ToString(line[9]));
                temp.AddOptionalAnswer(Convert.ToString(line[10]));
                temp.AddOptionalAnswer(Convert.ToString(line[11]));
            }
            return ret;
        }

        public int OptionalAnswerCount { set; get; }

        public string VID { set; get; }
        public string TestID { set; get; }
        public string TestContent { set; get; }
        public string Answer { set; get; }
        public string TestNotice { set; get; }
        public string GiftOffer { set; get; }
        public string GiftPath { set; get; }
        
        private List<string> optionalanswerlist = new List<string>();
        public List<string> AnswerList {
            set { optionalanswerlist.Clear();optionalanswerlist.AddRange(value); }
            get { return optionalanswerlist; }
        }
        
    }
}