using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class PNSNFR4Binning
    {
        public static List<PNSNFR4Binning> GetPNSNData(List<string> pnlist,string sdate,string edate)
        {
            var ret = new List<PNSNFR4Binning>();
            var sndict = new Dictionary<string, bool>();
            var pncond = "('" + string.Join("','", pnlist) + "')";

            var sql = @"select top 100000 dc.[ModuleSerialNum],dc.AssemblyPartNum,dc.ModulePartNum,dc.TestTimeStamp,dc.ErrAbbr,dce.[ProductGrade],dce.ProdBinPwrConsumption,dce.ModuleTemp_C,ws.WorkflowStepName from [InsiteDB].[insite].[dce_QuickTest_main] dce (nolock)
                        left join [InsiteDB].[insite].[dc_QuickTest] dc (nolock) on dc.dc_QuickTestHistoryId = dce.ParentHistoryID
                        left join InsiteDB.insite.Container c (nolock) on c.containername = dc.[ModuleSerialNum]
                        left join InsiteDB.insite.currentStatus cs (nolock) on c.currentStatusId = cs.currentStatusId 
                        left join InsiteDB.insite.workflowstep ws(nolock) on  cs.WorkflowStepId = ws.WorkflowStepId 
                        where dc.ErrAbbr = 'pass' and dce.ChannelNumber = '0'  and dc.[ModuleSerialNum] is not null and dce.CornerID like '1H' 
                        and dce.[ProductGrade] is not null and C.[Status] = 1 and c.HoldReasonId is null
                        and ( (dc.AssemblyPartNum in <pncond> and dc.TestTimeStamp > '<sdate>' and dc.TestTimeStamp < '<edate>' ) or dc.[ModuleSerialNum] in <pncond> )
                        order by dc.[ModuleSerialNum],dc.TestTimeStamp desc";
            sql = sql.Replace("<pncond>", pncond).Replace("<sdate>", sdate).Replace("<edate>", edate);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]).ToUpper().Trim();
                if (sndict.ContainsKey(sn))
                { continue; }
                sndict.Add(sn, true);

                ret.Add(new PNSNFR4Binning(UT.O2S(line[0]), UT.O2S(line[1]), UT.O2S(line[2]), UT.O2T(line[3]), UT.O2S(line[4]), UT.O2S(line[5]), UT.O2S(line[6]), UT.O2S(line[7]), UT.O2S(line[8])));
            }

            return ret;
        }

        public PNSNFR4Binning(string sn,string pn,string mpn,string tm,string err,string pg,string pbp,string modc,string wst)
        {
            SN = sn;
            PN = pn;
            MPN = mpn;
            Time = tm;
            ErrAbbr = err;
            ProductGrade = pg;
            ProdBinPwrCsum = pbp;
            ModTempC = modc;
            WorkFlowStep = wst;
            Status = "ACTIVE";
        }

        public string SN { set; get; }
        public string PN { set; get; }
        public string MPN { set; get; }
        public string Time { set; get; }
        public string ErrAbbr { set; get; }
        public string ProductGrade { set; get; }
        public string ProdBinPwrCsum { set; get; }
        public string ModTempC { set; get; }
        public string WorkFlowStep { set; get; }
        public string Status { set; get; }
    }
}