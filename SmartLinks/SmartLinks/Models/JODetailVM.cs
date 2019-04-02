using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class JODetailVM
    {
        public static List<JODetailVM> LoadData(List<string> jolist)
        {
            var ret = new List<JODetailVM>();

            var sb = new System.Text.StringBuilder();
            foreach (var jo in jolist)
            {
                sb.Append(" or jo.MfgOrderName like '%"+jo+"%'");
            }
            var jocond = sb.ToString().Substring(3);

            var sql = @"select distinct c.ContainerName
                    ,case when c.CustomerSerialNum is not null then c.CustomerSerialNum else '' end CustomerSerialNum
                    ,jo.MfgOrderName
                    ,case when c.DateCode is not null then c.DateCode else '' end LotNum
                    ,case when pc.ContainerName is not null then pc.ContainerName else '' end PackageNum
                    ,pb.ProductName,wb.WorkflowName CRTWFName,wf.WorkflowRevision CRTWFRev,ws.WorkflowStepName CRTWFStepName
                    , CASE WHEN (c.STATUS IS NULL) THEN 'NONEXIST' 
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
	                 END SNStatus
                    ,case when hr.HoldReasonName is not null then hr.HoldReasonName else '' end HoldReason
                    ,awb.WorkflowName OrgWFName,awf.WorkflowRevision OrgWFRev,aws.WorkflowStepName OrgWFStepName,hml.MfgDate
                     ,case when lr.LossReasonName is not null then lr.LossReasonName else '' end  ScrapReason
                     ,pd.Description
                     from InsiteDB.insite.Container (nolock) c
                    left join InsiteDB.insite.MfgOrder (nolock) jo on c.MfgOrderId = jo.MfgOrderId
                    left join InsiteDB.insite.CurrentStatus (nolock) cs on c.CurrentStatusId = cs.CurrentStatusId
                    left join InsiteDB.insite.WorkflowStep (nolock) ws on cs.WorkflowStepId = ws.WorkflowStepId
                    left join InsiteDB.insite.Workflow (nolock) wf on ws.WorkflowId = wf.WorkflowId
                    left join InsiteDB.insite.WorkflowBase (nolock) wb on wf.WorkflowBaseId = wb.WorkflowBaseId
                    left join InsiteDB.insite.Container (nolock) pc on pc.ContainerId = c.ParentContainerId
                    left join [InsiteDB].[insite].[Product]  (nolock) pd on pd.ProductId = c.ProductId
                    left join [InsiteDB].[insite].[ProductBase]  (nolock) pb on pb.ProductBaseId = pd.ProductBaseId
                    left join InsiteDB.insite.HoldReason (nolock) hr on hr.HoldReasonId = c.HoldReasonId
                    left join (
                    select hml.ContainerId,MAX(hml.MfgDate) as MAXTIME from InsiteDB.insite.Container (nolock) c
                    left join InsiteDB.insite.MfgOrder (nolock) jo on c.MfgOrderId = jo.MfgOrderId
                    left join InsiteDB.insite.HistoryMainline (nolock) hml on c.ContainerId = hml.ContainerId
                    where (<jocond>) group by hml.ContainerId
                    ) LatestHML  on LatestHML.ContainerId = c.ContainerId
                    left join InsiteDB.insite.historyMainline hml with (nolock) on  hml.ContainerId = LatestHML.ContainerId and hml.MfgDate = LatestHML.MAXTIME
                    left join InsiteDB.insite.workflowstep aws(nolock) on  aws.WorkflowStepId  = hml.WorkflowStepId
                    left join InsiteDB.insite.Workflow (nolock) awf on aws.WorkflowId = awf.WorkflowId
                    left join InsiteDB.insite.WorkflowBase (nolock) awb on awf.WorkflowBaseId = awb.WorkflowBaseId
                    left join InsiteDB.insite.ScrapHistoryDetails (nolock) shd on shd.ContainerId= c.ContainerId
                    left join [InsiteDB].[insite].[LossReason] (nolock) lr on lr.LossReasonId = shd.ReasonCodeId
                    where (<jocond>) and Len(c.ContainerName) = 7  order by jo.MfgOrderName,c.ContainerName";
            sql = sql.Replace("<jocond>", jocond);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var l in dbret)
            {
                try
                {
                    ret.Add(new JODetailVM(ts(l[0]),ts(l[1]), ts(l[2]), ts(l[3]), ts(l[4]), ts(l[5]), ts(l[6])
                            , ts(l[7]), ts(l[8]), ts(l[9]), ts(l[10]), ts(l[11])
                            , ts(l[12]), ts(l[13]), Convert.ToDateTime(l[14]).ToString("yyyy-MM-dd HH:mm:ss"), ts(l[15]), ts(l[16])));
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static List<JODetailVM> LoadSNData(List<string> snlist)
        {
            var ret = new List<JODetailVM>();

            var sncond = "('" + string.Join("','", snlist) + "')";

            var sql = @"select distinct c.ContainerName
                    ,case when c.CustomerSerialNum is not null then c.CustomerSerialNum else '' end CustomerSerialNum
                    ,jo.MfgOrderName
                    ,case when c.DateCode is not null then c.DateCode else '' end LotNum
                    ,case when pc.ContainerName is not null then pc.ContainerName else '' end PackageNum
                    ,pb.ProductName,wb.WorkflowName CRTWFName,wf.WorkflowRevision CRTWFRev,ws.WorkflowStepName CRTWFStepName
                    , CASE WHEN (c.STATUS IS NULL) THEN 'NONEXIST' 
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
	                 END SNStatus
                    ,case when hr.HoldReasonName is not null then hr.HoldReasonName else '' end HoldReason
                    ,awb.WorkflowName OrgWFName,awf.WorkflowRevision OrgWFRev,aws.WorkflowStepName OrgWFStepName,hml.MfgDate
                     ,case when lr.LossReasonName is not null then lr.LossReasonName else '' end  ScrapReason
                     ,pd.Description
                     from InsiteDB.insite.Container (nolock) c
                    left join InsiteDB.insite.MfgOrder (nolock) jo on c.MfgOrderId = jo.MfgOrderId
                    left join InsiteDB.insite.CurrentStatus (nolock) cs on c.CurrentStatusId = cs.CurrentStatusId
                    left join InsiteDB.insite.WorkflowStep (nolock) ws on cs.WorkflowStepId = ws.WorkflowStepId
                    left join InsiteDB.insite.Workflow (nolock) wf on ws.WorkflowId = wf.WorkflowId
                    left join InsiteDB.insite.WorkflowBase (nolock) wb on wf.WorkflowBaseId = wb.WorkflowBaseId
                    left join InsiteDB.insite.Container (nolock) pc on pc.ContainerId = c.ParentContainerId
                    left join [InsiteDB].[insite].[Product]  (nolock) pd on pd.ProductId = c.ProductId
                    left join [InsiteDB].[insite].[ProductBase]  (nolock) pb on pb.ProductBaseId = pd.ProductBaseId
                    left join InsiteDB.insite.HoldReason (nolock) hr on hr.HoldReasonId = c.HoldReasonId
                    left join (
                    select hml.ContainerId,MAX(hml.MfgDate) as MAXTIME from InsiteDB.insite.Container (nolock) c
                    left join InsiteDB.insite.MfgOrder (nolock) jo on c.MfgOrderId = jo.MfgOrderId
                    left join InsiteDB.insite.HistoryMainline (nolock) hml on c.ContainerId = hml.ContainerId
                    where c.ContainerName in <sncond> group by hml.ContainerId
                    ) LatestHML  on LatestHML.ContainerId = c.ContainerId
                    left join InsiteDB.insite.historyMainline hml with (nolock) on  hml.ContainerId = LatestHML.ContainerId and hml.MfgDate = LatestHML.MAXTIME
                    left join InsiteDB.insite.workflowstep aws(nolock) on  aws.WorkflowStepId  = hml.WorkflowStepId
                    left join InsiteDB.insite.Workflow (nolock) awf on aws.WorkflowId = awf.WorkflowId
                    left join InsiteDB.insite.WorkflowBase (nolock) awb on awf.WorkflowBaseId = awb.WorkflowBaseId
                    left join InsiteDB.insite.ScrapHistoryDetails (nolock) shd on shd.ContainerId= c.ContainerId
                    left join [InsiteDB].[insite].[LossReason] (nolock) lr on lr.LossReasonId = shd.ReasonCodeId
                    where c.ContainerName in <sncond> and Len(c.ContainerName) = 7  order by jo.MfgOrderName,c.ContainerName";
            sql = sql.Replace("<sncond>", sncond);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var l in dbret)
            {
                try
                {
                    ret.Add(new JODetailVM(ts(l[0]), ts(l[1]), ts(l[2]), ts(l[3]), ts(l[4]), ts(l[5]), ts(l[6])
                            , ts(l[7]), ts(l[8]), ts(l[9]), ts(l[10]), ts(l[11])
                            , ts(l[12]), ts(l[13]), Convert.ToDateTime(l[14]).ToString("yyyy-MM-dd HH:mm:ss"), ts(l[15]), ts(l[16])));
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        private static string ts(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            else
            {
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception ex) {
                    return string.Empty;
                }
            }
        }

        public JODetailVM()
        {
            ContainerName = "";
            CustomerSerialNum = "";
            MfgOrderName = "";
            LotNum = "";
            PackageNum = "";
            ProductName = "";
            CRTWFName = "";
            CRTWFRev = "";
            CRTWFStepName = "";
            SNStatus = "";
            HoldReason = "";
            OrgWFName = "";
            OrgWFRev = "";
            OrgWFStepName = "";
            MfgDate = "";
            ScrapReason = "";
            Description = "";
        }


        public JODetailVM(string sn,string csn,string jo,string lotnum,string packnum,string pn
            ,string crtwfn,string crtwfv,string crtstepname,string snstatus,string holdreason
            ,string orgwfn,string orgwfv,string orgstepname,string latedate,string scrapreason,string pdes)
        {
            ContainerName = sn;
            CustomerSerialNum = csn;
            MfgOrderName = jo;
            LotNum = lotnum;
            PackageNum = packnum;
            ProductName = pn;
            CRTWFName = crtwfn;
            CRTWFRev = crtwfv;
            CRTWFStepName = crtstepname;
            SNStatus = snstatus;
            HoldReason = holdreason;
            OrgWFName = orgwfn;
            OrgWFRev = orgwfv;
            OrgWFStepName = orgstepname;
            MfgDate = latedate;
            ScrapReason = scrapreason;
            Description = pdes.Split(new string[] { "*"," "},StringSplitOptions.RemoveEmptyEntries)[0];

        }

        public string ContainerName { set; get; }
        public string CustomerSerialNum { set; get; }
        public string MfgOrderName { set; get; }
        public string LotNum { set; get; }
        public string PackageNum { set; get; }
        public string ProductName { set; get; }
        public string CRTWFName { set; get; }
        public string CRTWFRev { set; get; }
        public string CRTWFStepName { set; get; }
        public string SNStatus { set; get; }
        public string HoldReason { set; get; }
        public string OrgWFName { set; get; }
        public string OrgWFRev { set; get; }
        public string OrgWFStepName { set; get; }
        public string MfgDate { set; get; }
        public string ScrapReason { set; get; }
        public string Description { set; get; }

    }
}