using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class DMRSNTestData
    {
        public static void UpdataProductLineTestData(string DMRProdLine, Dictionary<string, DateTime> snstoredatedict)
        {
            var snlist = snstoredatedict.Keys.ToList();
            var sncond = "('" + string.Join("','", snlist) + "')";
            var sql = @"select distinct ModuleSerialNum,WhichTest,ErrAbbr,TestTimeStamp FROM [NPITrace].[dbo].[ProjectTestData] where ModuleSerialNum in <sncond> order by ModuleSerialNum,TestTimeStamp desc";
            sql = sql.Replace("<sncond>", sncond);
            var sntestdict = new Dictionary<string, bool>();
            var ret = new List<DMRSNTestData>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new DMRSNTestData();
                tempvm.SN = O2S(line[0]).ToUpper().Trim();
                tempvm.WhichTest = O2S(line[1]);
                tempvm.Failure = O2S(line[2]);
                tempvm.TestTime = O2T(line[3]);

                var sntestkey = tempvm.SN + ":" + tempvm.WhichTest;
                if (sntestdict.ContainsKey(sntestkey))
                { continue; }
                else
                { sntestdict.Add(sntestkey, true); }

                if (!string.IsNullOrEmpty(tempvm.TestTime)
                    && DateTime.Parse(tempvm.TestTime) > snstoredatedict[tempvm.SN])
                {
                    ret.Add(tempvm);
                }
            }

            if (ret.Count > 0)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("@DMRProdLine", DMRProdLine);
                sql = "delete from DMRSNTestData where DMRProdLine = @DMRProdLine";
                DBUtility.ExeLocalSqlNoRes(sql, dict);

                sql = "insert into DMRSNTestData(DMRProdLine,SN,WhichTest,Failure,TestTime) values(@DMRProdLine,@SN,@WhichTest,@Failure,@TestTime)";
                foreach (var item in ret)
                {
                    dict = new Dictionary<string, string>();
                    dict.Add("@DMRProdLine", DMRProdLine);
                    dict.Add("@SN", item.SN);
                    dict.Add("@WhichTest", item.WhichTest);
                    dict.Add("@Failure", item.Failure);
                    dict.Add("@TestTime", item.TestTime);
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }

        }

        public static List<object> RetrieveTestData(string prodline,Dictionary<string, bool> sndict)
        {
            var r = new List<object>();

            var dict = new Dictionary<string, string>();
            dict.Add("@DMRProdLine", prodline);

            var snlist = sndict.Keys.ToList();
            var sncond = "('" + string.Join("','", snlist) + "')";
            var sql = "select WhichTest,Failure,SN,TestTime from DMRSNTestData where DMRProdLine = @DMRProdLine and SN in <sncond> order by SN,TestTime desc";
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);

            var whichcntdict = new Dictionary<string, DMRSNTestData>();
            var faillist = new List<DMRSNTestData>();

            foreach (var l in dbret)
            {
                var ts = O2S(l[0]);
                var fail = O2S(l[1]);
                var sn = O2S(l[2]);
                var tt = O2T(l[3]);

                var pass = true;
                if (string.Compare(fail, "pass", true) != 0)
                {
                    pass = false;

                    var tempvm = new DMRSNTestData();
                    tempvm.SN = sn;
                    tempvm.WhichTest = ts;
                    tempvm.Failure = fail;
                    tempvm.TestTime = tt;
                    faillist.Add(tempvm);
                }

                if (whichcntdict.ContainsKey(ts))
                {
                    if (pass)
                    { whichcntdict[ts].PassCnt += 1; }
                    else
                    { whichcntdict[ts].FailCnt += 1; }
                }
                else
                {
                    var tempvm = new DMRSNTestData();
                    tempvm.WhichTest = ts;
                    if (pass)
                    { tempvm.PassCnt += 1; }
                    else
                    { tempvm.FailCnt += 1; }
                    whichcntdict.Add(ts, tempvm);
                }
            }

            r.Add(whichcntdict.Values.ToList());
            r.Add(faillist);

            return r;
        }

        private static string O2S(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        private static string O2T(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public DMRSNTestData()
        {
            DMRProdLine = "";
            SN = "";
            WhichTest = "";
            Failure = "";
            TestTime = "";
            PassCnt = 0.0;
            FailCnt = 0.0;
        }

        public string DMRProdLine { set; get; }
        public string SN { set; get; }
        public string WhichTest { set; get; }
        public string Failure { set; get; }
        public string TestTime { set; get; }

        public double PassCnt { set; get; }

        public double FailCnt { set; get; }

        public double Yield { get {
                if ((PassCnt + FailCnt) == 0.0)
                { return 0.0; }

                return Math.Round(PassCnt / (PassCnt + FailCnt) * 100.0, 1);
            } }

        public double TotalCnt { get { return PassCnt + FailCnt; } }
    }
}