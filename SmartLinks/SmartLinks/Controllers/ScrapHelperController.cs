using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;
using System.Text;
using System.IO;
using System.Net;

namespace SmartLinks.Controllers
{
    public class ScrapHelperController : Controller
    {
        public static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        private void UserAuth()
        {
            string IP = Request.UserHostName;
            var compName = DetermineCompName(IP);
            ViewBag.compName = compName;
            var glbcfg = CfgUtility.GetSysConfig(this);
            if (glbcfg.ContainsKey(ViewBag.compName.ToUpper()))
            {
                ViewBag.LogAdmin = true;
            }
        }
        
        // GET: ScrapHelper
        public ActionResult Index()
        {
            UserAuth();
            return View();
        }

        private List<ScrapTableItem> RetrieveScrapData()
        {
            var marks = Request.Form["marks"];
            List<string> SNList = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var inputdata = new List<ScrapTableItem>();

            foreach (var item in SNList)
            {
                var tempvm = new ScrapTableItem();

                var sn = item.Replace("'", "").Trim().ToUpper();
                if (sn.Length > 7)
                {
                    tempvm.DateCode = sn;
                }
                else
                {
                    tempvm.SN = sn;
                }
                inputdata.Add(tempvm);
            }

            var scraptable = ScrapVM.RetrievePNBySNDC(inputdata);
            SnTestDataVM.RetrieveTestData(scraptable);
            ScrapVM.MatchRudeRule(scraptable);
            ScrapVM.FinalSetResult(scraptable,this);

            return scraptable;
        }

        public JsonResult QueryScrap()
        {
            var scraptable = RetrieveScrapData();
            foreach (var item in scraptable)
            {
                ScrapHistoryVM.AddHistory(item);
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true,
                data = scraptable
            };
            return ret;
        }


        private List<string> PrepeareSNScrapReport()
        {
            var wafertable = RetrieveScrapData();
            var ret = new List<string>();

            var line = "SN,Date Code,PN,Site,Failure Code,Match Rule,Result";
            ret.Add(line);

            foreach (var item in wafertable)
            {
                var line1 = string.Empty;
                line1 = "\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.DateCode.Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                    + "\"" + item.WhichTest.Replace("\"", "") + "\"," + "\"" + item.TestData.ErrAbbr.Replace("\"", "") + "\"," + "\"" + item.MatchedRule.Replace("\"", "") + "\"," + "\"" + item.Result.Replace("\"", "") + "\",";

                ret.Add(line1);
            }

            return ret;
        }

        public JsonResult DownloadScrapData()
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "SN_Scrap_Status_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;
            var url = "/userfiles/docs/" + datestring + "/" + fn;

            var lines = PrepeareSNScrapReport();

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

        private List<string> PrepeareSNScrapHistory()
        {
            var historytable = ScrapHistoryVM.RetrieveHistory();
            var ret = new List<string>();

            var line = "SN,Date Code,PN,Site,Failure Code,Match Rule,Result,Create Date";
            ret.Add(line);

            foreach (var item in historytable)
            {
                var line1 = string.Empty;
                line1 = "\"" + item.SN.ToString().Replace("\"", "") + "\"," + "\"" + item.DateCode.Replace("\"", "") + "\"," + "\"" + item.PN.Replace("\"", "") + "\","
                    + "\"" + item.WhichTest.Replace("\"", "") + "\"," + "\"" + item.ErrAbbr.Replace("\"", "") + "\"," + "\"" + item.MatchRule.Replace("\"", "")
                    + "\"," + "\"" + item.Result.Replace("\"", "") + "\"," + "\"" + item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss").Replace("\"", "") + "\",";

                ret.Add(line1);
            }

            return ret;
        }

        public JsonResult DownloadScrapHistory()
        {
            string datestring = DateTime.Now.ToString("yyyyMMdd");
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
            if (!Directory.Exists(imgdir))
            {
                Directory.CreateDirectory(imgdir);
            }

            var fn = "Scrap_History_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var filename = imgdir + fn;
            var url = "/userfiles/docs/" + datestring + "/" + fn;

            var lines = PrepeareSNScrapHistory();

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

        private string GetActivePN(string ActivePN,List<PnMainVM> pnvmlist)
        {
            var activepn = "";
            if (string.IsNullOrEmpty(ActivePN))
            {
                if (pnvmlist.Count > 0)
                {
                    activepn = pnvmlist[0].PN;
                }
            }
            else
            {
                var pnexist = false;
                foreach (var item in pnvmlist)
                {
                    if (string.Compare(item.PN, ActivePN,true) == 0)
                    {
                        pnexist = true;
                        break;
                    }
                }
                if (pnexist)
                {
                    activepn = ActivePN;
                }
                else
                {
                    if (pnvmlist.Count > 0)
                    {
                        activepn = pnvmlist[0].PN;
                    }
                }
            }
            return activepn;
        }

        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        public ActionResult SettingScrapRule(string ActivePN,string SearchKey="")
        {
            var pnvmlist = PnMainVM.RetrievePNList(SearchKey);
            var activepn = GetActivePN(ActivePN, pnvmlist);

            ViewBag.pnvmlist = pnvmlist;
            ViewBag.activepn = activepn;

            var pnsettings = PnMainVM.RetrieveAllPnSetting(activepn);
            if (pnsettings.Count > 0)
            {
                ViewBag.activepnsettings = pnsettings[0];
            }

            ViewBag.ruleresultlist = CreateSelectList(new List<string>(new string[] { SCRAPRESULT.DSCRAP, SCRAPRESULT.ISCRAP, SCRAPRESULT.REWORK }), "");
            ViewBag.defreslist = CreateSelectList(new List<string>(new string[] { SCRAPRESULT.DSCRAP, SCRAPRESULT.ISCRAP, SCRAPRESULT.REWORK }), "");
            if (ViewBag.activepnsettings != null)
            {
                ViewBag.updatedefreslist = CreateSelectList(new List<string>(new string[] { SCRAPRESULT.DSCRAP, SCRAPRESULT.ISCRAP, SCRAPRESULT.REWORK }), ViewBag.activepnsettings.DefaultResult);
            }


            return View();
        }

        public JsonResult NewPN()
        {
            var pn = Request.Form["npn"].ToUpper();
            var pj = Request.Form["npro"];
            var defres = Request.Form["defres"];
            PnMainVM.StoreNewPN(pn, pj, defres);
            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true
            };
            return ret;
        }

