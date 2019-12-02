using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartLinks.Models
{
    public class TunableInfoData
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

        private static List<string> RetrieveModuleSN(List<string> snlist)
        {
            //if (snlist.Count > 0)
            //{
            //    var sncond = "('" + string.Join("','", snlist) + "')";
            //    var sql = @"select p.mfr_sn,pt.mfr_sn from parts pt 
            //                left join assembly am on am.child_index = pt.opt_index 
            //                left join parts p on p.opt_index = am.parent_index  
            //                where pt.mfr_sn in <sncond>";
            //    sql = sql.Replace("<sncond>", sncond);

            //    var originalmodulesn = new List<string>();
            //    var msndict = new Dictionary<string, bool>();

            //    var dbret = DBUtility.ExeATESqlWithRes(sql);
            //    foreach (var line in dbret)
            //    {
            //        var modulesn = Convert2Str(line[0]);
            //        var csn = Convert2Str(line[1]);

            //        if (string.IsNullOrEmpty(modulesn))
            //        {
            //            originalmodulesn.Add(csn);
            //            continue;
            //        }

            //        if (string.Compare(modulesn, csn, true) == 0)
            //        { continue; }

            //        if (!msndict.ContainsKey(modulesn))
            //        { msndict.Add(modulesn, true); }
            //    }//end foreach

            //    foreach (var sn in originalmodulesn)
            //    {
            //        if (!msndict.ContainsKey(sn))
            //        { msndict.Add(sn, true); }
            //    }
            //    return msndict.Keys.ToList();
            //}

            return new List<string>();
        }

        public static List<TunableInfoData> GetData(List<string> snlist, Controller ctrl)
        {
            var ret = new List<TunableInfoData>();
            //var newsnlist = RetrieveModuleSN(snlist);
            //if (newsnlist.Count > 0)
            //{
            //    var sncond = "('" + string.Join("','", newsnlist) + "')";
            //    var sql = @"select p.mfr_sn,p.mfr_pn,pt.mfr_sn,am.notes,pt.mfr_pn,mf.mfr_name from parts pt 
            //                left join assembly am on am.child_index = pt.opt_index 
            //                left join parts p on p.opt_index = am.parent_index  
            //                left join mfr_info mf on pt.mfr_id = mf.mfr_id 
            //                where p.mfr_sn  in <sncond> order by p.mfr_sn";
            //    sql = sql.Replace("<sncond>", sncond);

            //    var dbret = DBUtility.ExeATESqlWithRes(sql);
            //    foreach (var line in dbret)
            //    {
            //        var vm = new TunableInfoData();
            //        vm.SN = Convert2Str(line[0]);
            //        vm.PN = Convert2Str(line[1]);
            //        vm.CSN = Convert2Str(line[2]);
            //        vm.CName = Convert2Str(line[3]);
            //        vm.CPN = Convert2Str(line[4]);
            //        vm.MFRName = Convert2Str(line[5]);
            //        ret.Add(vm);
            //    }

            //    return ret;
            //}
            return new List<TunableInfoData>();
        }

        public TunableInfoData()
        {
            SN = "";
            PN = "";
            CSN = "";
            CName = "";
            CPN = "";
            MFRName = "";
        }

        public string SN { set; get; }
        public string PN { set; get; }
        public string CSN { set; get; }
        public string CName { set; get; }
        public string CPN { set; get; }
        public string MFRName { set; get; }
    }
}