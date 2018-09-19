using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartLinks.Models
{
    public class CWDM4Data
    {
        //@"select ModuleSerialNum,SpecVersion,TestTimeStamp,TestStation from dc_QuickTest where ModuleSerialNum in ('X03AC41','X05A2XR')"

        public CWDM4Data()
        {
            SN="";
            PN="";
            CurrentStep="";
            PCBASN="";
            PCBARev="";
            PLCPN="";
            PLCVendor="";
            SHTOL = "NA";
            FW = "";
            TCBert = "NA";
            Spec = "";
            COCCOS = "";
            TXEye = "";
            RXEye = "";
            PNDesc = "";
            IsCWDM4 = false;
        }

        private static Dictionary<string, CWDM4Data> LoadCurrentStepAndPN(string sncond)
        {
            var ret = new Dictionary<string, CWDM4Data>();

            var sql = @"select c.ContainerName,w.WorkflowStepName,pb.ProductName,pd.Description from [InsiteDB].[insite].[Container] c (nolock) 
	                    left join InsiteDB.insite.CurrentStatus cs (nolock) on cs.CurrentStatusId = c.CurrentStatusId 
	                    left join InsiteDB.insite.WorkflowStep w (nolock) on w.WorkflowStepId = cs.WorkflowStepId 
	                    left join [InsiteDB].[insite].Product pd (nolock) on c.ProductId = pd.ProductId 
	                    left join [InsiteDB].[insite].ProductBase pb (nolock) on pd.ProductBaseId =  pb.ProductBaseId 
	                    where c.ContainerName in <SNCOND>";
            sql = sql.Replace("<SNCOND>", sncond);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]).ToUpper();
                var step = Convert.ToString(line[1]);
                var pn = Convert.ToString(line[2]);
                var pndesc = Convert.ToString(line[3]);

                if (!ret.ContainsKey(sn))
                {
                    var tempvm = new CWDM4Data();
                    tempvm.SN = sn;
                    tempvm.CurrentStep = step;
                    tempvm.PN = pn;
                    tempvm.PNDesc = pndesc;
                    ret.Add(sn, tempvm);
                }
            }

            return ret;
        }

        private static List<object> LoadPCBAPLCFromReport(string sncond)
        {
            var ret = new List<object>();

            var PCBADict = new Dictionary<string, string>();
            var PLCDict = new Dictionary<string, string>();
            var sql = @"select ToContainer,FromContainer,FromPNDescription,FromProductName  FROM [PDMS].[dbo].[ComponentIssueSummary]  
                           where ToContainer in <SNCOND> 
                           and (FromPNDescription like '%PCBA%' or FromPNDescription like '%PLC%') order by FromPNDescription";
            sql = sql.Replace("<SNCOND>", sncond);
            var dbret = DBUtility.ExeMESReportSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]).ToUpper().Trim();
                var fromsn = Convert.ToString(line[1]).ToUpper().Trim();
                var pndesc = Convert.ToString(line[2]).ToUpper();
                var frompn = Convert.ToString(line[3]).ToUpper().Trim();

                if (pndesc.Contains("PCBA"))
                {
                    if (!PCBADict.ContainsKey(sn))
                    {
                        PCBADict.Add(sn, fromsn);
                    }
                }
                else if (pndesc.Contains("PLC"))
                {
                    if (!PLCDict.ContainsKey(sn))
                    {
                        PLCDict.Add(sn, frompn);
                    }
                }
            }//end foreach

            ret.Add(PCBADict);
            ret.Add(PLCDict);
            return ret;
        }

        private static Dictionary<string,string> LoadPCBARev(List<CWDM4Data> retdata)
        {
            var ret = new Dictionary<string, string>();
            var pcbasnlist = new List<string>();
            foreach (var item in retdata)
            {
                if (!string.IsNullOrEmpty(item.PCBASN)) {
                    pcbasnlist.Add(item.PCBASN);
                }
            }

            if (pcbasnlist.Count > 0)
            {
                var sncond = "('" + string.Join("','", pcbasnlist) + "')";
                var sql = @"select c.ContainerName,ProductRevision FROM [InsiteDB].[insite].[Product] pd (nolock) 
                            left join [InsiteDB].[insite].[Container] c (nolock) on c.ProductId = pd.ProductId 
                            where c.ContainerName in <SNCOND>";
                sql = sql.Replace("<SNCOND>", sncond);
                var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var sn = Convert.ToString(line[0]).ToUpper().Trim();
                    var rev = Convert.ToString(line[1]);
                    if (!ret.ContainsKey(sn))
                    {
                        ret.Add(sn, rev);
                    }
                }
            }

            return ret;
        }


        private static Dictionary<string, string> LoadFWData(List<CWDM4Data> retdata)
        {
            foreach (var item in retdata)
            {
                if (!item.IsCWDM4)
                { continue; }


            }
        }

        public static List<CWDM4Data> LoadCWDM4Info(List<string> snlist, Controller ctrl)
        {
            var retdata = new List<CWDM4Data>();
            foreach (var sn in snlist)
            {
                var tempvm = new CWDM4Data();
                tempvm.SN = sn.Trim().ToUpper();
                retdata.Add(tempvm);
            }

            var hascwdm4module = false;
            //load base info
            var sncond = "('" + string.Join("','", snlist) + "')";
            var basinfodict = LoadCurrentStepAndPN(sncond);
            foreach (var item in retdata)
            {
                if (basinfodict.ContainsKey(item.SN))
                {
                    item.CurrentStep = basinfodict[item.SN].CurrentStep;
                    item.PN = basinfodict[item.SN].PN;
                    item.PNDesc = basinfodict[item.SN].PNDesc;
                    if (item.PNDesc.ToUpper().Contains("FTLC1157"))
                    {
                        item.IsCWDM4 = true;
                        hascwdm4module = true;
                    }
                    else
                    {
                        item.PCBARev = "NOT CWDM4 Module";
                    }
                }
            }

            if (hascwdm4module)
            {
                var cwdm4list = new List<string>();
                foreach (var item in retdata)
                {
                    if (item.IsCWDM4)
                    {
                        cwdm4list.Add(item.SN);
                    }
                }
                sncond = "('" + string.Join("','", cwdm4list) + "')";


                //load pcba sn, plc pn
                var pcbaplcdata = LoadPCBAPLCFromReport(sncond);
                var PCBADict = (Dictionary<string, string>)pcbaplcdata[0];
                var PLCDict = (Dictionary<string, string>)pcbaplcdata[1];
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }

                    if (PCBADict.ContainsKey(item.SN))
                    {
                        item.PCBASN = PCBADict[item.SN];
                    }
                    if (PLCDict.ContainsKey(item.SN))
                    {
                        item.PLCPN = PLCDict[item.SN];
                    }
                }

                //load pcba rev by pcba sn
                var PCBARevDict = LoadPCBARev(retdata);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }

                    if (!string.IsNullOrEmpty(item.PCBASN))
                    {
                        if (PCBARevDict.ContainsKey(item.PCBASN))
                        { item.PCBARev = PCBARevDict[item.PCBASN]; }
                    }
                }

                //Load S-HTOL SN dict
                var HTOLDict = ExternalDataCollector.LoadSHTOLData(ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }
                    if (HTOLDict.ContainsKey(item.SN))
                    { item.SHTOL = HTOLDict[item.SN]; }
                }

                

            }//end if (hascwdm4module)




            return retdata;
        }


        public string SN { set; get; }
        public string PN { set; get; }
        public string CurrentStep { set; get; }
        public string PNDesc { set; get; }
        public bool IsCWDM4 { set; get; }


        public string PCBASN { set; get; }
        public string PCBARev { set; get; }

        public string PLCPN { set; get; }
        public string PLCVendor { set; get; }

        public string SHTOL { set; get; }

        public string FW { set; get; }
        public string TCBert { set; get; }

        public string Spec { set; get; }
        public string COCCOS { set; get; }

        public string TXEye { set; get; }
        public string RXEye { set; get; }
    }
}