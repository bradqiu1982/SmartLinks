using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

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

        private static bool MatchRuleWithParam(ScrapTableItem data, PnRulesVM rule,Controller ctrl)
        {
            var traceviewfilelist = TraceViewVM.LoadTraceView2Local(data.TestData.TestStation,data.TestData.ModuleSerialNum
                ,data.WhichTest,data.TestData.TestTime.ToString("yyyy-MM-dd HH:mm:ss"),ctrl);

            if (traceviewfilelist.Count == 0)
            {
                return false;
            }

            var traceviewdata = new List<TraceViewData>();
            bool wildmatch = true;
            if (IsDigitsOnly(rule.LowLimit))
            {
                wildmatch = false;
            }

            foreach (var filename in traceviewfilelist)
            {
                var testcase = rule.TestCase;
                if (string.IsNullOrEmpty(rule.TestCase))
                { testcase = "ALL"; }

                var tempret = TraceViewVM.RetrieveTestDataFromTraceView(wildmatch, filename, testcase, rule.Param);
                if (tempret.Count > 0)
                {
                    traceviewdata.AddRange(tempret);
                }
            }

            if (traceviewdata.Count == 0)
            {
                return false;
            }

            if (wildmatch)
            {
                if (rule.LowLimit.Contains("##"))
                {
                    try
                    {
                        var lowhigh = rule.LowLimit.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
                        var low = 0.0;
                        var high = 0.0;
                        if (lowhigh[0].ToUpper().Contains("0X"))
                        {
                            var parsed = long.Parse(lowhigh[0].ToUpper().Replace("0X", ""), NumberStyles.AllowHexSpecifier);
                            low = Convert.ToDouble(parsed);
                        }
                        else
                        {
                            low = Convert.ToDouble(lowhigh[0]);
                        }
                        if (lowhigh[1].ToUpper().Contains("0X"))
                        {
                            var parsed = long.Parse(lowhigh[1].ToUpper().Replace("0X", ""), NumberStyles.AllowHexSpecifier);
                            high = Convert.ToDouble(parsed);
                        }
                        else
                        {
                            high = Convert.ToDouble(lowhigh[1]);
                        }

                        foreach (var tdata in traceviewdata)
                        {
                            if (tdata.dValue >= low && tdata.dValue <= high)
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception E) { }
                }
                else
                {
                    foreach (var tdata in traceviewdata)
                    {
                        if (string.Compare(tdata.Value.ToUpper().Trim(), rule.LowLimit.ToUpper().Trim()) != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                foreach (var tdata in traceviewdata)
                {
                    var low = Convert.ToDouble(rule.LowLimit);
                    var high = Convert.ToDouble(rule.HighLimit);
                    if (tdata.dValue >= low && tdata.dValue <= high)
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        public static void FinalSetResult(List<ScrapTableItem> scraptable, Controller ctrl)
        {
            var pndefresdict = PnMainVM.PNDefaultResMap();
            foreach (var item in scraptable)
            {
                if (!string.IsNullOrEmpty(item.TestData.DataID)
                    && string.Compare(item.TestData.ErrAbbr.Trim(), "pass", true) != 0)
                {
                    if (item.RlueList.Count == 0)
                    {
                        if (pndefresdict.ContainsKey(item.PN))
                        {
                            item.Result = pndefresdict[item.PN];
                            item.MatchedRule = "DEFAULT";
                        }
                    }
                    else
                    {
                        foreach (var rule in item.RlueList)
                        {
                            if (!string.IsNullOrEmpty(rule.Param))
                            {
                                if (MatchRuleWithParam(item,rule,ctrl))
                                {
                                    item.Result = rule.RuleRes;
                                    item.MatchedRule = item.PN + "_" + item.WhichTest + "_" + item.TestData.ErrAbbr + "_" + rule.Param;
                                    break;
                                }
                            }
                            else
                            {
                                item.Result = rule.RuleRes;
                                item.MatchedRule = item.PN + "_" + item.WhichTest + "_" + item.TestData.ErrAbbr;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(item.Result))
                        {
                            item.Result = pndefresdict[item.PN];
                            item.MatchedRule = "DEFAULT";
                        }
                    }
                }//end if
            }//foreach
        }


    }

}