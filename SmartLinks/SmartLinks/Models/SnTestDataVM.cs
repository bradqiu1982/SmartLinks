using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class DataField
    {
        public DataField()
        {
            FieldName = "";
            FieldVal = "";
        }

        public string FieldName { set; get; }
        public string FieldVal { set; get; }
    }

    public class SnTestDataVM
    {
        public SnTestDataVM()
        {
            DataID = "";
            ModuleSerialNum = "";
            PN = "";
            WhichTest = "";
            ErrAbbr = "";
            MESTab = "";
            TestTime = DateTime.Parse("1982-05-06 10:00:00");
        }

        public SnTestDataVM(string did,string sn,string pn,string wt,string err,string mes, DateTime tm)
        {
            DataID = did;
            ModuleSerialNum = sn;
            PN = pn;
            WhichTest = wt;
            ErrAbbr = err;
            MESTab = mes;
            TestTime = tm;
        }

        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public string PN { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string MESTab { set; get; }
        public DateTime TestTime { set; get; }


        private List<DataField> pdatafield = new List<DataField>();
        public List<DataField> DFieldList {
            get { return pdatafield; }
            set { pdatafield.Clear();
                pdatafield.AddRange(value);
            }
        }

        public static void RetrieveTestData(List<ScrapTableItem> scraptable)
        {
            var pnkeydict = new Dictionary<string, bool>();
            foreach (var item in scraptable)
            {
                if (!string.IsNullOrEmpty(item.PNKey) && !pnkeydict.ContainsKey(item.PNKey))
                {
                    pnkeydict.Add(item.PNKey, true);
                }//end if
            }//end foreach

            var mestablist = PnMESVM.RetrievePnMESTabByPNDict(pnkeydict);

            var sncond = " ('";
            foreach (var item in scraptable)
            {
                if (!string.IsNullOrEmpty(item.SN))
                {
                    sncond = sncond + item.SN + "','";
                }
            }
            sncond = sncond.Substring(0, sncond.Length - 2);
            sncond = sncond + ") ";

            var ret = new List<SnTestDataVM>();
            foreach (var tabname in mestablist)
            {
                var sql = "select dc_<DCTABLE>HistoryId,ModuleSerialNum, ErrAbbr, TestTimeStamp,assemblypartnum,WhichTest from "
                    + " insite.dc_<DCTABLE> (nolock)  where ModuleSerialNum in <SNCOND>";
                sql = sql.Replace("<SNCOND>", sncond).Replace("<DCTABLE>",tabname);
                var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var data = new SnTestDataVM();
                    data.DataID = Convert.ToString(line[0]);
                    data.ModuleSerialNum = Convert.ToString(line[1]);
                    data.ErrAbbr = Convert.ToString(line[2]);
                    data.TestTime = Convert.ToDateTime(line[3]);
                    data.PN = Convert.ToString(line[4]);
                    data.WhichTest = Convert.ToString(line[5]);
                    data.MESTab = tabname;
                    ret.Add(data);
                }
            }

            ret.Sort(delegate (SnTestDataVM d1, SnTestDataVM d2)
            {
                return DateTime.Compare(d2.TestTime, d1.TestTime);
            });

            var sndict = new Dictionary<string, bool>();
            var latestdata = new List<SnTestDataVM>();
            foreach (var item in ret)
            {
                if (!sndict.ContainsKey(item.ModuleSerialNum))
                {
                    sndict.Add(item.ModuleSerialNum, true);
                    latestdata.Add(item);
                }
            }

            foreach (var desdata in scraptable)
            {
                foreach (var srcdata in latestdata)
                {
                    if (string.Compare(desdata.SN, srcdata.ModuleSerialNum, true) == 0)
                    {
                        desdata.TestData = new SnTestDataVM(srcdata.DataID, srcdata.ModuleSerialNum
                            , srcdata.PN, srcdata.WhichTest, srcdata.ErrAbbr, srcdata.MESTab, srcdata.TestTime);
                        break;
                    }
                }//end foreach
            }//end foreach

            foreach (var desdata in scraptable)
            {
                if (desdata.TestData == null)
                {
                    desdata.TestData = new SnTestDataVM();
                }
            }
       }


    }

}