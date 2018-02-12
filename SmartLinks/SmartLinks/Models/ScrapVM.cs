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
            FailureCode = "";
            Result = "";
            testdata = null;
            rule = null;
        }

        public string SN { set; get; }
        public string DateCode { set; get; }
        public string PN { set; get; }
        public string PNKey { set; get; }
        public string WhichTest { set; get; }
        public string FailureCode { set; get; }
        public string Result { set; get; }
        SnTestDataVM testdata { set; get; }
        PnRulesVM rule { set; get; }
    }

    public class ScrapVM
    {
        public static List<ScrapTableItem> RetrievePNBySNDC(List<ScrapTableItem> inputdata)
        {
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
            var pnpnkeymap = PnMainVM.PNPNKeyMap();
            foreach (var item in scraptable)
            {
                if (pnpnkeymap.ContainsKey(item.PN))
                {
                    var pnkey = pnpnkeymap[item.PN];
                    item.PNKey = pnkey;
                    if (pnmesmap.ContainsKey(item.MesTab + pnkey))
                    {
                        item.WhichTest = pnmesmap[item.MesTab + pnkey];
                    }
                }
            }//end foreach
        }

    }
}