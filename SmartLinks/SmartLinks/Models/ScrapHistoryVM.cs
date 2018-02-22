using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class ScrapHistoryVM
    {
        public ScrapHistoryVM()
        {
            SN = "";
            DateCode = "";
            PN = "";
            WhichTest = "";
            ErrAbbr = "";
            MatchRule = "";
            Result = "";
            CreateDate = DateTime.Parse("1982-05-06 10:00:00");
        }

        public ScrapHistoryVM(string sn,string dc,string pn,string wt,string err,string rule,string res,DateTime crt)
        {
            SN = sn;
            DateCode = dc;
            PN = pn;
            WhichTest = wt;
            ErrAbbr = err;
            MatchRule = rule;
            Result = res;
            CreateDate = crt;
        }

        public string SN { set; get; }
        public string DateCode { set; get; }
        public string PN { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string MatchRule { set; get; }
        public string Result { set; get; }
        public DateTime CreateDate { set; get; }

        public static void AddHistory(ScrapTableItem item)
        {
            var sql = "delete from ScrapHistoryVM where SN = @SN";
            var param =new  Dictionary<string, string>();
            param.Add("@SN", item.SN);
            DBUtility.ExeLocalSqlNoRes(sql, param);

            sql = "insert into ScrapHistoryVM(SN,DateCode,PN,WhichTest,ErrAbbr,MatchRule,Result,CreateDate) values(@SN,@DateCode,@PN,@WhichTest,@ErrAbbr,@MatchRule,@Result,@CreateDate)";
            param = new Dictionary<string, string>();
            param.Add("@SN", item.SN);
            param.Add("@DateCode", item.DateCode);
            param.Add("@PN", item.PN);
            param.Add("@WhichTest", item.WhichTest);
            param.Add("@ErrAbbr", item.TestData.ErrAbbr);
            param.Add("@MatchRule", item.MatchedRule);
            param.Add("@Result", item.Result);
            param.Add("@CreateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<ScrapHistoryVM> RetrieveHistory()
        {
            var ret = new List<ScrapHistoryVM>();
            var sql = "select SN,DateCode,PN,WhichTest,ErrAbbr,MatchRule,Result,CreateDate from ScrapHistoryVM order by CreateDate desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(new ScrapHistoryVM(Convert.ToString(line[0]),Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToDateTime(line[7])));
            }
            return ret;
        }


    }
}