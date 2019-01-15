using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class ModuleAssemblyVM
    {

        private static string Convert2Str(object obj)
        {
            if (obj == null)
            { return string.Empty; }

            try
            {
                return Convert.ToString(obj);
            }
            catch (Exception ex) { return string.Empty; }
        }

        private static string Convert2Date(object obj)
        {
            if (obj == null)
            { return string.Empty; }

            try
            {
                return Convert.ToDateTime(obj).ToString("yyyy-MM-dd");
            }
            catch (Exception ex) { return string.Empty; }
        }

        public static List<ModuleAssemblyVM> RetrieveData(List<string> sns)
        {
            var ret = new List<ModuleAssemblyVM>();
            var sncond = "('" + string.Join("','", sns) + "')";
            //var sql = @"select ToContainer,ToProductName,ToPNDescription,FromContainer,FromProductName,FromPNDescription
            //            , ToWorkflowStepId,IssueDate FROM[PDMS].[dbo].[ComponentIssueSummary] where ToContainer in <sncond> order by ToContainer,IssueDate desc";

            var sql = @"select  co.ContainerName,tpb.ProductName,tp.Description,fc.ContainerName,pb.ProductName, p.Description,ws.WorkflowStepName,hml.MfgDate from   insitedb.insite.ComponentIssueHistory cih with(nolock)
                    inner join insitedb.insite.Historymainline hml  with(nolock) on hml.HistoryMainlineId = cih.historymainlineid  
                    inner join insitedb.insite.IssueHistoryDetail  ihd with(nolock) on ihd.ComponentIssueHistoryId= cih.ComponentIssueHistoryId
                    inner join insitedb.insite.IssueActualsHistory iah with(nolock) on iah.IssueHistoryDetailId=ihd.IssueHistoryDetailId
                    inner join insitedb.insite.Product p with(nolock) on p.ProductId  = ihd.ProductId
                    inner join insitedb.insite.ProductBase pb with(nolock) on pb.ProductBaseId  = p.ProductBaseId
                    inner join insitedb.insite.WorkflowStep ws with(nolock) on ws.WorkflowStepId = hml.WorkflowStepId
                    inner join  InsiteDB.insite.container co (nolock) on co.containerid=hml.HistoryId
                    inner join  InsiteDB.insite.container fc (nolock) on fc.ContainerId=iah.FromContainerId
                    inner join insitedb.insite.Product tp with(nolock) on tp.ProductId  = co.ProductId
                    inner join insitedb.insite.ProductBase tpb with(nolock) on tp.ProductBaseId  = tpb.ProductBaseId
                    where co.ContainerName in <sncond> order by co.ContainerName,hml.MfgDate desc";
            sql = sql.Replace("<sncond>", sncond);

            //var stepiddict = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new ModuleAssemblyVM();
                tempvm.SN = Convert2Str(line[0]);
                tempvm.PN = Convert2Str(line[1]);
                tempvm.Desc = Convert2Str(line[2]);
                tempvm.AssemblySN = Convert2Str(line[3]);
                tempvm.AssemblyPN = Convert2Str(line[4]);
                tempvm.AssemblyDesc = Convert2Str(line[5]);
                tempvm.StepName = Convert2Str(line[6]);
                tempvm.IssueDate = Convert2Date(line[7]);
                ret.Add(tempvm);

                //if (!string.IsNullOrEmpty(tempvm.StepID) && !stepiddict.ContainsKey(tempvm.StepID))
                //{ stepiddict.Add(tempvm.StepID,true); }
            }

            //var stepnamedict = new Dictionary<string, string>();
            //if (stepiddict.Count > 0)
            //{
            //    var idlist = stepiddict.Keys.ToList();
            //    var idcond = "('" + string.Join("','", idlist) + "')";
            //    sql = "select WorkflowStepId,WorkflowStepName from InsiteDB.insite.WorkflowStep where WorkflowStepId in <idcond>";
            //    sql = sql.Replace("<idcond>", idcond);
            //    dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            //    foreach (var line in dbret)
            //    {
            //        var id = Convert2Str(line[0]);
            //        var name = Convert2Str(line[1]);
            //        if (!stepnamedict.ContainsKey(id))
            //        { stepnamedict.Add(id, name); }
            //    }
            //}

            //foreach (var item in ret)
            //{
            //    if (stepnamedict.ContainsKey(item.StepID))
            //    {
            //        item.StepName = stepnamedict[item.StepID];
            //    }
            //}

            return ret;
        }
    
        public ModuleAssemblyVM()
        {
            SN="";
            PN="";
            Desc="";
            AssemblySN="";
            AssemblyPN="";
            AssemblyDesc="";
            StepID="";
            StepName="";
            IssueDate="";
        }

        public string SN { set; get; }
        public string PN { set; get; }
        public string Desc { set; get; }
        public string AssemblySN { set; get; }
        public string AssemblyPN { set; get; }
        public string AssemblyDesc { set; get; }
        public string StepID { set; get; }
        public string StepName { set; get; }
        public string IssueDate { set; get; }

    }
}