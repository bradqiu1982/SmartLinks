using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class SCRAPRESULT
    {
        public static string ISCRAP = "隔离报废";
        public static string DSCRAP = "直接报废";
        public static string REWORK = "返工";
    }
    
    public class ScrapTableItem
    {
        public ScrapTableItem()
        {
            SN = "";
            DateCode = "";
            PN = "";
            PNKey = "";
            WhichTest = "";
            Result = "";
            TestData = null;
            MatchedRule = "";
        }

        public string SN { set; get; }
        public string DateCode { set; get; }
        public string PN { set; get; }
        public string PNKey { set; get; }
        public string WhichTest { set; get; }
        public string Result { set; get; }
        public SnTestDataVM TestData { set; get; }
        public string MatchedRule { set; get; }

        private List<PnRulesVM> rulelist = new List<PnRulesVM>();
        public List<PnRulesVM> RlueList {
            set {
                rulelist.Clear();
                rulelist.AddRange(value);
            }
            get {
                return rulelist;
            }
        }

    }

    public class ScrapVM
    {
        public static List<ScrapTableItem> RetrievePNBySNDC(List<ScrapTableItem> inputdata)
        {
            var pnpnkeymap = PnMainVM.PNPNKeyMap();

            var hasdatecode = false;
            var hassn = false;
            var ret = new List<ScrapTableItem>();

            var datecond = " ('";
            foreach (var item in inputdata)
            {
                if (!string.IsNullOrEmpty(item.DateCode))
                {
                    datecond = datecond + item.DateCode + "','";
                    hasdatecode = true;
                }
            }
            datecond = datecond.Substring(0, datecond.Length - 2);
            datecond = datecond + ") ";

            var sncond = " ('";
            foreach (var item in inputdata)
            {
                if (!string.IsNullOrEmpty(item.SN))
                {
                    sncond = sncond + item.SN + "','";
                    hassn = true;
                }
            }
            sncond = sncond.Substring(0, sncond.Length - 2);
            sncond = sncond + ") ";

            var sql = "";
            if (hasdatecode && hassn)
            {
                sql = "select c.ContainerName,pb.ProductName,c.DateCode from [InsiteDB].[insite].[Container] c (nolock)"
                + " left join[InsiteDB].[insite].[Product] p(nolock) on p.ProductId = c.ProductId"
                + " left join[InsiteDB].[insite].[ProductBase] pb (nolock) on pb.ProductBaseId = p.ProductBaseId"
                + " where c.ContainerName in <sncond> or c.DateCode in <datecond>";
            }
            else if (hasdatecode)
            {
                sql = "select c.ContainerName,pb.ProductName,c.DateCode from [InsiteDB].[insite].[Container] c (nolock)"
                + " left join[InsiteDB].[insite].[Product] p(nolock) on p.ProductId = c.ProductId"
                + " left join[InsiteDB].[insite].[ProductBase] pb (nolock) on pb.ProductBaseId = p.ProductBaseId"
                + " where c.DateCode in <datecond>";
            }
            else
            {
                sql = "select c.ContainerName,pb.ProductName,c.DateCode from [InsiteDB].[insite].[Container] c (nolock)"
                + " left join[InsiteDB].[insite].[Product] p(nolock) on p.ProductId = c.ProductId"
                + " left join[InsiteDB].[insite].[ProductBase] pb (nolock) on pb.ProductBaseId = p.ProductBaseId"
                + " where c.ContainerName in <sncond>";
            }
            
            sql = sql.Replace("<sncond>", sncond).Replace("<datecond>", datecond);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new ScrapTableItem();
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.PN = Convert.ToString(line[1]);
                if (!string.IsNullOrEmpty(tempvm.SN) && !string.IsNullOrEmpty(tempvm.PN))
                {
                    if (line[2] == null)
                    { tempvm.DateCode = ""; }
                    else
                    { tempvm.DateCode = Convert.ToString(line[2]); }

                    if (pnpnkeymap.ContainsKey(tempvm.PN)){
                        tempvm.PNKey = pnpnkeymap[tempvm.PN];
                    }

                    ret.Add(tempvm);
                }//end if

                
            }
            return ret;
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static List<string> RetriveAllErrAbbr()
        {
            var ret = new List<string>();
            var sql = "select distinct OrignalCode from [NPITrace].[dbo].[ProjectError] order by OrignalCode";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var failurecode = Convert.ToString(line[0]);
                if (!IsDigitsOnly(failurecode.Replace("-", "").Replace("P","")))
                {
                    ret.Add(failurecode);
                }
            }

            return ret;
        }

        public static void MatchRudeRule(List<ScrapTableItem> scraptable)
        {
            var pnmesmap = PnMESVM.RetriveMESKey2TestDict();
            foreach (var item in scraptable)
            {
                if (!string.IsNullOrEmpty(item.TestData.DataID))
                {
                    if (pnmesmap.ContainsKey(item.TestData.MESTab + item.PNKey))
                    {
                        item.WhichTest = pnmesmap[item.TestData.MESTab + item.PNKey];
                        item.RlueList = PnRulesVM.RetrieveRule(item.PNKey, item.WhichTest, item.TestData.ErrAbbr);
                    }
                }
            }//end foreach
        }

        public static void FinalSetResult(List<ScrapTableItem> scraptable)
        {
            var pndefresdict = PnMainVM.PNDefaultResMap();
            foreach (var item in scraptable)
            {
                if (!string.IsNullOrEmpty(item.TestData.DataID)
                    && string.Compare(item.TestData.ErrAbbr.Trim(), "pass", true) != 0)
                {
                    if (item.RlueList.Count == 0)
                    {
                        if (pndefresdict.ContainsKey(item.PN)){
                            item.Result = pndefresdict[item.PN]; }
                    }
                    else
                    {
                        var hasparam = false;
                        foreach (var rule in item.RlueList)
                        {
                            if (!string.IsNullOrEmpty(rule.Param))
                            { hasparam = true; }
                        }

                        if (hasparam)
                        {
                            //ADD TEST CASE INTO RULE FIRST
                            //TO BE DONE
                        }
                        else
                        {
                            item.Result = item.RlueList[0].RuleRes;
                            item.MatchedRule = item.PN + "_" + item.WhichTest + "_" + item.TestData.ErrAbbr;
                        }
                    }
                }//end if
            }//foreach
        }



    }
}