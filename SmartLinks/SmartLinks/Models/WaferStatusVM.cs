using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class WaferStatusVM
    {
        public static List<WaferStatusVM> RetrieveData(List<string> waferlist)
        {
            var ret = new List<WaferStatusVM>();
            if (waferlist.Count == 0)
            { return ret; }

            var wafercond = "";
            foreach (var w in waferlist)
            {
                wafercond = wafercond + " or dc.[ParamValueString] like '" + w + "%'";
            }
            wafercond = wafercond.Substring(3);

            var sql = @"SELECT left(dc.[ParamValueString],9) WaferNum, c.ContainerName,ws.WorkflowStepName,pbb.ProductName ProductPN,pf.ProductFamilyName,hml.MfgDate
                        FROM InsiteDB.insite.container c with (nolock) 
                        left join InsiteDB.insite.currentStatus cs (nolock) on c.currentStatusId = cs.currentStatusId 
                        left join InsiteDB.insite.workflowstep ws(nolock) on  cs.WorkflowStepId = ws.WorkflowStepId 
                        left join InsiteDB.insite.componentRemoveHistory crh with (nolock) on crh.historyId = c.containerId 
                        left join InsiteDB.insite.removeHistoryDetail rhd on rhd.componentRemoveHistoryId = crh.componentRemoveHistoryId 
                        left join InsiteDB.insite.starthistorydetail  shd(nolock) on c.containerid=shd.containerId and shd.historyId <> shd.containerId 
                        left join InsiteDB.insite.container co (nolock) on co.containerid=shd.historyId 
                        left join InsiteDB.insite.historyMainline hml with (nolock) on c.containerId = hml.containerId 
                        left join InsiteDB.insite.componentIssueHistory cih with (nolock) on  hml.historyMainlineId=cih.historyMainlineId 
                        left join InsiteDB.insite.issueHistoryDetail ihd with (nolock) on cih.componentIssueHistoryId = ihd.componentIssueHistoryId 
                        left join InsiteDB.insite.issueActualsHistory iah with (nolock) on  ihd.issueHistoryDetailId = iah.issueHistoryDetailId 
                        left join InsiteDB.insite.RemoveHistoryDetail rem with (nolock) on iah.IssueActualsHistoryId = rem.IssueActualsHistoryId 
                        left join InsiteDB.insite.RemovalReason re with (nolock) on rem.RemovalReasonId = re.RemovalReasonId 
                        left join InsiteDB.insite.container cFrom with (nolock) on iah.fromContainerId = cFrom.containerId 
                        left join InsiteDB.insite.product p with (nolock) on  cFrom.productId = p.productId 
                        left join InsiteDB.insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId 
                        left join InsiteDB.insite.historyMainline hmll with (nolock)on cFrom.OriginalcontainerId=hmll.historyid 
                        left join InsiteDB.insite.product pp with (nolock) on c.productid=pp.productid 
                        left join InsiteDB.insite.productfamily pf (nolock) on  pp.productFamilyId = pf.productFamilyId 
                        left join InsiteDB.insite.productbase pbb with (nolock) on pp.productbaseid=pbb.productbaseid 
                        left join InsiteDB.insite.dc_AOC_ManualInspection dc (nolock) on hmll.[HistoryMainlineId]=dc.[HistoryMainlineId] 
                        WHERE dc.parametername='Trace_ID' and p.description like '%VCSEL%' and (<wafercond>) 
	                        and Len(c.ContainerName) = 7  order by dc.[ParamValueString],hml.MfgDate asc";
            sql = sql.Replace("<wafercond>", wafercond);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                try
                {
                    var tempvm = new WaferStatusVM();
                    tempvm.WaferNum = Convert.ToString(line[0]);
                    tempvm.SN = Convert.ToString(line[1]);
                    tempvm.WorkFlowStep = Convert.ToString(line[2]);
                    tempvm.PN = Convert.ToString(line[3]);
                    tempvm.ProductFamily = Convert.ToString(line[4]);
                    tempvm.UpdateTime = Convert.ToDateTime(line[5]).ToString("yyyy-MM-dd HH:mm:ss");
                    ret.Add(tempvm);
                }
                catch (Exception ex) { }
            }
            return ret;
        }

        public WaferStatusVM()
        {
            WaferNum = "";
            SN = "";
            WorkFlowStep = "";
            PN = "";
            ProductFamily = "";
            UpdateTime = "";
        }

        public string WaferNum { set; get; }
        public string SN { set; get; }
        public string WorkFlowStep { set; get; }
        public string PN { set; get; }
        public string ProductFamily { set; get; }
        public string UpdateTime { set; get; }
    }
}