        public JsonResult RemovePN()
        {
            var pnkey = Request.Form["pnkey"];
            PnMainVM.RemovePN(pnkey);
            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true
            };
            return ret;
        }

        private void StoreMesTables(string mesfilepath,string pnkey)
        {
            try
            {
                if (System.IO.File.Exists(mesfilepath))
                {
                    string[] lines = System.IO.File.ReadAllLines(mesfilepath);
                    bool tableseg = false;
                    foreach (var line in lines)
                    {
                        if (line.Contains(";"))
                        {
                            continue;
                        }

                        if (line.ToUpper().Contains("[MESTABLENAME]"))
                        {
                            tableseg = true;
                            continue;
                        }

                        if (tableseg && line.Contains("[") && line.Contains("]"))
                        {
                            tableseg = false;
                        }

                        if (tableseg && line.Contains("="))
                        {
                            PnMESVM.StoreMesTab(pnkey, line.Split(new char[] { '=' })[0].Trim(), line.Split(new char[] { '=' })[1].Trim().Replace("\"", ""));
                        }
                    }
                }

            }
            catch (Exception ex){}
        }

        public JsonResult UpdatePNMainVM()
        {
            var pnkey = Request.Form["pnkey"];
            var pn = Request.Form["npn"];
            var pnpj = Request.Form["npro"];
            var defres = Request.Form["defres"];

            PnMainVM.UpdatePNPJ(pnkey, pn, pnpj,defres);

            foreach (string fl in Request.Files)
            {
                if (fl != null && Request.Files[fl].ContentLength > 0
                    && string.Compare(Path.GetExtension(Path.GetFileName(Request.Files[fl].FileName)), ".ini", true) == 0)
                {
                    string fn = System.IO.Path.GetFileName(Request.Files[fl].FileName);
                    string datestring = DateTime.Now.ToString("yyyyMMdd");
                    string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                    if (!Directory.Exists(imgdir))
                    {
                        Directory.CreateDirectory(imgdir);
                    }

                    fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                    Request.Files[fl].SaveAs(imgdir + fn);

                    StoreMesTables(imgdir + fn, pnkey);
                }
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true
            };
            return ret;
        }

        public JsonResult GetPNWhichTables()
        {
            var pnkey = Request.Form["pnkey"];
            var pnmeslist = PnMESVM.RetrievePnWhichTestByPNKey(pnkey);

            var ret = new JsonResult();
            ret.Data = new
            {
                data = pnmeslist
            };
            return ret;
        }

        public JsonResult GetAllErrAbbr()
        {
            var errlist = ScrapVM.RetriveAllErrAbbr();
            var ret = new JsonResult();
            ret.Data = new
            {
                data = errlist
            };
            return ret;
        }

        public JsonResult GetAllTestCase()
        {
            var tclist = new List<string>();
            tclist.Add("ALL");
            var tlist = PnRulesVM.RetrieveAllTestCase();
            tclist.AddRange(tlist);
            var ret = new JsonResult();
            ret.Data = new
            {
                data = tclist
            };
            return ret;
        }

        public JsonResult UpdatePNRule()
        {

            var nruleid = PnMainVM.GetUniqKey();

            var rule_id = Request.Form["rule_id"];
            var pnkey = Request.Form["pnkey"];
            var whichtest = Request.Form["whichtest"];
            var errabbr = Request.Form["errabbr"];
            var testcase = Request.Form["testcase"];
            var param = Request.Form["param"];
            var min = Request.Form["min"];
            var max = Request.Form["max"];
            var ruleres = Request.Form["ruleres"];

            if (string.IsNullOrEmpty(rule_id))
            {
                PnRulesVM.AddRule(pnkey, nruleid, whichtest, errabbr, param, min, max, ruleres,testcase);
            }
            else
            {
                PnRulesVM.EditRule(pnkey, rule_id, whichtest, errabbr, param, min, max, ruleres,testcase);
            }

            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true,
                rid = nruleid
            };
            return ret;
        }

        public JsonResult RemovePNRule()
        {
            var ruleid = Request.Form["rule_id"];
            PnRulesVM.RemoveRule(ruleid);
            var ret = new JsonResult();
            ret.Data = new
            {
                sucess = true
            };
            return ret;
        }

    }
}