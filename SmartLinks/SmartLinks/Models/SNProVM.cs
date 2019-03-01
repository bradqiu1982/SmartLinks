using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class SNProVM
    {

        public SNProVM()
        {
            SN = "";
            PN = "";
            WKFlow = "";
            Time = "";
            Failure = "";
            Tester = "";
        }

        public SNProVM(string sn,string pn,string wf,string t)
        {
            SN = sn;
            PN = pn;
            WKFlow = wf;
            Time = t;
            Failure = "";
            Tester = "";
        }


        public static List<SNProVM> RetrieveWorkFlowData(List<string> SNList)
        {
            var ret = new List<SNProVM>();

            var sncond = "('" + string.Join("','", SNList) + "')";
            var sql = @"SELECT distinct c.ContainerName as SerialName,pb.productname,ws.WorkflowStepName ,hml.MfgDate
                         FROM InsiteDB.insite.container c with (nolock) 
                        left join InsiteDB.insite.historyMainline hml with (nolock) on c.containerId = hml.containerId
                        left join InsiteDB.insite.MoveHistory mv with (nolock) on mv.HistoryMainlineId= hml.HistoryMainlineId
                        left join InsiteDB.insite.workflowstep ws(nolock) on  ws.WorkflowStepId  = hml.WorkflowStepId
                        left join InsiteDB.insite.product p with (nolock) on  c.productId = p.productId 
                        left join InsiteDB.insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId 
                        where c.ContainerName in <sncond> and mv.MoveInTime is not null   order by SerialName,hml.MfgDate asc";

            sql = sql.Replace("<sncond>", sncond);

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                try
                {
                    ret.Add(new SNProVM(Convert.ToString(line[0]), Convert.ToString(line[1])
                        , Convert.ToString(line[2]), Convert.ToDateTime(line[3]).ToString("yyyy-MM-dd HH:mm:ss")));
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public static List<SNProVM> RetrieveTestFlowData(List<string> SNList)
        {
            var ret = new List<SNProVM>();

            var sncond = "('" + string.Join("','", SNList) + "')";
            var sql = @"SELECT distinct [ModuleSN],[PN],[WhichTest],[TestTimeStamp],[ErrAbbr],[TestStation]  FROM [BSSupport].[dbo].[ModuleTestData] 
                        WHERE ModuleSN in <sncond> order by ModuleSN,TestTimeStamp asc";

            sql = sql.Replace("<sncond>", sncond);

            var dbret = DBUtility.ExeBSSqlWithRes(sql);
            foreach (var line in dbret)
            {
                try
                {
                    var tempvm = new SNProVM(Convert.ToString(line[0]), Convert.ToString(line[1])
                        , Convert.ToString(line[2]), Convert.ToDateTime(line[3]).ToString("yyyy-MM-dd HH:mm:ss"));
                    tempvm.Failure = Convert.ToString(line[4]);
                    tempvm.Tester = Convert.ToString(line[5]);
                    ret.Add(tempvm);
                }
                catch (Exception ex) { }
            }

            return ret;
        }

        public string SN { set; get; }
        public string PN { set; get; }
        public string WKFlow { set; get; }
        public string Time { set; get; }
        public string Failure { set; get; }
        public string Tester { set; get; }
    }
}