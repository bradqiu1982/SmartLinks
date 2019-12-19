using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Caching;
using MathNet.Numerics;
namespace SmartLinks.Models
{
    public class SNApertureSizeVM
    {
        public SNApertureSizeVM()
        {
            SN = "";
            Wafer = "";
            IthSlope = "";
            Intercept = "";
            Ith = "";
            ApertureConst = "";
            ApertureSize = "";
            IthList = new List<double>();
            PwrList = new List<double>();
        }

        private static List<string> GetAllFiles(string bifolder,Controller ctrl)
        {
            if (ctrl != null)
            {
                var cachelist = (List<string>)ctrl.HttpContext.Cache.Get("allwafermapfiles");
                if (cachelist != null)
                { return cachelist; }
            }

            var allwafermapfiles = ExternalDataCollector.DirectoryEnumerateAllFilesByYear(ctrl, bifolder,"2019;2020;2021;2022");

            if (ctrl != null && allwafermapfiles.Count > 0)
            {
                if (ctrl.HttpContext.Cache != null)
                {
                    ctrl.HttpContext.Cache.Insert("allwafermapfiles", allwafermapfiles, null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
                }
            }

            return allwafermapfiles;

        }

        public static List<SNApertureSizeVM> LoadData(List<string> snlist,Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var bifolder = syscfg["BIITHFOLDER"];
            var allfiles = GetAllFiles(bifolder, ctrl);
            var snwfdict = UT.GetWaferFromSN(snlist);

            var ret = new List<SNApertureSizeVM>();
            foreach (var sn in snlist)
            {
                SNApertureSizeVM tempvm = null;

                var fs = "";
                foreach (var f in allfiles)
                {
                    if (f.ToUpper().Contains(sn.ToUpper()) && f.ToUpper().Contains("_PRE.TXT"))
                    {
                        fs = f;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(fs))
                { continue; }

                var bifile = ExternalDataCollector.DownloadShareFile(fs, ctrl);
                if (!string.IsNullOrEmpty(bifile))
                {
                    var alline = System.IO.File.ReadAllLines(bifile);
                    var idx = 1;
                    foreach (var line in alline)
                    {
                        if (line.ToUpper().Contains("CHANNEL"))
                        {
                            tempvm = new SNApertureSizeVM();
                            tempvm.SN = sn.ToUpper();
                            tempvm.CH = "CH" + idx;
                            if (snwfdict.ContainsKey(tempvm.SN))
                            { tempvm.Wafer = snwfdict[tempvm.SN]; }

                            ret.Add(tempvm);
                            idx++;
                            continue;
                        }

                        if (tempvm != null && tempvm.IthList.Count < 14)
                        {
                            var items = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                            tempvm.IthList.Add(UT.O2D(items[1]));
                            tempvm.PwrList.Add(UT.O2D(items[2]));
                        }
                    }//end foreach
                }//end if

                foreach (var item in ret)
                {
                    if (item.IthList.Count == 14 && item.PwrList.Count == 14)
                    {
                        var rest = Fit.Line( item.IthList.ToArray(),item.PwrList.ToArray());
                        item.Intercept = rest.Item1.ToString();
                        item.IthSlope = rest.Item2.ToString();
                        item.Ith = (Math.Abs(rest.Item1 / rest.Item2)/1000.0).ToString();
                    }
                }

                var wflist = new List<string>();
                foreach (var kv in snwfdict)
                {
                    var wf = kv.Value.ToUpper().Trim();
                    if (!wflist.Contains(wf))
                    { wflist.Add(wf); }
                }

                var wfapdict = new Dictionary<string, string>();
                foreach (var w in wflist)
                {
                    var apconst  = ProbeTestData.PrepareAPConst2162(w);
                    wfapdict.Add(w, apconst);
                }

                foreach (var item in ret)
                {
                    if (!string.IsNullOrEmpty(item.Wafer) && wfapdict.ContainsKey(item.Wafer))
                    {
                        item.ApertureConst = wfapdict[item.Wafer];
                        if (!string.IsNullOrEmpty(item.ApertureConst) && !string.IsNullOrEmpty(item.Ith))
                        {
                            var apconst = UT.O2D(item.ApertureConst);
                            var ith = UT.O2D(item.Ith);
                            item.ApertureSize = (ith * 7996.8 + apconst).ToString();
                        }
                    }
                }
                
            }
            return ret;
        }

        public string SN { set; get; }
        public string CH { set; get; }
        public string Wafer { set; get; }
        public string IthSlope { set; get; }
        public string Intercept { set; get; }
        public string Ith { set; get; }
        public string ApertureConst { set; get; }
        public string ApertureSize { set; get; }
        public List<double> IthList { set; get; }
        public List<double> PwrList { set; get; }

    }
}