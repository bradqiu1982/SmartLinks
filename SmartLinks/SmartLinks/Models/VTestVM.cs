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
            VID = "";
            TestID = "";
            TestType = "";
            TestContent = "";
            Answer = "";
            TestNotice = "";
            GiftOffer = "";
            GiftPath = "";
            OptionalAnswers = "";
        }

        public VTestVM(string vid,string tid,string tt,string tc,string ans,string tn,string gfer,string gfph,string osw)
        {
            VID = vid;
            TestID = tid;
            TestType = tt;
            TestContent = tc;
            Answer = ans;
            TestNotice = tn;
            GiftOffer = gfer;
            GiftPath = gfph;
            OptionalAnswers = osw;
        }


        public void StoreTestVM()
        {
            var sql = @"insert into VTestVM(VID,TestID,TestType,TestContent,Answer,TestNotice,GiftOffer,GiftPath,OptionalAnswers) 
                        values(@VID,@TestID,@TestType,@TestContent,@Answer,@TestNotice,@GiftOffer,@GiftPath,@OptionalAnswers)";

            var param = new Dictionary<string, string>();
            param.Add("@VID", VID);
            param.Add("@TestID", TestID);
            param.Add("@TestType", TestType);
            param.Add("@TestContent", TestContent);
            param.Add("@Answer", Answer);
            param.Add("@TestNotice", TestNotice);
            param.Add("@GiftOffer", GiftOffer);
            param.Add("@GiftPath", GiftPath);
            param.Add("@OptionalAnswers", OptionalAnswers);
            DBUtility.ExeLocalSqlNoRes(sql,param);
        }

        public static void CleanTest(string vid)
        {
            var sql = "delete from VTestVM where VID = '<VID>'";
            sql = sql.Replace("<VID>", vid);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<VTestVM> RetrieveTest(string vid)
        {
            var ret = new List<VTestVM>();
            var sql = "select VID,TestID,TestType,TestContent,Answer,TestNotice,GiftOffer,GiftPath,OptionalAnswers from VTestVM where VID = '<VID>' order by TestID";
            sql = sql.Replace("<VID>", vid);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var temp = new VTestVM(Convert.ToString(line[0]),Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5]), Convert.ToString(line[6]), Convert.ToString(line[7]),Convert.ToString(line[8]));
                ret.Add(temp);
            }
            return ret;
        }

        public string VID { set; get; }
        public string TestID { set; get; }
        public string TestType { set; get; }
        public string TestContent { set; get; }
        public string Answer { set; get; }
        public string TestNotice { set; get; }
        public string GiftOffer { set; get; }
        public string GiftPath { set; get; }

        public bool IsMultiSelect {
            get {
                if (!string.IsNullOrEmpty(TestType) && TestType.ToUpper().Contains("MULTI"))
                { return true; }
                else { return false; }
            }
        }
        
        public string OptionalAnswers { set; get; }
        public List<string> OpticalAnswerList
        {
            get {
                return (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(OptionalAnswers, (new List<string>()).GetType());
            }
        }

    }
}