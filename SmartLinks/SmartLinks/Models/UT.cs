using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class UT
    {
        public static List<List<T>> SplitList<T>(List<T> me, int size = 5000)
        {
            var list = new List<List<T>>();
            try
            {
                for (int i = 0; i < me.Count; i += size) {
                    list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
                }
            }
            catch (Exception ex) { }
            return list;
        }

        public static string O2S(object obj)
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

        public static string O2T(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }


        public static double O2D(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToDouble(obj);
                }
                catch (Exception ex) { return 0.0; }
            }
            return 0.0;
        }

        public static int O2I(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch (Exception ex) { return 0; }
            }
            return 0;
        }

        public static Dictionary<string, string> GetWaferFromSN(List<string> snlist)
        {
            var ret = new Dictionary<string, string>();

            var sncond = "('" + string.Join("','", snlist) + "')";

            var  sql = @"SELECT distinct c.ContainerName as SerialName,isnull(dc.[ParamValueString],'') as WaferLot
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
                        WHERE dc.parametername='Trace_ID' and p.description like '%VCSEL%' and dc.[ParamValueString] like '%-%'and c.containername in <SNCOND> ";

            sql = sql.Replace("<SNCOND>", sncond);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]).ToUpper();
                var wafer = UT.O2S(line[1]).Replace("A","");
                if (wafer.Length == 12)
                { wafer = wafer.Substring(0, 9); }

                if (!ret.ContainsKey(sn))
                { ret.Add(sn, wafer); }
            }
            return ret;
        }


    }
}