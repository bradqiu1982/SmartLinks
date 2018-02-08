using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class WAFERSTATUS
    {
        public static string GOOD = "GOOD";
        public static string NG = "NG";
    }

    public class WaferTableItem
    {
        public WaferTableItem()
        {
            SN = "";
            DateCode = "";
            WaferNum = "";
            PN = "";
            Status = "";
        }

        public string SN { set; get; }
        public string DateCode { set; get; }
        public string WaferNum { set; get; }
        public string PN { set; get; }
        public string Status { set; get; }
    }

    public class WaferPackVM
    {
        public static List<WaferTableItem> RetrieveSNByDateCode(List<string> datecodelist)
        {
            var ret = new List<WaferTableItem>();
            var datecond = " ('";
            foreach (var item in datecodelist)
            {
                datecond = datecond + item + "','";
            }
            datecond = datecond.Substring(0, datecond.Length - 2);
            datecond = datecond + ") ";

            var sql = "select ContainerName,DateCode FROM [InsiteDB].[insite].[Container] where DateCode in <datecond>";
            sql = sql.Replace("<datecond>", datecond);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret) {
                var tempvm = new WaferTableItem();
                tempvm.SN = Convert.ToString(line[0]);
                tempvm.DateCode = Convert.ToString(line[1]);
                if (!string.IsNullOrEmpty(tempvm.SN))
                {
                    ret.Add(tempvm);
                }
            }
            return ret;
        }

        public static void RetrieveWaferBySN(List<WaferTableItem> desdata)
        {
            var sncond = " ('";
            foreach (var item in desdata)
            {
                sncond = sncond + item.SN + "','";
            }
            sncond = sncond.Substring(0, sncond.Length - 2);
            sncond = sncond + ") ";

            var tetmpres = new List<WaferTableItem>();

            var sql = "select ToContainer,Wafer,FromProductName,FromPNDescription from [PDMS].[dbo].[ComponentIssueSummary] where ToContainer in <SNCOND> and Wafer is not null and FromPNDescription is not null order by Wafer";
            sql = sql.Replace("<SNCOND>", sncond);

            var dbret = DBUtility.ExeMESReportSqlWithRes(sql);
            foreach(var line in dbret)
            {
                var pndesc = Convert.ToString(line[3]);

                if ((pndesc.ToUpper().Contains("LD,") && pndesc.ToUpper().Contains("VCSEL,"))
                        || (pndesc.ToUpper().Contains("CSG") && (pndesc.ToUpper().Contains("INGAAS VCSEL") || pndesc.ToUpper().Contains("VCSEL ARRAY"))))
                {
                    var tempvm = new WaferTableItem();
                    tempvm.SN = Convert.ToString(line[0]);
                    tempvm.WaferNum = Convert.ToString(line[1]);
                    if (tempvm.WaferNum.Length > 3)
                    {
                        tempvm.WaferNum = tempvm.WaferNum.Substring(0, tempvm.WaferNum.Length - 3);
                    }
                    tempvm.PN = Convert.ToString(line[2]);
                    tetmpres.Add(tempvm);
                }
            }

            foreach (var des in desdata)
            {
                foreach (var src in tetmpres)
                {
                    if (string.Compare(des.SN, src.SN,true) == 0)
                    {
                        des.WaferNum = src.WaferNum;
                        des.PN = src.PN;
                        break;
                    }
                }//end foreach
            }//end foreach
        }

        public static void UpdateNGWafer(List<string> waferlist)
        {
            var sql = "delete from NGWafer";
            DBUtility.ExeLocalSqlNoRes(sql);
            foreach (var item in waferlist)
            {
                sql = "insert into NGWafer(WaferNo) values('<WaferNo>')";
                sql = sql.Replace("<WaferNo>", item.Replace("'", "").Trim());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static List<string> RetrieveNGWafer()
        {
            var ret = new List<string>();
            var sql = "select WaferNo from NGWafer order by WaferNo";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static Dictionary<string, bool> RetrieveNGWaferDict()
        {
            var ret = new Dictionary<string, bool>();
            var waferlist = RetrieveNGWafer();
            foreach (var item in waferlist)
            {
                if (!ret.ContainsKey(item))
                {
                    ret.Add(item, true);
                }
            }
            return ret;
        }

    }
}