using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class PnRulesVM
    {
        public string PnKey { set; get; }
        public string RuleID { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string Param { set; get; }
        public string LowLimit { set; get; }
        public string HighLimit { set; get; }
        public string RuleRes { set; get; }

        public PnRulesVM()
        {
            PnKey = "";
            RuleID = "";
            WhichTest = "";
            ErrAbbr = "";
            Param = "";
            LowLimit = "";
            HighLimit = "";
            RuleRes = "";
        }

        public PnRulesVM(string pnkey, string ruleid, string wt, string err, string para, string low, string high, string ruleres)
        {
            PnKey = pnkey;
            RuleID = ruleid;
            WhichTest = wt;
            ErrAbbr = err;
            Param = para;
            LowLimit = low;
            HighLimit = high;
            RuleRes = ruleres;
        }

        public static List<PnRulesVM> RetrieveRule(string pnkey)
        {
            var ret = new List<PnRulesVM>();
            var sql = "select PnKey,RuleID,WhichTest,ErrAbbr,Param,LowLimit,HighLimit,RuleRes from PnRulesVM where PnKey = @PnKey order by CreateDate DESC";
            var pa = new Dictionary<string, string>();
            pa.Add("@PnKey", pnkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, pa);
            foreach (var line in dbret)
            {
                ret.Add(new PnRulesVM(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToString(line[7])));
            }
            return ret;
        }

        public static List<PnRulesVM> RetrieveRule(string pnkey,string whichtest,string errabbr)
        {
            var ret = new List<PnRulesVM>();
            var sql = "select PnKey,RuleID,WhichTest,ErrAbbr,Param,LowLimit,HighLimit,RuleRes from PnRulesVM "
                +" where PnKey = @PnKey and WhichTest = @WhichTest and ErrAbbr = @ErrAbbr order by CreateDate DESC";

            var pa = new Dictionary<string, string>();
            pa.Add("@PnKey", pnkey);
            pa.Add("@WhichTest", whichtest);
            pa.Add("@ErrAbbr", errabbr);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, pa);

            foreach (var line in dbret)
            {
                ret.Add(new PnRulesVM(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToString(line[7])));
            }
            return ret;
        }

        public static void AddRule(string pnkey, string ruleid, string wt, string err, string para, string low, string high, string ruleres)
        {
            var sql = "insert into PnRulesVM(PnKey,RuleID,WhichTest,ErrAbbr,Param,LowLimit,HighLimit,RuleRes,CreateDate) values(@PnKey,@RuleID,@WhichTest,@ErrAbbr,@Param,@LowLimit,@HighLimit,@RuleRes,@CreateDate)";
            var pa = new Dictionary<string, string>();
            pa.Add("@PnKey",pnkey);
            pa.Add("@RuleID",ruleid);
            pa.Add("@WhichTest",wt);
            pa.Add("@ErrAbbr",err);
            pa.Add("@Param",para);
            pa.Add("@LowLimit",low);
            pa.Add("@HighLimit",high);
            pa.Add("@RuleRes", ruleres);
            pa.Add("@CreateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql,pa);
            PnMESVM.BindMesTab(pnkey, wt);
        }

        public static void EditRule(string pnkey, string ruleid, string wt, string err, string para, string low, string high, string ruleres)
        {
            var sql = "update PnRulesVM set WhichTest=@WhichTest,ErrAbbr=@ErrAbbr,Param=@Param,LowLimit=@LowLimit,HighLimit=@HighLimit,RuleRes= @RuleRes where RuleID = @RuleID";
            var pa = new Dictionary<string, string>();
            pa.Add("@RuleID", ruleid);
            pa.Add("@WhichTest", wt);
            pa.Add("@ErrAbbr", err);
            pa.Add("@Param", para);
            pa.Add("@LowLimit", low);
            pa.Add("@HighLimit", high);
            pa.Add("@RuleRes", ruleres);
            DBUtility.ExeLocalSqlNoRes(sql, pa);
            PnMESVM.BindMesTab(pnkey, wt);
        }

        public static void RemoveRule(string ruleid)
        {
            var sql = "delete from PnRulesVM where RuleID = @RuleID";
            var param = new Dictionary<string, string>();
            param.Add("@RuleID", ruleid);
            DBUtility.ExeLocalSqlNoRes(sql,param);
        }
    }
}