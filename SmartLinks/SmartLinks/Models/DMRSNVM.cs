using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace SmartLinks.Models
{
    public class DMRSNVM
    {
        private static string RetrieveLatestDMRDate(string prodline)
        {
            var sql = "select top 1 DMRDate from DMRSNVM where DMRProdLine = @DMRProdLine order by DMRDate desc";
            var dict = new Dictionary<string, string>();
            dict.Add("@DMRProdLine", prodline);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return Convert.ToDateTime(line[0]).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return string.Empty;
        }

        private static void UpdateDMRSN(string prodline,Controller ctrl)
        {
            var dict = new Dictionary<string, string>();
            var sql = "SELECT distinct [DMR_ID],[Prod_Line],[Created_at],[Created_By],[File_URL]  FROM [eDMR].[dbo].[DMR_Detail_List_View] where Prod_Line =  @Prod_Line and File_URL is not null order by [Created_at] asc";
            dict.Add("@Prod_Line", prodline);
            var latestdate = RetrieveLatestDMRDate(prodline);
            if (!string.IsNullOrEmpty(latestdate))
            {
                sql = @"SELECT distinct [DMR_ID],[Prod_Line],[Created_at],[Created_By],[File_URL]  FROM [eDMR].[dbo].[DMR_Detail_List_View] 
                        where Prod_Line =  @Prod_Line and Created_at > @StartDate and Created_at <= @EndDate and File_URL is not null";
                dict.Add("@StartDate", latestdate);
                dict.Add("@EndDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            var dmrlist = new List<DMRSNVM>();

            var dbret = DBUtility.ExeDMRSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                try
                {
                    var tempvm = new DMRSNVM();
                    tempvm.DMRID = Convert.ToString(line[0]);
                    tempvm.DMRProdLine = Convert.ToString(line[1]);
                    tempvm.DMRDate = Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                    tempvm.DMRCreater = Convert.ToString(line[3]);
                    tempvm.DMRFileURL = Convert.ToString(line[4]);
                    dmrlist.Add(tempvm);
                }
                catch (Exception ex) { }

            }

            sql = @"insert into DMRSNVM(DMRID,DMRProdLine,DMRDate,DMRCreater,SN,SNFailure) values(@DMRID,@DMRProdLine,@DMRDate,@DMRCreater,@SN,@SNFailure)";

            foreach (var vm in dmrlist)
            {
                var lfs = ExternalDataCollector.DownloadShareFile(vm.DMRFileURL, ctrl);
                if (!string.IsNullOrEmpty(lfs) && ExternalDataCollector.FileExist(ctrl, lfs))
                {
                    var sndatalist = ExternalDataCollector.RetrieveDataFromExcelWithAuth(ctrl, lfs, null, 5);
                    foreach (var l in sndatalist)
                    {
                        var sn = l[0];
                        if (sn.Length != 7) {
                            continue;
                        }

                        var failure = l[2];
                        dict = new Dictionary<string, string>();
                        dict.Add("@DMRID",vm.DMRID);
                        dict.Add("@DMRProdLine",vm.DMRProdLine);
                        dict.Add("@DMRDate",vm.DMRDate);
                        dict.Add("@DMRCreater",vm.DMRCreater);
                        dict.Add("@SN",sn);
                        dict.Add("@SNFailure",failure);
                        DBUtility.ExeLocalSqlNoRes(sql, dict);
                    }
                }
            }

        }

        private static Dictionary<string, DMRSNVM> UpdateSNStatus(string prodline)
        {
            var ret = new Dictionary<string, DMRSNVM>();

            var dict = new Dictionary<string, string>();
            dict.Add("@DMRProdLine", prodline);
            var sql = "select distinct SN FROM DMRSNVM where SNStatus <> 'SCRAP' and SNStatus <> 'CLOSED'  and DMROAStatus <> 'X'  and DMRProdLine=@DMRProdLine";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
            var snlist = new List<string>();

            foreach (var line in dbret)
            { snlist.Add(Convert.ToString(line[0])); }

            if (snlist.Count > 0)
            {
                var sncond = "('" + string.Join("','", snlist) + "')";
                sql = @"select distinct c.ContainerName, CASE WHEN (c.STATUS IS NULL) THEN 'NONEXIST' 
	                        ELSE CASE WHEN (c.STATUS = 1 AND c.holdReasonId IS NULL) THEN 'ACTIVE'
	                        ELSE CASE WHEN  CHARINDEX('scrap',ws.WorkflowStepName) > 0 THEN 'SCRAP' 
	                        ELSE CASE WHEN (c.STATUS = 1 AND c.holdReasonId IS NOT NULL) THEN 'HOLD' 
	                        ELSE CASE WHEN (c.STATUS = 2 AND c.qty > 0 AND ws.workflowstepname <> 'SHIPPING') THEN 'CLOSED' 
	                        ELSE  CASE WHEN (c.STATUS = 2 AND c.qty = 0) THEN 'SCRAP' 
	                        ELSE CASE WHEN (c.STATUS = 4) THEN 'ISSUED' 
	                        ELSE CASE WHEN (ws.workflowstepname = 'SHIPPING' AND c.qty > 0 AND c.STATUS = 2) THEN 'SHIPPING' ELSE 'UNKNOW' 
	                        END 
	                        END 
	                        END 
	                        END 
	                        END 
	                        END 
	                        END
	                        END SNStatus ,jo.MfgOrderName
                        ,pb.ProductName,wb.WorkflowName CRTWFName,ws.WorkflowStepName CRTWFStepName,c.LastActivityDateGMT
                            from InsiteDB.insite.Container (nolock) c
                        left join InsiteDB.insite.MfgOrder (nolock) jo on c.MfgOrderId = jo.MfgOrderId
                        left join InsiteDB.insite.CurrentStatus (nolock) cs on c.CurrentStatusId = cs.CurrentStatusId
                        left join InsiteDB.insite.WorkflowStep (nolock) ws on cs.WorkflowStepId = ws.WorkflowStepId
                        left join InsiteDB.insite.Workflow (nolock) wf on ws.WorkflowId = wf.WorkflowId
                        left join InsiteDB.insite.WorkflowBase (nolock) wb on wf.WorkflowBaseId = wb.WorkflowBaseId
                        left join [InsiteDB].[insite].[Product]  (nolock) pd on pd.ProductId = c.ProductId
                        left join [InsiteDB].[insite].[ProductBase]  (nolock) pb on pb.ProductBaseId = pd.ProductBaseId
                        where c.ContainerName in <sncond> and Len(c.ContainerName) = 7";
                sql = sql.Replace("<sncond>", sncond);
                dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                var sninfo = new List<DMRSNVM>();
                foreach (var l in dbret)
                {
                    var tempvm = new DMRSNVM();
                    tempvm.SN = O2S(l[0]).ToUpper().Trim();
                    tempvm.SNStatus = O2S(l[1]);
                    tempvm.JO = O2S(l[2]);
                    tempvm.PN = O2S(l[3]);
                    tempvm.WorkFlow = O2S(l[4]);
                    tempvm.WorkFlowStep = O2S(l[5]);
                    tempvm.DMRDate = O2T(l[6]);
                    sninfo.Add(tempvm);

                    if (!ret.ContainsKey(tempvm.SN))
                    { ret.Add(tempvm.SN,tempvm); }
                }

                dict = new Dictionary<string, string>();
                sql = @"update DMRSNVM set SNStatus=@SNStatus,JO=@JO,PN=@PN,WorkFlow=@WorkFlow,WorkFlowStep=@WorkFlowStep where SN=@SN and DMRProdLine=@DMRProdLine";
                foreach (var s in sninfo)
                {
                    dict = new Dictionary<string, string>();
                    dict.Add("@SNStatus",s.SNStatus);
                    dict.Add("@JO",s.JO);
                    dict.Add("@PN",s.PN);
                    dict.Add("@WorkFlow",s.WorkFlow);
                    dict.Add("@WorkFlowStep",s.WorkFlowStep);
                    dict.Add("@SN",s.SN);
                    dict.Add("@DMRProdLine",prodline);
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }

            return ret;
        }

        private static void UpdateDMRStep(string prodline, Dictionary<string, DMRSNVM> snlaststep)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@DMRProdLine", prodline);
            var sql = "select distinct SN FROM DMRSNVM where DMRProdLine=@DMRProdLine  and DMROAStatus <> 'X'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            var snlist = new List<string>();

            foreach (var line in dbret)
            { snlist.Add(Convert.ToString(line[0]).ToUpper().Trim()); }

            if (snlist.Count > 0)
            {
                var sncond = "('" + string.Join("','", snlist) + "')";

                var snstepdict = new Dictionary<string, DMRSNVM>();
                foreach (var s in snlist)
                { snstepdict.Add(s, new DMRSNVM()); }

                var snworkflowdict = new Dictionary<string, List<DMRSNVM>>();

                sql = @"SELECT distinct c.ContainerName as SerialName,ws.WorkflowStepName ,hml.MfgDate
		                    FROM InsiteDB.insite.container c with (nolock) 
	                    left join InsiteDB.insite.historyMainline hml with (nolock) on c.containerId = hml.containerId
	                    left join InsiteDB.insite.MoveHistory mv with (nolock) on mv.HistoryMainlineId= hml.HistoryMainlineId
	                    left join InsiteDB.insite.workflowstep ws(nolock) on  ws.WorkflowStepId  = hml.WorkflowStepId
	                    where c.ContainerName in <sncond> and mv.MoveInTime is not null and ws.WorkflowStepName  is not null
                        and hml.MfgDate is not null order by SerialName,hml.MfgDate asc";
                sql = sql.Replace("<sncond>", sncond);
                dbret = DBUtility.ExeRealMESSqlWithRes(sql);

                //split sn history workflowstep
                foreach (var l in dbret)
                {
                    try
                    {
                        var sn = O2S(l[0]).ToUpper().Trim();
                        var step = O2S(l[1]);
                        var ustep = step.ToUpper();
                        var dt = O2T(l[2]);

                        var tempvm = new DMRSNVM();
                        tempvm.SN = sn;
                        tempvm.WorkFlowStep = step;
                        tempvm.DMRDate = dt;

                        if (snworkflowdict.ContainsKey(sn))
                        {
                            snworkflowdict[sn].Add(tempvm);
                        }
                        else
                        {
                            var templist = new List<DMRSNVM>();
                            templist.Add(tempvm);
                            snworkflowdict.Add(sn, templist);
                        }
                    }
                    catch (Exception ex) { }
                }//end foreach

                //append current workflowstep
                foreach (var wkv in snworkflowdict)
                {
                    if (snlaststep.ContainsKey(wkv.Key))
                    {
                        if (string.Compare(wkv.Value[wkv.Value.Count - 1].WorkFlowStep, snlaststep[wkv.Key].WorkFlowStep, true) != 0)
                        {
                            wkv.Value.Add(snlaststep[wkv.Key]);
                        }
                    }
                }

                //scan workflow step
                foreach (var wkv in snworkflowdict)
                {
                    var previousstep = new DMRSNVM();

                    foreach (var item in wkv.Value)
                    {
                        var sn = item.SN;
                        var step = item.WorkFlowStep;
                        var ustep = step.ToUpper();
                        var dt = item.DMRDate;

                        if (!string.IsNullOrEmpty(snstepdict[sn].DMRRepairStep)
                                && snstepdict[sn].DMRRepairStep.ToUpper().Contains("COMPONENTS")
                               && snstepdict[sn].DMRRepairStep.ToUpper().Contains("REMOVE"))
                        {
                            snstepdict[sn].DMRRepairStep = step;
                            //snstepdict[sn].DMRRepairTime = dt;
                        }

                        if (!string.IsNullOrEmpty(snstepdict[sn].DMRStoreStep)
                            && string.IsNullOrEmpty(snstepdict[sn].DMRRepairStep))
                        {
                            snstepdict[sn].DMRRepairStep = step;
                            snstepdict[sn].DMRRepairTime = dt;
                        }

                        if (!string.IsNullOrEmpty(snstepdict[sn].DMRStoreStep)
                            && ustep.Contains("MAIN") && ustep.Contains("STORE")
                            && string.IsNullOrEmpty(snstepdict[sn].DMRReturnStep))
                        {
                            snstepdict[sn].DMRReturnStep = step;
                            snstepdict[sn].DMRReturnTime = dt;
                        }

                        if (ustep.Contains("EQ") && ustep.Contains("INVENTORY"))
                        {
                            snstepdict[sn].DMRStoreStep = step;
                            snstepdict[sn].DMRStoreTime = dt;

                            snstepdict[sn].DMRRepairStep = "";
                            snstepdict[sn].DMRRepairTime = "";
                            snstepdict[sn].DMRReturnStep = "";
                            snstepdict[sn].DMRReturnTime = "";

                            if (string.Compare(previousstep.SN, sn) == 0 && !string.IsNullOrEmpty(previousstep.DMRStartStep))
                            {
                                snstepdict[sn].DMRStartStep = previousstep.DMRStartStep;
                                snstepdict[sn].DMRStartTime = previousstep.DMRStartTime;
                            }
                        }

                        previousstep.SN = sn;
                        previousstep.DMRStartStep = step;
                        previousstep.DMRStartTime = dt;
                    }//end foreach
                }//end foreach

                dict = new Dictionary<string, string>();
                sql = @"update DMRSNVM set DMRStartStep=@DMRStartStep,DMRStartTime=@DMRStartTime,DMRStoreStep=@DMRStoreStep,DMRStoreTime=@DMRStoreTime
                           ,DMRRepairStep=@DMRRepairStep,DMRRepairTime=@DMRRepairTime,DMRReturnStep=@DMRReturnStep,DMRReturnTime=@DMRReturnTime where SN=@SN and DMRProdLine=@DMRProdLine";
                foreach (var kv in snstepdict)
                {
                    dict = new Dictionary<string, string>();
                    dict.Add("@DMRStartStep",kv.Value.DMRStartStep);
                    dict.Add("@DMRStartTime", kv.Value.DMRStartTime);
                    dict.Add("@DMRStoreStep", kv.Value.DMRStoreStep);
                    dict.Add("@DMRStoreTime", kv.Value.DMRStoreTime);
                    dict.Add("@DMRRepairStep", kv.Value.DMRRepairStep);
                    dict.Add("@DMRRepairTime", kv.Value.DMRRepairTime);
                    dict.Add("@DMRReturnStep", kv.Value.DMRReturnStep);
                    dict.Add("@DMRReturnTime", kv.Value.DMRReturnTime);

                    dict.Add("@SN", kv.Key);
                    dict.Add("@DMRProdLine", prodline);
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }
        }

        private static void UpdateDMROAStatus(string prodline)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@DMRProdLine", prodline);
            var sql = "select distinct DMRID from DMRSNVM where DMRProdLine=@DMRProdLine and DMROAStatus <> 'C' and  DMROAStatus <> 'X'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
            var dmridlist = new List<string>();
            foreach (var l in dbret)
            { dmridlist.Add(O2S(l[0])); }

            if (dmridlist.Count > 0)
            {
                var oastatlist = new List<DMRSNVM>();

                var dmridcond = "('" + string.Join("','", dmridlist) + "')";
                sql = "SELECT distinct [DMR_ID],[Step_ID] ,[Status] FROM [eDMR].[dbo].[DMR_View_bk] where DMR_ID in <dmridcond>";
                sql = sql.Replace("<dmridcond>", dmridcond);
                dbret = DBUtility.ExeDMRSqlWithRes(sql);
                foreach (var l in dbret)
                {
                    var tempvm = new DMRSNVM();
                    tempvm.DMRID = O2S(l[0]);
                    tempvm.DMROAStep = O2S(l[1]);
                    tempvm.DMROAStatus = O2S(l[2]);
                    oastatlist.Add(tempvm);
                }

                dict = new Dictionary<string, string>();
                sql = @"update DMRSNVM set DMROAStep=@DMROAStep,DMROAStatus=@DMROAStatus where DMRID=@DMRID";
                foreach (var oa in oastatlist)
                {
                    dict = new Dictionary<string, string>();
                    dict.Add("@DMRID", oa.DMRID);
                    dict.Add("@DMROAStep", oa.DMROAStep);
                    dict.Add("@DMROAStatus", oa.DMROAStatus);
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
            }
            
        }

        private static string O2S(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        private static string O2T(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static void UpdateDMRSNStatus(string prodline,Controller ctrl)
        {
            if (ctrl.HttpContext.Cache.Get(prodline) == null)
            {
                UpdateDMRSN(prodline,ctrl);
                var snlaststep = UpdateSNStatus(prodline);
                UpdateDMRStep(prodline,snlaststep);
                UpdateDMROAStatus(prodline);

                var cachehour = Convert.ToDouble(CfgUtility.GetSysConfig(ctrl)["DMRCACHEHOUR"]);
                ctrl.HttpContext.Cache.Insert(prodline, "true", null, DateTime.Now.AddHours(cachehour), Cache.NoSlidingExpiration);
            }
        }

        public static List<DMRSNVM> RetrieveDMRSNData(string prodline, string startdate, string enddate, Controller ctrl)
        {
            UpdateDMRSNStatus(prodline, ctrl);

            var ret = new List<DMRSNVM>();
            var dict = new Dictionary<string, string>();
            dict.Add("@DMRProdLine", prodline);
            var sql = "";
            if (!string.IsNullOrEmpty(startdate) && !string.IsNullOrEmpty(enddate))
            {//period
                sql = @"SELECT DMRID,DMRProdLine,DMRDate,DMRCreater,SN,SNFailure,SNStatus,JO,PN,WorkFlow,WorkFlowStep
                      ,DMRStartStep,DMRStartTime,DMRStoreStep,DMRStoreTime,DMRRepairStep,DMRRepairTime,DMRReturnStep
                      ,DMRReturnTime,DMROAStep,DMROAStatus FROM DMRSNVM where DMRProdLine = @DMRProdLine and DMRDate > @startdate and DMRDate < @enddate and DMROAStatus <> 'X' order by DMRDate asc,DMRID";
                dict.Add("@startdate", startdate);
                dict.Add("@enddate", enddate);
            }
            else
            {//wip
                sql = @"SELECT DMRID,DMRProdLine,DMRDate,DMRCreater,SN,SNFailure,SNStatus,JO,PN,WorkFlow,WorkFlowStep
                      ,DMRStartStep,DMRStartTime,DMRStoreStep,DMRStoreTime,DMRRepairStep,DMRRepairTime,DMRReturnStep
                      ,DMRReturnTime,DMROAStep,DMROAStatus FROM DMRSNVM where DMRProdLine = @DMRProdLine and  SNStatus <> 'SCRAP' and SNStatus <> 'CLOSED' and DMROAStatus <> 'X'  order by DMRDate asc,DMRID";
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var l in dbret)
            {
                ret.Add(new DMRSNVM(O2S(l[0]), O2S(l[1]), O2T(l[2]), O2S(l[3]), O2S(l[4]), O2S(l[5])
                    , O2S(l[6]), O2S(l[7]), O2S(l[8]), O2S(l[9]), O2S(l[10]), O2S(l[11])
                    , O2S(l[12]), O2S(l[13]), O2S(l[14]), O2S(l[15]), O2S(l[16]), O2S(l[17]), O2S(l[18]),O2S(l[19]),O2S(l[20])));
            }

            return ret;
        }

        public static List<DMRSNVM> RetrieveSNWorkFlow(string sn)
        {
            var ret = new List<DMRSNVM>();

            var dict = new Dictionary<string, string>();
            dict.Add("@ContainerName", sn);

            var sql = @"SELECT distinct c.ContainerName as SerialName,pb.ProductName,wb.WorkflowName,ws.WorkflowStepName,jo.MfgOrderName ,hml.MfgDate
		                    FROM InsiteDB.insite.container c with (nolock) 
	                    left join InsiteDB.insite.historyMainline hml with (nolock) on c.containerId = hml.containerId
	                    left join InsiteDB.insite.MoveHistory mv with (nolock) on mv.HistoryMainlineId= hml.HistoryMainlineId
	                    left join InsiteDB.insite.workflowstep ws(nolock) on  ws.WorkflowStepId  = hml.WorkflowStepId
	                    left join InsiteDB.insite.Workflow (nolock) wf on ws.WorkflowId = wf.WorkflowId
	                    left join InsiteDB.insite.WorkflowBase (nolock) wb on wf.WorkflowBaseId = wb.WorkflowBaseId
	                    left join InsiteDB.insite.MfgOrder (nolock) jo on jo.MfgOrderId = mv.MfgOrderId
	                    left join InsiteDB.insite.product p with (nolock) on  hml.productId = p.productId 
                        left join InsiteDB.insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId 
	                    where c.ContainerName = @ContainerName and mv.MoveInTime is not null   order by hml.MfgDate asc";
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql, dict);
            foreach (var l in dbret)
            {
                var tempvm = new DMRSNVM();
                tempvm.SN = O2S(l[0]);
                tempvm.PN = O2S(l[1]);
                tempvm.WorkFlow = O2S(l[2]);
                tempvm.WorkFlowStep = O2S(l[3]);
                tempvm.JO = O2S(l[4]);
                tempvm.DMRDate = O2T(l[5]);
                ret.Add(tempvm);
            }

            sql = @"select distinct c.ContainerName,pb.ProductName,wb.WorkflowName CRTWFName,ws.WorkflowStepName CRTWFStepName, jo.MfgOrderName,c.LastActivityDateGMT
                        from InsiteDB.insite.Container (nolock) c
                    left join InsiteDB.insite.MfgOrder (nolock) jo on c.MfgOrderId = jo.MfgOrderId
                    left join InsiteDB.insite.CurrentStatus (nolock) cs on c.CurrentStatusId = cs.CurrentStatusId
                    left join InsiteDB.insite.WorkflowStep (nolock) ws on cs.WorkflowStepId = ws.WorkflowStepId
                    left join InsiteDB.insite.Workflow (nolock) wf on ws.WorkflowId = wf.WorkflowId
                    left join InsiteDB.insite.WorkflowBase (nolock) wb on wf.WorkflowBaseId = wb.WorkflowBaseId
                    left join InsiteDB.insite.product p with (nolock) on  c.ProductId = p.productId 
                    left join InsiteDB.insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId 
                    where c.ContainerName  = @ContainerName";
            dbret = DBUtility.ExeRealMESSqlWithRes(sql, dict);
            foreach (var l in dbret)
            {
                var tempvm = new DMRSNVM();
                tempvm.SN = O2S(l[0]);
                tempvm.PN = O2S(l[1]);
                tempvm.WorkFlow = O2S(l[2]);
                tempvm.WorkFlowStep = O2S(l[3]);
                tempvm.JO = O2S(l[4]);
                tempvm.DMRDate = O2T(l[5]);

                if (string.Compare(ret[ret.Count - 1].WorkFlowStep, tempvm.WorkFlowStep, true) != 0)
                {
                    ret.Add(tempvm);
                }
            }

            return ret;
        }

        public DMRSNVM()
        {
            DMRID = "";
            DMRProdLine = "";
            DMRDate = "";
            DMRCreater = "";
            SN = "";
            SNFailure = "";
            SNStatus = "";
            JO = "";
            PN = "";
            WorkFlow = "";
            WorkFlowStep = "";
            DMRFileURL = "";
            ModuleCount = 0;

            DMRStartStep = "";
            DMRStartTime = "";
            DMRStoreStep = "";
            DMRStoreTime = "";
            DMRRepairStep = "";
            DMRRepairTime = "";
            DMRReturnStep = "";
            DMRReturnTime = "";

            DMROAStep = "";
            DMROAStatus = "";
        }

        public DMRSNVM(string did,string dprod,string ddate,string dcrt,string sn,string snfail,string snstat
            ,string jo,string pn,string wf,string step, string srtstep,string srttime
            ,string dsrstep,string dsrtime,string drpstep,string drptime,string dretstep,string drettime,string oastep,string oastat)
        {
            DMRID = did;
            DMRProdLine = dprod;
            DMRDate = ddate;
            DMRCreater = dcrt;
            SN = sn;
            SNFailure = snfail;
            SNStatus = snstat;
            JO = jo;
            PN = pn;
            WorkFlow = wf;
            WorkFlowStep = step;

            DMRFileURL = "";
            ModuleCount = 0;

            DMRStartStep = srtstep;
            DMRStartTime = srttime;
            DMRStoreStep = dsrstep;
            DMRStoreTime = dsrtime;
            DMRRepairStep = drpstep;
            DMRRepairTime = drptime;
            DMRReturnStep = dretstep;
            DMRReturnTime = drettime;

            DMROAStep = oastep;
            DMROAStatus = oastat;

            if (string.IsNullOrEmpty(DMRRepairStep)
                && !string.IsNullOrEmpty(DMRStoreStep)
                && !(WorkFlowStep.ToUpper().Contains("EQ") && WorkFlowStep.ToUpper().Contains("INVENTORY")))
            {
                DMRRepairStep = WorkFlowStep;
                DMRRepairTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private string SpendTime(string s, string e)
        {
            if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(e))
            {
                try
                {
                    if (DateTime.Parse(s) > DateTime.Parse(e))
                    { return string.Empty; }

                    return (DateTime.Parse(e) - DateTime.Parse(s)).Days.ToString();
                }
                catch (Exception ex) { }
            }
            return string.Empty;
        }

        public string DMRID { set; get; }
        public string DMRProdLine { set; get; }
        public string DMRDate { set; get; }
        public string DMRCreater { set; get; }
        public string SN { set; get; }
        public string SNStatus { set; get; }
        public string SNFailure { set; get; }
        public string JO { set; get; }
        public string PN { set; get; }
        public string WorkFlow { set; get; }
        public string WorkFlowStep { set; get; }
        public string DMRFileURL { set; get; }

        public string DMRStartStep { set; get; }
        public string DMRStartTime { set; get; }
        public string DMRStoreStep { set; get; }
        public string DMRStoreTime { set; get; }
        public string DMRRepairStep { set; get; }
        public string DMRRepairTime { set; get; }
        public string DMRReturnStep { set; get; }
        public string DMRReturnTime { set; get; }

        public int ModuleCount { set; get; }

        public string DMROAStep { set; get; }
        public string DMROAStatus { set; get; }

        public string OASpend {
            get {
                return SpendTime(DMRDate, DMRStoreTime);
            }           
        }

        public string StoreSpend
        {
            get
            {
                return SpendTime(DMRStoreTime, DMRRepairTime);
            }
        }

        public string RepairSpend
        {
            get
            {
                return SpendTime(DMRRepairTime, DMRReturnTime);
            }
        }

        public string TotleDRMSpend {
            get {
                return SpendTime(DMRDate, DMRReturnTime);
            }
        }

    }
}