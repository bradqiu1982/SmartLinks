using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SmartLinks.Models
{
    public class WAFERSTATUS
    {
        public static string GOOD = "GOOD";
        public static string NG = "NG";
        public static string NA = "NA";
    }

    public class WaferTableItem
    {
        public WaferTableItem()
        {
            SN = "";
            DateCode = "";
            WaferNum = "";
            PN = "";
            Status = "";
            AppendInfo = "";
        }

        public string SN { set; get; }
        public string DateCode { set; get; }
        public string WaferNum { set; get; }
        public string PN { set; get; }
        public string Status { set; get; }
        public string AppendInfo { set; get; }
    }

    public class WaferPackVM
    {
        public static List<WaferTableItem> RetrieveSNByDateCode(List<string> datecodelist,Dictionary<string,string> appenddict)
        {
            var ret = new List<WaferTableItem>();
            var datecond = " ('";
            foreach (var item in datecodelist)
            {
                datecond = datecond + item + "','";
            }
            datecond = datecond.Substring(0, datecond.Length - 2);
            datecond = datecond + ") ";

            var sql = "select ContainerName,DateCode,CustomerSerialNum,ContainerId FROM [InsiteDB].[insite].[Container] where DateCode in <datecond> or CustomerSerialNum in <datecond>";
            sql = sql.Replace("<datecond>", datecond);

            var excludsndict = new Dictionary<string, bool>();
            var pkgdict = new Dictionary<string, string>();

            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret) {
                try {

                    var sn = Convert.ToString(line[0]);
                    if (sn.Length > 7 
                        && !line[1].Equals(null) 
                        && !pkgdict.ContainsKey(sn)) {
                        pkgdict.Add(Convert.ToString(line[3]), Convert.ToString(line[1]));
                    }
                    else
                    {
                            var tempvm = new WaferTableItem();
                            tempvm.SN = Convert.ToString(line[0]);

                            if (!excludsndict.ContainsKey(tempvm.SN))
                            {
                                excludsndict.Add(tempvm.SN, true);

                                var appendinfo = "";
                                if (line[1].Equals(null))
                                {
                                    tempvm.DateCode = "";
                                }
                                else
                                {
                                    tempvm.DateCode = Convert.ToString(line[1]);
                                    appendinfo = tempvm.DateCode;
                                }
                                if (!line[2].Equals(null))
                                {
                                    appendinfo = Convert.ToString(line[2]);
                                }
                
                                if (!string.IsNullOrEmpty(tempvm.SN))
                                {
                                    if (!appenddict.ContainsKey(tempvm.SN)) {
                                        appenddict.Add(tempvm.SN, appendinfo);
                                    }
                                    ret.Add(tempvm);
                                }
                            }//check sn
                    }
                } catch (Exception ex) { }
            }


            if (pkgdict.Count > 0)
            {
                try
                {
                    datecond = " ('";
                    foreach (var item in pkgdict)
                    {
                        datecond = datecond + item.Key + "','";
                    }
                    datecond = datecond.Substring(0, datecond.Length - 2);
                    datecond = datecond + ") ";

                    sql = "select ContainerName,ParentContainerId FROM [InsiteDB].[insite].[Container] where ParentContainerId in <datecond>";
                    sql = sql.Replace("<datecond>", datecond);
                    dbret = DBUtility.ExeRealMESSqlWithRes(sql, null);
                    foreach (var line in dbret)
                    {
                        var sn = Convert.ToString(line[0]);
                        var pid = Convert.ToString(line[1]);
                        if (sn.Length <= 7 && !excludsndict.ContainsKey(sn) && pkgdict.ContainsKey(pid))
                        {
                            excludsndict.Add(sn, true);
                            var tempvm = new WaferTableItem();
                            tempvm.SN = Convert.ToString(line[0]);
                            tempvm.DateCode = pkgdict[pid];
                            ret.Add(tempvm);
                        }//end if
                    }//end foreach
                } catch (Exception ex) { }
            }//end if

            return ret;
        }

        public static List<WaferTableItem> SolveCableSN(List<WaferTableItem> desdata,Dictionary<string, bool> cablesndict)
        {
            var excludsndict = new Dictionary<string, bool>();

            var ret = new List<WaferTableItem>();

            var tempres = new List<WaferTableItem>();

            StringBuilder sb = new StringBuilder(10 * (desdata.Count + 5));
            sb.Append("('");
            foreach (var item in desdata)
            {
                sb.Append(item.SN + "','");
            }
            var tempstr = sb.ToString();
            var sncond = tempstr.Substring(0, tempstr.Length - 2) + ")";

            var sql = @"select a.ToContainer,a.FromContainer from [PDMS].[dbo].[ComponentIssueSummary] a 
                  inner join (SELECT COUNT(*) as tcnt,ToContainer FROM [PDMS].[dbo].[ComponentIssueSummary] where ToContainer  in <sncond> and LEN(FromContainer) = 7 group by ToContainer) b on a.ToContainer = b.ToContainer
                  where b.tcnt >= 2 and LEN(a.FromContainer) = 7 and a.ToContainer in <sncond> order by a.ToContainer";
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeMESReportSqlWithRes(sql);
            foreach (var line in dbret)
            {
                
                var tempvm = new WaferTableItem();
                tempvm.SN = Convert.ToString(line[1]);
                tempvm.DateCode = Convert.ToString(line[0]);
                if (!excludsndict.ContainsKey(tempvm.SN))
                {
                    excludsndict.Add(tempvm.SN, true);

                    tempres.Add(tempvm);

                    if (!cablesndict.ContainsKey(tempvm.SN)) {
                        cablesndict.Add(tempvm.SN, true);
                    }
                    if (!cablesndict.ContainsKey(tempvm.DateCode))
                    {
                        cablesndict.Add(tempvm.DateCode, true);
                    }
                }//check sn

            }

            foreach (var item in desdata)
            {
                if (!cablesndict.ContainsKey(item.SN))
                {
                    ret.Add(item);
                }
            }
            ret.AddRange(tempres);

            return ret;
        }

        public static void RetrieveWaferBySN(List<WaferTableItem> desdata)
        {
            //var sncond = " ('";
            //foreach (var item in desdata)
            //{
            //    sncond = sncond + item.SN + "','";
            //}
            //sncond = sncond.Substring(0, sncond.Length - 2);
            //sncond = sncond + ") ";

            StringBuilder sb = new StringBuilder(10 * (desdata.Count + 5));
            sb.Append("('");
            foreach (var item in desdata)
            {
                sb.Append(item.SN + "','");
            }
            var tempstr = sb.ToString();
            var sncond = tempstr.Substring(0, tempstr.Length - 2) + ")";

            var queryedsndict = new Dictionary<string, bool>();
            var tetmpres = new List<WaferTableItem>();

            var sql = "select ToContainer,Wafer,FromProductName,FromPNDescription from [PDMS].[dbo].[ComponentIssueSummary] where ToContainer in <SNCOND> and Wafer is not null and FromPNDescription is not null order by Wafer";
            sql = sql.Replace("<SNCOND>", sncond);

            var dbret = DBUtility.ExeMESReportSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pndesc = Convert.ToString(line[3]);

                if ((pndesc.ToUpper().Contains("LD,") && pndesc.ToUpper().Contains("VCSEL,"))
                        || (pndesc.ToUpper().Contains("CSG") && (pndesc.ToUpper().Contains("INGAAS VCSEL") || pndesc.ToUpper().Contains("VCSEL ARRAY"))))
                {
                    var tempvm = new WaferTableItem();
                    tempvm.SN = Convert.ToString(line[0]);
                    tempvm.WaferNum = Convert.ToString(line[1]);
                    if (tempvm.WaferNum.Length > 3)
                    {
                        var fidx = tempvm.WaferNum.IndexOf("-");
                        if (fidx != -1 && tempvm.WaferNum.Length >= (fidx + 3))
                        {
                            tempvm.WaferNum = tempvm.WaferNum.Substring(0, fidx + 3);
                        }
                    }
                    tempvm.PN = Convert.ToString(line[2]);
                    tetmpres.Add(tempvm);

                    if (!queryedsndict.ContainsKey(tempvm.SN.Trim().ToUpper()))
                    {
                        queryedsndict.Add(tempvm.SN.Trim().ToUpper(), true);
                    }
                }
            }


            var leftsnlist = new List<string>();
            foreach (var item in desdata)
            {
                if (!string.IsNullOrEmpty(item.SN.Trim())
                    && !queryedsndict.ContainsKey(item.SN.Trim().ToUpper()))
                {
                    leftsnlist.Add(item.SN.Trim());
                }
            }


            if (leftsnlist.Count > 0)
            {
                //sncond = " ('";
                //foreach (var item in leftsnlist)
                //{
                //    sncond = sncond + item + "','";
                //}
                //sncond = sncond.Substring(0, sncond.Length - 2);
                //sncond = sncond + ") ";

                StringBuilder sb1 = new StringBuilder(10 * (leftsnlist.Count + 5));
                sb1.Append("('");
                foreach (var line in leftsnlist)
                {
                    sb1.Append(line + "','");
                }
                var tempstr1 = sb1.ToString();
                var sncond1 = tempstr1.Substring(0, tempstr1.Length - 2) + ")";

                //sql = "SELECT distinct c.ContainerName as SN,dc.[ParamValueString] as wafer,pb.productname MaterialPN FROM insite.container c with(nolock)"
                //    + " inner join insite.currentStatus cs(nolock) on c.currentStatusId = cs.currentStatusId "
                //    + "inner join insite.workflowstep ws(nolock) on cs.WorkflowStepId = ws.WorkflowStepId "
                //    + "inner join insite.historyMainline hml with (nolock) on c.containerId = hml.containerId "
                //    + "inner join insite.componentIssueHistory cih with (nolock) on hml.historyMainlineId=cih.historyMainlineId "
                //    + "inner join insite.issueHistoryDetail ihd with (nolock) on cih.componentIssueHistoryId = ihd.componentIssueHistoryId "
                //    + "inner join insite.issueActualsHistory iah with (nolock) on ihd.issueHistoryDetailId = iah.issueHistoryDetailId "
                //    + "inner join insite.container cFrom with (nolock) on iah.fromContainerId = cFrom.containerId "
                //    + "inner join insite.product p with (nolock) on cFrom.productId = p.productId "
                //    + "inner join insite.productBase pb with (nolock) on p.productBaseId  = pb.productBaseId "
                //    + "inner join insite.historyMainline hmll with (nolock)on cFrom.containerId=hmll.historyid "
                //    + "inner join insite.product pp with (nolock) on c.productid=pp.productid "
                //    + "left outer join insite.productfamily pf (nolock) on pp.productFamilyId = pf.productFamilyId "
                //    + "inner join insite.productbase pbb with (nolock) on pp.productbaseid=pbb.productbaseid "
                //    + "inner join[InsiteDB].[insite].[dc_AOC_ManualInspection] dc (nolock) on hmll.[HistoryMainlineId]= dc.[HistoryMainlineId] "
                //    + "where dc.parametername= 'Trace_ID'  and p.description like '%VCSEL%'  and c.containername in <SNCOND> order by c.ContainerName,pb.productname ";

                sql = @"SELECT distinct c.ContainerName as SerialName,isnull(dc.[ParamValueString],'') as WaferLot,pb.productname MaterialPN ,hml.MfgDate
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
                        WHERE dc.parametername='Trace_ID' and p.description like '%VCSEL%' and dc.[ParamValueString] like '%-%'and c.containername in <SNCOND> order by pb.productname,c.ContainerName,hml.MfgDate DESC";

                sql = sql.Replace("<SNCOND>", sncond1);
                dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var tempvm = new WaferTableItem();
                    tempvm.SN = Convert.ToString(line[0]);
                    tempvm.WaferNum = Convert.ToString(line[1]);
                    if (tempvm.WaferNum.Length > 3)
                    {
                        var fidx = tempvm.WaferNum.IndexOf("-");
                        if (fidx != -1 && tempvm.WaferNum.Length >= (fidx + 3))
                        {
                            tempvm.WaferNum = tempvm.WaferNum.Substring(0, fidx + 3);
                        }
                    }
                    tempvm.PN = Convert.ToString(line[2]);
                    tetmpres.Add(tempvm);
                }
            }

            foreach (var des in desdata)
            {
                foreach (var src in tetmpres)
                {
                    if (string.Compare(des.SN, src.SN,true) == 0)
                    {
                        des.WaferNum = src.WaferNum;
                        des.PN = src.PN;
                        break;
                    }
                }//end foreach
            }//end foreach
        }

        public static void UpdateNGWafer(List<string> waferlist)
        {
            var sql = "delete from NGWafer";
            DBUtility.ExeLocalSqlNoRes(sql);
            foreach (var item in waferlist)
            {
                sql = "insert into NGWafer(WaferNo) values(N'<WaferNo>')";
                sql = sql.Replace("<WaferNo>", item.Replace("'", "").Trim());
                DBUtility.ExeLocalSqlNoRes(sql);
            }
        }

        public static List<string> RetrieveNGWafer()
        {
            var ret = new List<string>();
            var sql = "select WaferNo from NGWafer order by WaferNo";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }
            return ret;
        }

        public static Dictionary<string, bool> RetrieveNGWaferDict()
        {
            var ret = new Dictionary<string, bool>();
            var waferlist = RetrieveNGWafer();
            foreach (var item in waferlist)
            {
                if (!ret.ContainsKey(item))
                {
                    ret.Add(item, true);
                }
            }
            return ret;
        }

    }
}