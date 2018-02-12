using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class PnMESVM
    {
        public PnMESVM()
        {
            PNKey = "";
            WhichTest = "";
            MESTab = "";
            Bind = "";
        }
        public PnMESVM(string pk,string w,string m,string bd)
        {
            PNKey = pk;
            WhichTest = w;
            MESTab = m;
            Bind = bd;
        }

        public string PNKey { set; get; }
        public string WhichTest { set; get; }
        public string MESTab { set; get; }
        public string Bind { set; get; }

        public static List<string> RetrievePnMESTabByPNDict(Dictionary<string, bool> pnkeydict)
        {
            var pncond = " ('";
            foreach (var item in pnkeydict)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    pncond = pncond + item.Key + "','";
                  }
            }
            pncond = pncond.Substring(0, pncond.Length - 2);
            pncond = pncond + ") ";

            var mesdict = new Dictionary<string, bool>();
            var sql = "select MESTab from PnMESVM where PNKey in <PNKeyCond> and Bind = 'TRUE'";
            sql = sql.Replace("<PNKeyCond>", pncond);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var mestab = Convert.ToString(line[0]);
                if (!mesdict.ContainsKey(mestab))
                {
                    mesdict.Add(mestab, true);
                }
            }

            var ret = new List<string>();
            ret.AddRange(mesdict.Keys);
            return ret;
        }

        public static List<string> RetrievePnWhichTestByPNKey(string pnkey)
        {
            var ret = new List<string>();
            var sql = "select WhichTest from PnMESVM where PNKey = '<PNKey>'";
            sql = sql.Replace("<PNKey>", pnkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        private static List<PnMESVM> RetrievePnMESTab(string pnkey,string whichtest)
        {
            var ret = new List<PnMESVM>();

            var sql = "select PNKey,WhichTest,MESTab,Bind from PnMESVM where PNKey = @PNKey and WhichTest = @WhichTest";
            var param = new Dictionary<string, string>();
            param.Add("@PNKey", pnkey);
            param.Add("@WhichTest", whichtest);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,param);
            foreach (var line in dbret)
            {
                var tempvm = new PnMESVM(Convert.ToString(line[0]), Convert.ToString(line[1])
                    , Convert.ToString(line[2]), Convert.ToString(line[3]));
                ret.Add(tempvm);
            }
            return ret;
        }

        public static void StoreMesTab(string pnkey,string whichtest, string mestab)
        {
            var existtab = RetrievePnMESTab(pnkey, whichtest);
            if (existtab.Count > 0)
            {
                var sql = "update PnMESVM set MESTab = @MESTab where PNKey = @PNKey and WhichTest = @WhichTest";
                var param = new Dictionary<string, string>();
                param.Add("@PNKey", pnkey);
                param.Add("@WhichTest", whichtest);
                param.Add("@MESTab", mestab);
                DBUtility.ExeLocalSqlNoRes(sql, param);
            }
            else
            {
                var sql = "insert into PnMESVM(PNKey,WhichTest,MESTab,Bind) values(@PNKey,@WhichTest,@MESTab,'FALSE')";
                var param = new Dictionary<string, string>();
                param.Add("@PNKey", pnkey);
                param.Add("@WhichTest", whichtest);
                param.Add("@MESTab", mestab);
                DBUtility.ExeLocalSqlNoRes(sql, param);
            }
        }

        public static void BindMesTab(string PNKey, string whichtest)
        {
            var sql = "update PnMESVM set Bind = 'TRUE'  where PNKey = @PNKey and WhichTest = @WhichTest";
            var param = new Dictionary<string, string>();
            param.Add("@PNKey", PNKey);
            param.Add("@WhichTest", whichtest);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string, string> RetriveMESKey2TestDict()
        {
            var ret = new Dictionary<string, string>();

            var sql = "select PNKey,WhichTest,MESTab from PnMESVM";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var key = Convert.ToString(line[0]);
                var wt = Convert.ToString(line[1]);
                var mes = Convert.ToString(line[2]);
                if (!ret.ContainsKey(mes + key))
                {
                    ret.Add(mes + key, wt);
                }
            }
            return ret;
        }


    }
}