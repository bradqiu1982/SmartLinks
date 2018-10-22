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
            TestStation = "";
        }

        public SnTestDataVM(string did,string sn,string pn,string wt,string err,string mes, DateTime tm,string ts)
        {
            DataID = did;
            ModuleSerialNum = sn;
            PN = pn;
            WhichTest = wt;
            ErrAbbr = err;
            MESTab = mes;
            TestTime = tm;
            TestStation = ts;
        }

        public string DataID { set; get; }
        public string ModuleSerialNum { set; get; }
        public string PN { set; get; }
        public string WhichTest { set; get; }
        public string ErrAbbr { set; get; }
        public string MESTab { set; get; }
        public DateTime TestTime { set; get; }
        public string TestStation { set; get; }

        private List<DataField> pdatafield = new List<DataField>();
        public List<DataField> DFieldList {
            get { return pdatafield; }
            set { pdatafield.Clear();
                pdatafield.AddRange(value);
            }
        }


        private static string Convert2Str(object obj)
        {
            try
            {
                if (obj.Equals(null))
                { return string.Empty; }
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static List<string> RetrieveDCTableFromSn(string sn)
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
                    { realdc = "initial"; }
                    else
                    { realdc = dc.Substring(4); }

                    if (!dctabledict.ContainsKey(realdc))
                    {
                        dctabledict.Add(realdc, true);
                    }
                }//end if
            }//end foreach
            ret.AddRange(dctabledict.Keys);
            return ret;
        }


        public static void RetrieveTestData(List<ScrapTableItem> scraptable) {
            foreach (var stab in scraptable)
            {
                var mestablist = RetrieveDCTableFromSn(stab.SN);

                var ret = new List<SnTestDataVM>();
                foreach (var tabname in mestablist)
                {
                    var sql = "select top 1 dc_<DCTABLE>HistoryId,ModuleSerialNum, ErrAbbr, TestTimeStamp,assemblypartnum,WhichTest,TestStation from "
                        + " insite.dc_<DCTABLE> (nolock)  where ModuleSerialNum = '<modulesn>'";
                    sql = sql.Replace("<modulesn>", stab.SN).Replace("<DCTABLE>", tabname);
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
                        data.TestStation = Convert.ToString(line[6]);
                        data.MESTab = tabname.ToUpper().Trim();
                        ret.Add(data);
                    }
                }

                ret.Sort(delegate (SnTestDataVM d1, SnTestDataVM d2)
                {
                    return DateTime.Compare(d2.TestTime, d1.TestTime);
                });

                if (ret.Count > 0)
                {
                    stab.TestData = ret[0];
                }
                else
                {
                    stab.TestData = new SnTestDataVM();
                }
            }
        }


    }

}