using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class PnMainVM
    {

        public string PNKey { set; get; }
        public string PN { set; get; }
        public string PNPJ { set; get; }
        public string DefaultResult { set; get; }

        private List<PnRulesVM> pnrulelist= new List<PnRulesVM>();
        public List<PnRulesVM> PnRuleList {
            set {
                pnrulelist.Clear();
                pnrulelist.AddRange(value);
            }
            get { return pnrulelist; }
        }

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static void StoreNewPN(string pn,string pnpj,string defres)
        {
            var sql = "insert into PnMainVM(PNKey,PN,PNPJ,DefaultResult) values(@PNKey,@PN,@PNPJ,@DefaultResult)";
            var param = new Dictionary<string, string>();
            param.Add("@PNKey", GetUniqKey());
            param.Add("@PN", pn);
            param.Add("@PNPJ", pnpj);
            param.Add("@DefaultResult", defres);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void RemovePN(string pnkey)
        {
            var param = new Dictionary<string, string>();
            param.Add("@PNKey", pnkey);

            var sql = "delete from PnMainVM where PNKey = @PNKey";
            DBUtility.ExeLocalSqlNoRes(sql, param);

            sql = "delete from PnMESVM where PNKey = @PNKey";
            DBUtility.ExeLocalSqlNoRes(sql, param);

            sql = "delete from PnRulesVM where PnKey = @PNKey";
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static List<PnMainVM> RetrievePNList(string searchkey)
        {
            var ret = new List<PnMainVM>();

            var sql = "";
            if (!string.IsNullOrEmpty(searchkey))
            {
                sql = "select PNKey,PN,PNPJ,DefaultResult from PnMainVM where PN like '%<searchkey>%' order by PNPJ";
                sql = sql.Replace("<searchkey>", searchkey);
            }
            else
            {
                sql = "select PNKey,PN,PNPJ,DefaultResult from PnMainVM order by PNPJ";
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new PnMainVM();
                tempvm.PNKey = Convert.ToString(line[0]);
                tempvm.PN = Convert.ToString(line[1]);
                tempvm.PNPJ = Convert.ToString(line[2]);
                tempvm.DefaultResult = Convert.ToString(line[3]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<PnMainVM> RetrieveAllPnSetting(string pn)
        {
            var ret = new List<PnMainVM>();

            var sql  = "select PNKey,PN,PNPJ,DefaultResult from PnMainVM where PN = '<PN>' order by PNPJ";
            sql = sql.Replace("<PN>", pn);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new PnMainVM();
                tempvm.PNKey = Convert.ToString(line[0]);
                tempvm.PN = Convert.ToString(line[1]);
                tempvm.PNPJ = Convert.ToString(line[2]);
                tempvm.DefaultResult = Convert.ToString(line[3]);
                tempvm.PnRuleList = PnRulesVM.RetrieveRule(tempvm.PNKey);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static void UpdatePNPJ(string pnkey,string pn,string pnpj,string defres)
        {
            var sql = "update PnMainVM set PN=@PN,PNPJ=@PNPJ,DefaultResult=@DefaultResult where PNKey = @PNKey";
            var param = new Dictionary<string, string>();
            param.Add("@PNKey", pnkey);
            param.Add("@PN", pn);
            param.Add("@PNPJ", pnpj);
            param.Add("@DefaultResult", defres);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string,bool> RetrieveKeyByPNDict(Dictionary<string, bool> pndict)
        {
            var pncond = " ('";
            foreach (var item in pndict)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    pncond = pncond + item.Key + "','";
                }
            }
            pncond = pncond.Substring(0, pncond.Length - 2);
            pncond = pncond + ") ";

            var sql = "select PNKey from PnMainVM where PN in <PNCOND>";
            sql = sql.Replace("<PNCOND>", pncond);

            var pnkeydict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pnkey = Convert.ToString(line[0]);
                if (!pnkeydict.ContainsKey(pnkey))
                {
                    pnkeydict.Add(pnkey, true);
                }
            }
            return pnkeydict;
        }

        public static Dictionary<string, string> PNPNKeyMap()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select PNKey,PN from PnMainVM";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var key = Convert.ToString(line[0]);
                var pn = Convert.ToString(line[1]);
                if (!ret.ContainsKey(pn))
                {
                    ret.Add(pn, key);
                }
            }

            return ret;
        }

        public static Dictionary<string, string> PNDefaultResMap()
        {
            var ret = new Dictionary<string, string>();
            var sql = "select PN,DefaultResult from PnMainVM";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pn = Convert.ToString(line[0]);
                var def = Convert.ToString(line[1]);
                if (!ret.ContainsKey(pn))
                {
                    ret.Add(pn, def);
                }
            }
            return ret;
        }

    }
}