﻿using System;
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

            ERTCWL_CH0 = "";
            ERTCWL_CH1 = "";
            ERTCWL_CH2 = "";
            ERTCWL_CH3 = "";

            FINALWL_CH0 = "";
            FINALWL_CH1 = "";
            FINALWL_CH2 = "";
            FINALWL_CH3 = "";
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
            var COCCOSDict = new Dictionary<string, string>();

            var sql = @"select ToContainer,FromContainer,FromPNDescription,FromProductName  FROM [PDMS].[dbo].[ComponentIssueSummary]  
                           where ToContainer in <SNCOND> 
                           and (FromPNDescription like '%PCBA%' or FromPNDescription like '%PLC%'  
                           or FromPNDescription like '%ON SUBMOUNT%' or  FromPNDescription like '%ON-SUBMOUNT%' or  FromPNDescription like '%ON-SILICON%') order by  IssueDate desc,FromPNDescription";

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
                else if (pndesc.Contains("ON SUBMOUNT"))
                {
                    if (!COCCOSDict.ContainsKey(sn))
                    { COCCOSDict.Add(sn, "DUAL PAD COC"); }
                }
                else if (pndesc.Contains("ON-SUBMOUNT"))
                {
                    if (!COCCOSDict.ContainsKey(sn))
                    { COCCOSDict.Add(sn, "NORMAL COC"); }
                }
                else if (pndesc.Contains("ON-SILICON"))
                {
                    if (!COCCOSDict.ContainsKey(sn))
                    { COCCOSDict.Add(sn, "COS"); }
                }
            }//end foreach

            ret.Add(PCBADict);
            ret.Add(PLCDict);
            ret.Add(COCCOSDict);
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


        private static Dictionary<string, string> LoadFWData(List<string> snlist,Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var stationlist = CfgUtility.GetSysConfig(ctrl)["CWDM4QUICKTESTSTATION"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var station in stationlist)
            {
                var filelist = TraceViewVM.LoadAllTraceView2Local(station, snlist, "QUICKTEST", ctrl);
                if (filelist.Count > 0)
                {
                    filelist.Sort(delegate (string obj1, string obj2)
                    {
                        var time1 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj1));
                        var time2 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj2));
                        return time2.CompareTo(time1);
                    });

                    foreach (var f in filelist)
                    {
                        var currentsn = "";
                        foreach (var tempsn in snlist)
                        {
                            if (f.ToUpper().Contains(tempsn))
                            {
                                currentsn = tempsn;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(currentsn))
                        { continue; }

                        if (ret.ContainsKey(currentsn))
                        { continue; }

                        var allline = System.IO.File.ReadAllLines(f);
                        foreach (var l in allline)
                        {
                            if (l.ToUpper().Contains("Firmware Version:".ToUpper()) || l.ToUpper().Contains("QSFP28_eCWDM FW Rev".ToUpper()))
                            {

                                var fws = l.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                if (!ret.ContainsKey(currentsn))
                                {
                                    ret.Add(currentsn, fws[fws.Length - 1].Replace("<", "").Replace(">", "").Replace("(", "").Replace(")", ""));
                                }
                                break;
                            }
                        }
                    }
                }//end if
            }

            return ret;
        }

        private static string LoadFWData2(string sn, Controller ctrl)
        {
            var ret = "";
            var stationlist = CfgUtility.GetSysConfig(ctrl)["CWDM4QUICKTESTSTATION"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var station in stationlist)
            {
                    var filelist = TraceViewVM.LoadAllTraceView2Local(station, sn, "QUICKTEST", ctrl);
                    if (filelist.Count > 0)
                    {
                        filelist.Sort(delegate (string obj1, string obj2) {
                            var time1 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj1));
                            var time2 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj2));
                            return time2.CompareTo(time1);
                        });

                        foreach (var f in filelist)
                        {
                            var allline = System.IO.File.ReadAllLines(f);
                            foreach (var l in allline)
                            {
                                if (l.ToUpper().Contains("Firmware Version:".ToUpper()) || l.ToUpper().Contains("QSFP28_eCWDM FW Rev".ToUpper()))
                                {
                                    var fws = l.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    ret = fws[fws.Length - 1].Replace("<", "").Replace(">", "").Replace("(", "").Replace(")", "");
                                    return ret;
                                }
                            }
                        }
                    }//end if

            }//end foreach

            return ret;
        }

        private static Dictionary<string,string> LoadTCBertInfo(List<string> snlist,Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var stationlist = syscfg["CWDM4FINAL1STATION"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();


            var allfiles = new List<string>();
            foreach (var station in stationlist)
            {
                var tempf = TraceViewVM.LoadAllTraceView2Local(station, snlist, "Final1", ctrl);
                allfiles.AddRange(tempf);
            }

            if (allfiles.Count > 0)
            {
                allfiles.Sort(delegate (string obj1, string obj2) {
                    var time1 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj1));
                    var time2 = DateTime.Parse(TraceViewVM.RetrieveTimeFromTraceViewName(obj2));
                    return time2.CompareTo(time1);
                });

                foreach (var f in allfiles)
                {
                    var tcbertpass = false;

                    if (ret.Count == snlist.Count)
                    { return ret; }

                    var currentsn = "";
                    foreach (var tempsn in snlist)
                    {
                        if (f.ToUpper().Contains(tempsn))
                        {
                            currentsn = tempsn;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(currentsn))
                    { continue; }

                    if (ret.ContainsKey(currentsn))
                    { continue; }

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
                    {
                        if (!ret.ContainsKey(currentsn))
                        { ret.Add(currentsn, tcbertpass ? "PASS" : "FAIL"); }
                    }
                }//end foreach
            }

            return ret;
        }

        private static Dictionary<string, Dictionary<string,string>> RetrieveWL(string sncond,string dctable,string corner)
        {
            var ret = new Dictionary<string, Dictionary<string, string>>();
            var sql = @"select dc.ModuleSerialNum,dce.CenterWLShift,dce.ChannelNumber from [InsiteDB].[insite].[dce_<tabname>_main] dce  
                          left join [InsiteDB].[insite].[dc_<tabname>] dc on dc.dc_<tabname>HistoryId = dce.ParentHistoryID 
                          where dc.ModuleSerialNum in <sncond> and dce.CornerID = '<corner>' and dce.CenterWLShift is not null order by dc.TestTimeStamp desc";
            sql = sql.Replace("<tabname>", dctable).Replace("<sncond>", sncond).Replace("<corner>", corner);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = Convert.ToString(line[0]);
                var wl = Convert.ToString(line[1]);
                var ch = Convert.ToString(line[2]);

                if (!ret.ContainsKey(sn))
                {
                    var chdict = new Dictionary<string, string>();
                    chdict.Add(ch, wl);
                    ret.Add(sn, chdict);
                }
                else
                {
                    var chdict = ret[sn];
                    if (!chdict.ContainsKey(ch))
                    { chdict.Add(ch,wl); }
                }
            }
            return ret;
        }


        public static List<CWDM4Data> QueryWL(List<string> snlist, Controller ctrl)
        {
            var retdata = new List<CWDM4Data>();
            foreach (var sn in snlist)
            {
                var tempvm = new CWDM4Data();
                tempvm.SN = sn.Trim().ToUpper();
                retdata.Add(tempvm);
            }
            var sncond = "('" + string.Join("','", snlist) + "')";

            var ertcdict = RetrieveWL(sncond, "ER_TEMPCOMP_TX", "25G2H");
            var finaldict = RetrieveWL(sncond, "FINAL_TX", "25G1H");
            foreach (var item in retdata)
            {
                if (ertcdict.ContainsKey(item.SN))
                {
                    var chdict = ertcdict[item.SN];
                    if (chdict.ContainsKey("0"))
                    { item.ERTCWL_CH0 = chdict["0"]; }
                    if (chdict.ContainsKey("1"))
                    { item.ERTCWL_CH1 = chdict["1"]; }
                    if (chdict.ContainsKey("2"))
                    { item.ERTCWL_CH2 = chdict["2"]; }
                    if (chdict.ContainsKey("3"))
                    { item.ERTCWL_CH3 = chdict["3"]; }
                }
                if (finaldict.ContainsKey(item.SN))
                {
                    var chdict = finaldict[item.SN];
                    if (chdict.ContainsKey("0"))
                    { item.FINALWL_CH0 = chdict["0"]; }
                    if (chdict.ContainsKey("1"))
                    { item.FINALWL_CH1 = chdict["1"]; }
                    if (chdict.ContainsKey("2"))
                    { item.FINALWL_CH2 = chdict["2"]; }
                    if (chdict.ContainsKey("3"))
                    { item.FINALWL_CH3 = chdict["3"]; }
                }
            }//end foreach

            return retdata;
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
                var COCCOSDict = (Dictionary<string, string>)pcbaplcdata[2];
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
                    if (COCCOSDict.ContainsKey(item.SN))
                    {
                        item.COCCOS = COCCOSDict[item.SN];
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
                var FWDict = LoadFWData(cwdm4list, ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }
                    if (FWDict.ContainsKey(item.SN))
                    { item.FW = FWDict[item.SN]; }
                }

                //load tcbert
                var tcbertdict = LoadTCBertInfo(cwdm4list, ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }
                    if (tcbertdict.ContainsKey(item.SN))
                    { item.TCBert = tcbertdict[item.SN]; }
                }

                var PNDict = CfgUtility.GetSysConfig(ctrl);
                foreach (var item in retdata)
                {
                    if (!item.IsCWDM4)
                    { continue; }

                    if (PNDict.ContainsKey("PLC-" + item.PLCPN))
                    { item.PLCVendor = PNDict["PLC-" + item.PLCPN]; }

                    if (PNDict.ContainsKey("SPEC-" + item.PN))
                    { item.Spec = PNDict["SPEC-" + item.PN]; }

                }

            }//end if (hascwdm4module)

            return retdata;
        }

        //private static void DownloadOETestStation(Controller ctrl)
        //{
        //    //station , date ,input/output
        //    var retdata = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        //    var dict = new Dictionary<string, bool>();


        //    var sql = "SELECT [ModuleSerialNum],[ErrAbbr],[TestStation],TestTimeStamp FROM [NPITrace].[dbo].[ProjectTestData] where ProjectKey = 'OE25LPFN' and WhichTest='Final_RX' and TestTimeStamp > '2018-03-01 00:00:00' order by TestStation,TestTimeStamp desc";
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql);
        //    foreach (var line in dbret)
        //    {
        //        var sn = Convert.ToString(line[0]);
        //        var fail = Convert.ToString(line[1]);
        //        var station = Convert.ToString(line[2]);
        //        var date = Convert.ToDateTime(line[3]).ToString("yyyy-MM-dd");

        //        var uniq = sn + station + date;
        //        if (dict.ContainsKey(uniq))
        //        {
        //            continue;
        //        }
        //        dict.Add(uniq, true);
                
        //        if (retdata.ContainsKey(station))
        //        {
        //            var sdict = retdata[station];
        //            if (sdict.ContainsKey(date))
        //            {
        //                var ddict = sdict[date];
        //                ddict["INPUT"] += 1;
        //                if (string.Compare(fail, "PASS", true) == 0)
        //                { ddict["OUTPUT"] += 1;}
        //            }
        //            else
        //            {
        //                var tempdict = new Dictionary<string, int>();
        //                tempdict.Add("INPUT", 1);
        //                if (string.Compare(fail, "PASS", true) == 0)
        //                { tempdict.Add("OUTPUT", 1); }
        //                else
        //                { tempdict.Add("OUTPUT", 0); }
        //                sdict.Add(date, tempdict);
        //            }
        //        }
        //        else
        //        {
        //            var vdict = new Dictionary<string, int>();
        //            vdict.Add("INPUT", 1);
        //            if (string.Compare(fail, "PASS", true) == 0)
        //            { vdict.Add("OUTPUT", 1); }
        //            else
        //            { vdict.Add("OUTPUT", 0); }
                    
        //            var datedict = new Dictionary<string, Dictionary<string, int>>();
        //            datedict.Add(date, vdict);
        //            retdata.Add(station, datedict);
        //        }
        //    }//end foreach

        //    var sb = new System.Text.StringBuilder((dbret.Count + 1) * 120);

        //    var lines = new List<string>();
        //    foreach (var skv in retdata)
        //    {
        //        foreach (var dkv in skv.Value)
        //        {
        //            sb.Append(skv.Key + "," + dkv.Key);
        //            sb.Append("," + dkv.Value["INPUT"]);
        //            sb.Append("," + dkv.Value["OUTPUT"]+",\r\n");
        //        }
        //    }

        //    System.IO.File.WriteAllText("d:\\OEFN_RX.csv", sb.ToString());
        //}

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

        public string ERTCWL_CH0 { set; get; }
        public string ERTCWL_CH1 { set; get; }
        public string ERTCWL_CH2 { set; get; }
        public string ERTCWL_CH3 { set; get; }

        public string FINALWL_CH0 { set; get; }
        public string FINALWL_CH1 { set; get; }
        public string FINALWL_CH2 { set; get; }
        public string FINALWL_CH3 { set; get; }
    }
}