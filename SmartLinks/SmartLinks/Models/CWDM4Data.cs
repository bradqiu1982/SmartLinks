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
            ORL = "";
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


        private static Dictionary<string, string> LoadFWData(string sncond,Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var sndict = new Dictionary<string, bool>();
            var sql = @"select ModuleSerialNum,TestStation,TestTimeStamp FROM [InsiteDB].[insite].[dc_QuickTest] where ModuleSerialNum in <SNCOND> order by TestTimeStamp desc";
            sql = sql.Replace("<SNCOND>", sncond);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]).ToUpper().Trim();
                var station = Convert.ToString(line[1]).ToUpper().Trim();
                var testtime = Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                if (!sndict.ContainsKey(sn))
                {
                    sndict.Add(sn, true);
                    var filelist = TraceViewVM.LoadAllTraceView2Local(station, sn, "QUICKTEST", ctrl);
                    if (filelist.Count > 0)
                    {
                        filelist.Sort(delegate(string obj1,string obj2) {
                            var time1 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj1));
                            var time2 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj2));
                            return time2.CompareTo(time1);
                        });
                        foreach(var f in filelist)
                        {
                            var allline = System.IO.File.ReadAllLines(f);
                            foreach (var l in allline)
                            {
                                if (l.ToUpper().Contains("Firmware Version:".ToUpper()) || l.ToUpper().Contains("QSFP28_eCWDM FW Rev".ToUpper()))
                                {
                                    var fws = l.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    if (!ret.ContainsKey(sn))
                                    {
                                        ret.Add(sn, fws[fws.Length - 1].Replace("<","").Replace(">", "").Replace("(", "").Replace(")", ""));
                                    }
                                    break;
                                }
                            }
                        }
                    }//end if
                }//end if
           }//end foreach

            return ret;
        }

        private static void LoadTCBertInfo(List<CWDM4Data> retdata,Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var stationlist = syscfg["CWDM4FINAL1STATION"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in retdata)
            {
                if (!item.IsCWDM4)
                { continue; }

                var allfiles = new List<string>();
                foreach (var station in stationlist)
                {
                    var tempf = TraceViewVM.LoadAllTraceView2Local(station, item.SN, "Final1", ctrl);
                    allfiles.AddRange(tempf);
                }

                if (allfiles.Count > 0)
                {
                    item.TCBert = "";
                    var tcbertpass = false;

                    allfiles.Sort(delegate (string obj1, string obj2) {
                        var time1 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj1));
                        var time2 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj2));
                        return time2.CompareTo(time1);
                    });

                    foreach (var f in allfiles)
                    {
                        bool validfile = false;

                        var allline = System.IO.File.ReadAllLines(f);
                        foreach (var l in allline)
                        {
                            if (l.ToUpper().Contains("SoakTest".ToUpper()))
                            { validfile = true; }

                            if (l.ToUpper().Contains("Sequence Main Passed".ToUpper()) && validfile)
                            {
                                tcbertpass = true;
                                break;
                            }

                        }//end foreach

                        if (validfile)
                            { break; }
                    }//end foreach

                    if (tcbertpass)
                    { item.TCBert += "PASS"; }
                    else
                    { item.TCBert += "FAIL"; }
                }
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
                        item.PCBARev = "NOT CWDM4";
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

                //load ORL SN dict
                var ORLDict = ExternalDataCollector.LoadORLData(ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }
                    if (ORLDict.ContainsKey(item.SN))
                    { item.ORL = ORLDict[item.SN]; }
                }

                //load FW
                var FWDict = LoadFWData(sncond,ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }
                    if (FWDict.ContainsKey(item.SN))
                    { item.FW = FWDict[item.SN]; }
                }

                //load tcbert
                LoadTCBertInfo(retdata, ctrl);

                var PNDict = CfgUtility.GetSysConfig(ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }

                    if (PNDict.ContainsKey("PLC-" + item.PLCPN))
                    { item.PLCVendor = PNDict["PLC-" + item.PLCPN]; }

                    if (PNDict.ContainsKey("SPEC-" + item.PN))
                    { item.Spec = PNDict["SPEC-" + item.PN]; }

                    if (PNDict.ContainsKey("COCCOS-" + item.PN))
                    { item.COCCOS = PNDict["COCCOS-" + item.PN]; }
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
        public string ORL { set; get; }

        public string ORLTX {
            get {
                if (ORL.Contains("/"))
                {
                    return ORL.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("TX:", "").Replace("RX:", "");
                }
                else
                {
                    return "";
                }
            }
        }

        public string ORLRX
        {
            get
            {
                if (ORL.Contains("/"))
                {
                    return ORL.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("TX:", "").Replace("RX:", "");
                }
                else
                {
                    return "";
                }
            }
        }

        public string FW { set; get; }
        public string TCBert { set; get; }

        public string Spec { set; get; }
        public string COCCOS { set; get; }

        public string TXEye { set; get; }
        public string RXEye { set; get; }
    }
}