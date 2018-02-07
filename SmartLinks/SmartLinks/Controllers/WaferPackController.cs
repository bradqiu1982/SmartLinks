using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;
using System.Text;
using System.IO;

namespace SmartLinks.Controllers
{

    public class WaferPackController : Controller
    {
        // GET: WaferPack
        public ActionResult Index()
        {
            ViewBag.NG_Wafer_List = WaferPackVM.RetrieveNGWafer();
            return View();
        }

        private List<WaferTableItem> RetrieveWaferData()
        {
            var marks = Request.Form["marks"];
            List<string>SNList = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var DateCodeList = new List<string>();
            foreach (var item in SNList)
            {
                var sn = item.Replace("'", "").Trim().ToUpper();
                if (sn.Length > 7)
                {
                    DateCodeList.Add(sn);
                }
            }

            var sndict = new Dictionary<string, bool>();
            var wafertable = new List<WaferTableItem>();

            if (DateCodeList.Count > 0)
            {
                var datesnlist = WaferPackVM.RetrieveSNByDateCode(DateCodeList);
                foreach (var item in datesnlist)
                {
                    var sn = item.SN.Replace("'", "").Trim().ToUpper();
                    if (!sndict.ContainsKey(sn))
                    {
                        sndict.Add(sn, true);
                        wafertable.Add(item);
                    }
                }
            }

            foreach (var item in SNList)
            {
                var sn = item.Replace("'", "").Trim().ToUpper();
                if (sn.Length <= 7 && !sndict.ContainsKey(sn))
                {
                    sndict.Add(sn , true);
                    var tempvm = new WaferTableItem();
                    tempvm.SN = sn;
                    wafertable.Add(tempvm);
                }
            }

            WaferPackVM.RetrieveWaferBySN(wafertable);
            var ngwaferdict = WaferPackVM.RetrieveNGWaferDict();
            foreach (var item in wafertable)
            {
                if (ngwaferdict.ContainsKey(item.WaferNum))
                {
                    item.Status = WAFERSTATUS.NG;
                }
                else
                {
                    item.Status = WAFERSTATUS.GOOD;
                }
            }

            return wafertable;
        }

        public JsonResult QueryWafer()
        {

            var wafertable = RetrieveWaferData();

            var waferdupdict = new Dictionary<string, string>();
            var waferdupcount = new Dictionary<string, int>();
            foreach (var item in wafertable)
            {
                if (!string.IsNullOrEmpty(item.DateCode))
                {
                    if (!waferdupdict.ContainsKey(item.DateCode))
                    {
                        waferdupdict.Add(item.DateCode, item.WaferNum);
                        waferdupcount.Add(item.DateCode, 1);
                    }
                    else
                    {
                        if (string.Compare(item.WaferNum, waferdupdict[item.DateCode],true) != 0)
                        {
                            waferdupcount[item.DateCode] = waferdupcount[item.DateCode] + 1;
                        }
                    }
                }
            }

            var waferdup = "";
            foreach (var item in waferdupcount)
            {
                if (item.Value > 1)
                {
                    waferdup = waferdup + item.Key + ";";
                }
            }

            var ret = new JsonResult();
            ret.Data = new { sucess = true,
                             data = wafertable,
                             waferdup = waferdup
            };
            return ret;
        }

        private List<string> PrepeareSNWaferReport()
        {
            var wafertable = RetrieveWaferData();
            var ret = new List<string>();

            var line = "SN,Date Code,Wafer Num,PN,Status";
            ret.Add(line);

            foreach (var item in wafertable)
            {
                var line1 = string.Empty;
                line1 = "\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.DateCode.Replace("\"", "") + "\"," + "\"" + item.WaferNum.Replace("\"", "") + "\","
                    + "\"" + item.PN.Replace("\"", "") + "\"," + "\"" + item.Status.Replace("\"", "") + "\",";

                ret.Add(line1);
            }

            return ret;
        }

        public JsonResult DownloadWaferData()
        {
            var wafertable = RetrieveWaferData();
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "SN_Wafer_Status_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;
            var url = "/userfiles/docs/" + datestring + "/" + fn;

            var lines = PrepeareSNWaferReport();

            var wholefile = "";
            foreach (var l in lines)
            {
                wholefile = wholefile + l + "\r\n";
            }
            System.IO.File.WriteAllText(filename, wholefile, Encoding.UTF8);

            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true,
                data = url
            };
            return ret;
        }

        public JsonResult UpdateNGWafer()
        {
            var wafer_list = Request.Form["wafer_list"];
            List<string> wafelist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(wafer_list, (new List<string>()).GetType());
            WaferPackVM.UpdateNGWafer(wafelist);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

    }

}