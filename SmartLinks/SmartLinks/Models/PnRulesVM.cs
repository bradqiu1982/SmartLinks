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
        public string TestCase { set; get; }

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
            TestCase = "";
        }

        public PnRulesVM(string pnkey, string ruleid, string wt, string err, string para, string low, string high, string ruleres,string tc)
        {
            PnKey = pnkey;
            RuleID = ruleid;
            WhichTest = wt;
            ErrAbbr = err;
            Param = para;
            LowLimit = low;
            HighLimit = high;
            RuleRes = ruleres;
            TestCase = tc;
        }

        public static List<PnRulesVM> RetrieveRule(string pnkey)
        {
            var ret = new List<PnRulesVM>();
            var sql = "select PnKey,RuleID,WhichTest,ErrAbbr,Param,LowLimit,HighLimit,RuleRes,TestCase from PnRulesVM where PnKey = @PnKey order by CreateDate DESC";
            var pa = new Dictionary<string, string>();
            pa.Add("@PnKey", pnkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, pa);
            foreach (var line in dbret)
            {
                ret.Add(new PnRulesVM(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToString(line[7]), Convert.ToString(line[8])));
            }
            return ret;
        }

        public static List<PnRulesVM> RetrieveRule(string pnkey,string whichtest,string errabbr)
        {
            var ret = new List<PnRulesVM>();
            var sql = "select PnKey,RuleID,WhichTest,ErrAbbr,Param,LowLimit,HighLimit,RuleRes,TestCase from PnRulesVM "
                + " where PnKey = @PnKey and WhichTest = @WhichTest and ErrAbbr = @ErrAbbr order by CreateDate DESC";

            var pa = new Dictionary<string, string>();
            pa.Add("@PnKey", pnkey);
            pa.Add("@WhichTest", whichtest);
            pa.Add("@ErrAbbr", errabbr);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, pa);

            foreach (var line in dbret)
            {
                ret.Add(new PnRulesVM(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToString(line[2])
                    , Convert.ToString(line[3]), Convert.ToString(line[4]), Convert.ToString(line[5])
                    , Convert.ToString(line[6]), Convert.ToString(line[7]), Convert.ToString(line[8])));
            }
            return ret;
        }

        public static void AddRule(string pnkey, string ruleid, string wt, string err, string para, string low, string high, string ruleres,string tc)
        {
            var sql = "insert into PnRulesVM(PnKey,RuleID,WhichTest,ErrAbbr,Param,LowLimit,HighLimit,RuleRes,CreateDate,TestCase) values(@PnKey,@RuleID,@WhichTest,@ErrAbbr,@Param,@LowLimit,@HighLimit,@RuleRes,@CreateDate,@TestCase)";
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
            pa.Add("@TestCase", tc);
            DBUtility.ExeLocalSqlNoRes(sql,pa);
            PnMESVM.BindMesTab(pnkey, wt);
        }

        public static void EditRule(string pnkey, string ruleid, string wt, string err, string para, string low, string high, string ruleres, string tc)
        {
            var sql = "update PnRulesVM set WhichTest=@WhichTest,ErrAbbr=@ErrAbbr,Param=@Param,LowLimit=@LowLimit,HighLimit=@HighLimit,RuleRes= @RuleRes,TestCase= @TestCase where RuleID = @RuleID";
            var pa = new Dictionary<string, string>();
            pa.Add("@RuleID", ruleid);
            pa.Add("@WhichTest", wt);
            pa.Add("@ErrAbbr", err);
            pa.Add("@Param", para);
            pa.Add("@LowLimit", low);
            pa.Add("@HighLimit", high);
            pa.Add("@RuleRes", ruleres);
            pa.Add("@TestCase", tc);
            DBUtility.ExeLocalSqlNoRes(sql, pa);
            PnMESVM.BindMesTab(pnkey, wt);
        }

        public static List<string> RetrieveAllTestCase()
        {
            var ret = new List<string>();
            var sql = "select distinct TestCase from PnRulesVM where TestCase <> '' order by TestCase";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static void RemoveRule(string ruleid)
        {
            var sql = "select PnKey,WhichTest from PnRulesVM where WhichTest+PnKey in (select WhichTest+PnKey from PnRulesVM where RuleID = @RuleID)";
            var param = new Dictionary<string, string>();
            param.Add("@RuleID", ruleid);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count == 1)
            {
                var pnkey = Convert.ToString(dbret[0][0]);
                var test = Convert.ToString(dbret[0][1]);
                sql = "update PnMESVM set Bind = '' where PNKey = @PNKey and WhichTest = @WhichTest";
                param = new Dictionary<string, string>();
                param.Add("@PNKey", pnkey);
                param.Add("@WhichTest", test);
                DBUtility.ExeLocalSqlNoRes(sql);
            }

            sql = "delete from PnRulesVM where RuleID = @RuleID";
            param = new Dictionary<string, string>();
            param.Add("@RuleID", ruleid);
            DBUtility.ExeLocalSqlNoRes(sql,param);
        }
    }
}