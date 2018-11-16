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
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var processlist = syscfg["SCRAP4PROCESSNAME"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                                if (MatchRuleWithParam(item, rule, ctrl))
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

                {
                    var testdatalist = RetrieveLatestSNStep(item.SN);
                    if (testdatalist.Count > 0)
                    {

                        var matchprocess = false;
                        foreach (var p in processlist)
                        {
                            if (testdatalist[0].WhichTest.ToUpper().Contains(p.ToUpper()))
                            {
                                item.TestData.DataID = testdatalist[0].DataID;
                                item.WhichTest = testdatalist[0].WhichTest;
                                item.TestData.ErrAbbr = testdatalist[0].ErrAbbr;
                                item.TestData.TestTime = testdatalist[0].TestTime;
                                item.TestData.ErrAbbr = "PROCESS";
                                matchprocess = true;
                                break;
                            }
                        }//end foreach

                        if (matchprocess)
                        {
                            item.Result = SCRAPRESULT.DSCRAP;
                            item.MatchedRule = "PROCESS SCRAP";
                        }
                        //else
                        //{
                        //    if (pndefresdict.ContainsKey(item.PN))
                        //    {
                        //        item.Result = pndefresdict[item.PN];
                        //        item.MatchedRule = "DEFAULT";
                        //    }
                        //}
                    }
                    //else
                    //{
                    //    if (pndefresdict.ContainsKey(item.PN))
                    //    {
                    //        item.Result = pndefresdict[item.PN];
                    //        item.MatchedRule = "DEFAULT";
                    //    }
                    //}
                }

            }//foreach
        }

        private static string Convert2Str(object obj)
        {
            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        public static List<string> RetrieveDCTableFromSn(string sn)
        {
            var ret = new List<string>();
            var dctabledict = new Dictionary<string, bool>();

            var sql = @" select ddr.DataCollectionDefName from insitedb.insite.DataCollectionDefBase ddr  (nolock)
	                    inner join insitedb.insite.TxnMap tm with(noloCK) ON tm.DataCollectionDefinitionBaseId = ddr.DataCollectionDefBaseId
	                    inner join insitedb.insite.spec sp with(nolock) on sp.specid =  tm.specid
	                    inner join InsiteDB.insite.WorkflowStep ws (nolock)on  ws.specbaseid = sp.specbaseid
	                    inner join InsiteDB.insite.Workflow w (nolock)on w.WorkflowID = ws.WorkflowID
                        inner join InsiteDB.insite.Product p(nolock) on w.WorkflowBaseId = p.WorkflowBaseId
	                    inner join [InsiteDB].[insite].[Container] c(nolock) on c.ProductId = p.ProductId
                        where c.ContainerName = '<ContainerName>' and ddr.DataCollectionDefName is not null";
            sql = sql.Replace("<ContainerName>", sn);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var dc = Convert2Str(line[0]).ToUpper();
                if (dc.Length > 4 && dc.Substring(0, 4).Contains("DCD_"))
                {
                    var realdc = "";
                    if (dc.Contains("DCD_Module_Initialization_0811".ToUpper()))
                    { realdc = "dc_initial"; }
                    else
                    { realdc = "dc_" + dc.Substring(4); }

                    if (!dctabledict.ContainsKey(realdc))
                    {
                        dctabledict.Add(realdc, true);
                    }
                }//end if
            }//end foreach
            ret.AddRange(dctabledict.Keys);
            return ret;
        }

        public static List<SnTestDataVM> RetrieveLatestSNTestResult(string sn)
        {
            var dctablelist = RetrieveDCTableFromSn(sn);
            var testdatalist = new List<SnTestDataVM>();

            foreach (var dctable in dctablelist)
            {
                if (dctable.Contains("OQCPARALLEL")
                    || dctable.Contains("AOC_MANUALINSPECTION"))
                { continue; }

                var sql = @"select top 1 a.<DCTABLE>HistoryId,a.ModuleSerialNum, a.WhichTest, a.ModuleType, a.ErrAbbr, a.TestTimeStamp, a.TestStation,a.assemblypartnum 
                               from insite.<DCTABLE> a (nolock) where a.ModuleSerialNum = '<ModuleSerialNum>' order by  testtimestamp DESC";
                sql = sql.Replace("<DCTABLE>", dctable).Replace("<ModuleSerialNum>", sn);
                var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var item in dbret)
                {
                    var tempdata = new SnTestDataVM(Convert.ToString(item[0]), Convert.ToString(item[1]), Convert.ToString(item[7]), Convert.ToString(item[2])
                        , Convert.ToString(item[4]),dctable,Convert.ToDateTime(item[5]),Convert.ToString(item[6]));

                    testdatalist.Add(tempdata);
                }
            }

            testdatalist.Sort(delegate (SnTestDataVM obj1, SnTestDataVM obj2)
            {
                return obj2.TestTime.CompareTo(obj1.TestTime);
            });

            var ret = new List<SnTestDataVM>();
            ret.Add(testdatalist[0]);
            return ret;
        }

        public static List<SnTestDataVM> RetrieveLatestSNStep(string sn)
        {
            var testdatalist = new List<SnTestDataVM>();

            var sql = @"select top 1 ProductId,MoveOutTime,WorkflowStepName,Comments,TxnTypeName
                        from PDMSMaster.dbo.HistStepMoveSummaryOld (nolock) where ContainerName = '<ContainerName>'  and MFGOrderId is not null order by MoveOutTime desc";
            sql = sql.Replace("<ContainerName>", sn.Replace("'", ""));
            var dbret = DBUtility.ExeMESReportSqlWithRes(sql, null);
            if (dbret.Count > 0)
            {
                var line = dbret[0];
                var data = new SnTestDataVM();
                data.ModuleSerialNum = sn;
                data.DataID = Convert.ToString(line[0]);
                data.TestTime = Convert.ToDateTime(line[1]);
                data.WhichTest = Convert.ToString(line[2]);
                data.ErrAbbr = "";
                if (!string.IsNullOrEmpty(Convert2Str(line[3])))
                {
                    data.ErrAbbr = Convert2Str(line[3]);
                }
                testdatalist.Add(data);
            }
            return testdatalist;
        }

    }

}