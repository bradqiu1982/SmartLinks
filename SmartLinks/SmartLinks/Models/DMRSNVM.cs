using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                return Convert.ToDateTime(line[0]).ToString("yyyy-MM-dd HH:mm:ss");
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

        private static void UpdateSNStatus(string prodline)
        {

        }

        public static void UpdateDMRSNStatus(string prodline,Controller ctrl)
        {
            UpdateDMRSN(prodline,ctrl);
            UpdateSNStatus(prodline);
        }

        public DMRSNVM()
        {
            DMRID = "";
            DMRProdLine = "";
            DMRDate = "";
            DMRCreater = "";
            SN = "";
            SNStatus = "";
            JO = "";
            PN = "";
            WorkFlow = "";
            WorkFlowStep = "";
            DMRFileURL = "";
        }

        public string DMRID { set; get; }
        public string DMRProdLine { set; get; }
        public string DMRDate { set; get; }
        public string DMRCreater { set; get; }
        public string SN { set; get; }
        public string SNStatus { set; get; }
        public string JO { set; get; }
        public string PN { set; get; }
        public string WorkFlow { set; get; }
        public string WorkFlowStep { set; get; }
        public string DMRFileURL { set; get; }

    }
}