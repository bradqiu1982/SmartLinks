using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SmartLinks.Controllers
{
    public class SmartLinksController : Controller
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

        private List<LinkVM> RetrieveAllLinks(string machine)
        {
            var ret = new List<LinkVM>();
            var mvm = MachineLink.RetrieveLinks(machine);
            var mvmdict = new Dictionary<string, bool>();
            foreach (var item in mvm)
            {
                mvmdict.Add(item.LinkName, true);
                if (string.Compare(item.Action, LINKACTION.DELETE) != 0)
                {
                    var templink = new LinkVM();
                    templink.LinkName = item.LinkName;
                    templink.Link = item.Link;
                    templink.Logo = item.Logo;
                    templink.Comment = item.Comment;
                    ret.Add(templink);
                }
            }
            var vm = LinkVM.RetrieveLinks();
            foreach (var item in vm)
            {
                if (!mvmdict.ContainsKey(item.LinkName))
                {
                    ret.Add(item);
                }
            }

            return ret;
        }

        // GET: SmartLinks
        public ActionResult All()
        {
            ViewBag.isie8 = false;
            ViewBag.showie8modal = false;

            var browse = Request.Browser;
            if (string.Compare(browse.Browser, "IE", true) == 0
                && (string.Compare(browse.Version, "7.0", true) == 0
                || string.Compare(browse.Version, "8.0", true) == 0))
            {
                ViewBag.isie8 = true;
            }

            //var logonnames = Request.LogonUserIdentity.Name;
            //if (!string.IsNullOrEmpty(logonnames))
            //{
            //    var splitnames = logonnames.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
            //    ViewBag.logonname = splitnames[splitnames.Length - 1];
            //}

            var machine = string.Empty;
            var ckdict = CookieUtility.UnpackCookie(this);
            if (!ckdict.ContainsKey("reqmachine"))
            {
                string IP = Request.UserHostName;
                string compName = DetermineCompName(IP);
                if (!string.IsNullOrEmpty(compName))
                {
                    var tempdict = new Dictionary<string, string>();
                    tempdict.Add("reqmachine", compName);
                    CookieUtility.SetCookie(this, tempdict);
                    machine = compName.ToUpper();
                }//end if
            }//end
            else
            {
                machine = ckdict["reqmachine"].ToUpper();
            }

            ViewBag.logonname = machine;
            var mudict = MachineUserMap.RetrieveUserMap(machine);
            if (mudict.ContainsKey(machine))
            {
                ViewBag.logonname = mudict[machine];
            }

            if (!string.IsNullOrEmpty(machine))
            {
                ViewBag.machine = machine;
                if (ViewBag.isie8)
                {
                    if (!MachineLink.IsNeverShowIE8Modal(machine))
                    {
                        ViewBag.showie8modal = true;
                    }
                }
            }

            return View();
        }

        [HttpPost]
        public JsonResult AllData()
        {

            var vm = new List<LinkVM>();
            var ckdict = CookieUtility.UnpackCookie(this);
            if (!ckdict.ContainsKey("reqmachine"))
            {
                string IP = Request.UserHostName;
                string compName = DetermineCompName(IP);
                if (!string.IsNullOrEmpty(compName))
                {
                    var tempdict = new Dictionary<string, string>();
                    tempdict.Add("reqmachine", compName);
                    CookieUtility.SetCookie(this, tempdict);
                }//end if
                vm = LinkVM.RetrieveLinks();
            }//end
            else
            {
                vm = RetrieveAllLinks(ckdict["reqmachine"]);
            }

            var res = new JsonResult();
            res.Data = new { data = vm};
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return res;
        }

        public ActionResult AddLink()
        {
            return View();
        }


        public string GetLinkLogo()
        {
            try
            {
                foreach (string fl in Request.Files)
                {
                    if (fl != null && Request.Files[fl].ContentLength > 0)
                    {
                        var ext = Path.GetExtension(Path.GetFileName(Request.Files[fl].FileName)).ToUpper();
                        if (ext.Contains(".JPG")
                            || ext.Contains(".JPEG")
                            || ext.Contains(".PNG")
                            || ext.Contains(".BMP")
                            || ext.Contains(".GIF")
                            || ext.Contains(".SVG"))
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
                            return "/userfiles/docs/" + datestring + "/" + fn;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

            return string.Empty;

        }

        [HttpPost, ActionName("AddLink")]
        [ValidateAntiForgeryToken]
        public ActionResult AddLinkPost()
        {
            var linkname = Request["LinkName"];
            var link = Request["UrlAddr"];
            var comment = Request["comment"];
            var logo = GetLinkLogo();
            if (!string.IsNullOrEmpty(linkname)
                && !string.IsNullOrEmpty(link)
                && !string.IsNullOrEmpty(logo))
            {
                LinkVM.StoreLink(linkname, link, logo,comment);
            }
            return RedirectToAction("All", "SmartLinks");
        }

        public ActionResult RedirectToLink(string linkname)
        {
            var vm = new List<LinkVM>();
            var machine = string.Empty;

            var ckdict = CookieUtility.UnpackCookie(this);
            if (!ckdict.ContainsKey("reqmachine"))
            {
                string IP = Request.UserHostName;
                string compName = DetermineCompName(IP);
                if (!string.IsNullOrEmpty(compName))
                {
                    var tempdict = new Dictionary<string, string>();
                    tempdict.Add("reqmachine", compName);
                    machine = compName.ToUpper();
                    CookieUtility.SetCookie(this, tempdict);
                }//end if
                vm = LinkVM.RetrieveLinks();
            }//end
            else
            {
                vm = RetrieveAllLinks(ckdict["reqmachine"]);
                machine = ckdict["reqmachine"].ToUpper();
            }


            var validlink = string.Empty;
            foreach (var item in vm)
            {
                if (string.Compare(linkname, item.LinkName) == 0)
                {
                    validlink = item.Link;
                    if (!string.IsNullOrEmpty(machine))
                    {
                        MachineLink.UpdateFrequence(item.LinkName, item.Link, item.Logo, item.Comment, machine);
                    }
                    break;
                }
            }

            if (!string.IsNullOrEmpty(validlink))
            {
                //var logonnames = Request.LogonUserIdentity.Name;
                //if (!string.IsNullOrEmpty(logonnames))
                //{
                //    var splitnames = logonnames.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                //    ViewBag.logonname = splitnames[splitnames.Length - 1];
                //    LinkVM.UpdateSmartLinkLog(ViewBag.logonname,machine, validlink);
                //}

                ViewBag.logonname = machine;
                var mudict = MachineUserMap.RetrieveUserMap(machine);
                if (mudict.ContainsKey(machine))
                {
                    ViewBag.logonname = mudict[machine];
                    LinkVM.UpdateSmartLinkLog(ViewBag.logonname, machine, validlink);
                }


                if (validlink.Contains("wux-engsys01"))
                {
                    var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var timestamp = now +"_joke";
                    using (MD5 md5Hash = MD5.Create())
                    {
                        var smartkey1 = GetMd5Hash(md5Hash, timestamp) + "::" + now;
                        var smartkey2 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(smartkey1));
                        var smartkey = HttpUtility.UrlEncode(smartkey2);
                        return Redirect(validlink + "?smartkey="+smartkey);
                    }
                }
                else
                {
                    return Redirect(validlink);
                }
            }
            else
            {
                return RedirectToAction("All","SmartLinks");
            }

        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public JsonResult AddCustomLink()
        {
            var mvm = new MachineLink();
            mvm.Link = Request.Form["link"];
            mvm.LinkName = Request.Form["link_name"];
            mvm.Comment = Request.Form["comment"];
            mvm.Logo = Request.Form["image_url"];

            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("reqmachine"))
            {
                mvm.ReqMachine = ckdict["reqmachine"];
                MachineLink.StoreLink(mvm.LinkName, mvm.Link, mvm.Logo, mvm.Comment, mvm.ReqMachine);
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public JsonResult RemoveCustomLink()
        {
            var linkname = Request.Form["link_name"];

            var vm = new List<LinkVM>();
            var machine = string.Empty;

            var ckdict = CookieUtility.UnpackCookie(this);
            if (!ckdict.ContainsKey("reqmachine"))
            {
                string IP = Request.UserHostName;
                string compName = DetermineCompName(IP);
                if (!string.IsNullOrEmpty(compName))
                {
                    var tempdict = new Dictionary<string, string>();
                    tempdict.Add("reqmachine", compName);
                    machine = compName;
                    CookieUtility.SetCookie(this, tempdict);
                }//end if
                vm = LinkVM.RetrieveLinks();
            }//end
            else
            {
                vm = RetrieveAllLinks(ckdict["reqmachine"]);
                machine = ckdict["reqmachine"];
            }

            var validlink = string.Empty;
            foreach (var item in vm)
            {
                if (string.Compare(linkname, item.LinkName) == 0)
                {
                    validlink = item.Link;
                    if (!string.IsNullOrEmpty(machine))
                    {
                        MachineLink.RemoveCustomLink(item.LinkName, item.Link, item.Logo, item.Comment, machine);
                    }
                    break;
                }
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        public JsonResult NeverShowIE8Modal()
        {
            var ckdict = CookieUtility.UnpackCookie(this);
            if (ckdict.ContainsKey("reqmachine"))
            {
                var ReqMachine = ckdict["reqmachine"];
                MachineLink.NeverShowIE8Modal(ReqMachine);
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }


        public ActionResult CWDM4Info()
        {
            return View();
        }

        public JsonResult GetCWDM4InfoData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var cwdm4list = CWDM4Data.LoadCWDM4Info(snlist,this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new {
                cwdm4list = cwdm4list
            };
            return ret;
        }

        public ActionResult CWDM4WLInfo()
        {
            return View();
        }

        public JsonResult GetCWDM4WLInfoData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var cwdm4list = CWDM4Data.QueryWL(snlist, this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                cwdm4list = cwdm4list
            };
            return ret;
        }

        public ActionResult TunableInfo()
        {
            return View();
        }

        public JsonResult GetTunableInfoData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var tunablelist = TunableInfoData.GetData(snlist, this);
            var ret = new JsonResult();
            ret.Data = new
            {
                tunablelist = tunablelist
            };
            return ret;
        }

        public ActionResult ModuleSubAssembly()
        {
            return View();
        }

        public JsonResult ModuleSubAssemblyData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var assemblylist = ModuleAssemblyVM.RetrieveData(snlist);
            var ret = new JsonResult();
            ret.Data = new
            {
                assemblylist = assemblylist
            };
            return ret;
        }

        public ActionResult WaferStatus()
        {
            return View();
        }

        public JsonResult WaferStatusData()
        {
            var marks = Request.Form["marks"];
            List<string> wlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var waferdatalist = WaferStatusVM.RetrieveData(wlist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferdatalist = waferdatalist
            };
            return ret;
        }


        public ActionResult SNProgress()
        {
            return View();
        }

        public JsonResult SNWorkFlowData()
        {
            var marks = Request.Form["marks"];
            List<string> wlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var sndatalist = SNProVM.RetrieveWorkFlowData(wlist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sndatalist = sndatalist
            };
            return ret;
        }

        public JsonResult SNTestFlowData()
        {
            var marks = Request.Form["marks"];
            List<string> wlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var sndatalist = SNProVM.RetrieveTestFlowData(wlist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sndatalist = sndatalist
            };
            return ret;
        }

        public ActionResult JOWorkFlow()
        {
            return View();
        }

        public JsonResult JOWorkFlowData()
        {
            var marks = Request.Form["marks"];
            List<string> jolist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var jdatalist = JODetailVM.LoadData(jolist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                jdatalist = jdatalist
            };
            return ret;
        }

        public ActionResult SNLatestStatus()
        {
            return View();
        }

        public JsonResult SNStatusData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var sndatalist = JODetailVM.LoadSNData(snlist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sndatalist = sndatalist
            };
            return ret;
        }

        public ActionResult DMRTrace()
        {
            return View();
        }

        public JsonResult LoadDRMProLine()
        {
            var prodlist = CfgUtility.GetSysConfig(this)["DMRTRACEPRODUCTLINES"].Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new {
                prodlist = prodlist
            };
            return ret;
        }

        private List<DMRSNVM> ModuleDistribution(List<DMRSNVM> srcdata)
        {
            var stepdict = new Dictionary<string, DMRSNVM>();
            foreach (var item in srcdata)
            {
                if (stepdict.ContainsKey(item.WorkFlowStep))
                {
                    stepdict[item.WorkFlowStep].ModuleCount += 1;
                    stepdict[item.WorkFlowStep].WorkFlow = item.WorkFlow;
                }
                else
                {
                    var tempvm = new DMRSNVM();
                    tempvm.WorkFlow = item.WorkFlow;
                    tempvm.ModuleCount = 1;
                    stepdict.Add(item.WorkFlowStep, tempvm);
                }
            }

            var ret = new List<DMRSNVM>();
            foreach (var kv in stepdict)
            {
                var tempvm = new DMRSNVM();
                tempvm.WorkFlowStep = kv.Key;
                tempvm.WorkFlow = kv.Value.WorkFlow;
                tempvm.ModuleCount = kv.Value.ModuleCount;
                ret.Add(tempvm);
            }

            ret.Sort(delegate (DMRSNVM obj1, DMRSNVM obj2)
            {
                return obj2.ModuleCount.CompareTo(obj1.ModuleCount);
            });

            return ret;
        }

        private List<DMRSNVM> DMRStatusSum(List<DMRSNVM> srcdata)
        {
            var ret = new List<DMRSNVM>();

            var dmrdict = new Dictionary<string, DMRSNVM>();
            foreach (var item in srcdata)
            {
                if (dmrdict.ContainsKey(item.DMRID))
                {
                    dmrdict[item.DMRID].ModuleCount += 1;
                }
                else
                {
                    var tempvm = new DMRSNVM();
                    tempvm.DMRID = item.DMRID;
                    tempvm.DMRDate = item.DMRDate;
                    tempvm.DMROAStep = item.DMROAStep;
                    tempvm.DMROAStatus = item.DMROAStatus;
                    tempvm.ModuleCount = 1;

                    dmrdict.Add(item.DMRID, tempvm);
                }
            }

            if (dmrdict.Count > 0)
            {
                ret = dmrdict.Values.ToList();
                ret.Sort(delegate (DMRSNVM obj1, DMRSNVM obj2)
                {
                    return obj2.ModuleCount.CompareTo(obj1.ModuleCount);
                });
            }

            return ret;
        }

        private object DMRModuleYield(string prodline,List<DMRSNVM> srcdata)
        {
            var sndict = new Dictionary<string, bool>();
            foreach (var item in srcdata)
            {
                if (!sndict.ContainsKey(item.SN))
                {
                    sndict.Add(item.SN, true);
                }
            }
            var testdata = DMRSNTestData.RetrieveTestData(prodline,sndict);
            var yieldlist = (List<DMRSNTestData>)testdata[0];
            var faillist = (List<DMRSNTestData>)testdata[1];

            var cumyield = 1.0;

            var titlelist = new List<string>();
            titlelist.Add("");
            var passlist = new List<string>();
            passlist.Add("PASS");
            var totlelist = new List<string>();
            totlelist.Add("TOTAL");
            var ydlist = new List<string>();
            ydlist.Add("YIELD");

            foreach (var item in yieldlist)
            {
                titlelist.Add(item.WhichTest);
                passlist.Add(item.PassCnt.ToString());
                totlelist.Add(item.TotalCnt.ToString());
                if (item.Yield != 100.0)
                { ydlist.Add("YD:"+item.Yield.ToString()+"%"); }
                else
                { ydlist.Add(item.Yield.ToString() + "%"); }

                cumyield = cumyield * (item.Yield/100.0);
            }

            titlelist.Add("Cumm Yield");
            passlist.Add("");
            totlelist.Add("");
            ydlist.Add(Math.Round(cumyield*100.0,2).ToString()+"%");

            return new
            {
                titlelist = titlelist,
                passlist = passlist,
                totlelist = totlelist,
                ydlist = ydlist,
                faillist = faillist
            };
        }

        public JsonResult DMRWIPData()
        {
            var prodline = Request.Form["prodline"];
            var dmrdata = DMRSNVM.RetrieveDMRSNData(prodline,null,null,this);
            var moduledist = ModuleDistribution(dmrdata);
            var dmrstatuslist = DMRStatusSum(dmrdata);
            var yielddata = DMRModuleYield(prodline,dmrdata);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                moduledist = moduledist,
                dmrstatuslist = dmrstatuslist,
                dmrdata = dmrdata,
                yielddata = yielddata
            };
            return ret;
        }

        public JsonResult SNTRACEData()
        {
            var sns = Request.Form["sns"];
            var snlist = sns.Replace("'", "").Replace("\r\n", " ").Split(new string[] { ";", " ", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var dmrdata = DMRSNVM.RetrieveDMRSNDataBySN(snlist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                dmrdata = dmrdata
            };
            return ret;
        }

        public JsonResult DMRTRACEData()
        {
            var prodline = Request.Form["prodline"];

            var startdate = DateTime.Now;
            var enddate = DateTime.Now;
            var sdate = DateTime.Parse(Request.Form["sdate"]);
            var edate = DateTime.Parse(Request.Form["edate"]);
            if (sdate < edate)
            {
                startdate = DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00");
                enddate = DateTime.Parse(edate.ToString("yyyy-MM-dd") + " 00:00:00").AddDays(1).AddSeconds(-1);
            }
            else
            {
                startdate = DateTime.Parse(edate.ToString("yyyy-MM-dd") + " 00:00:00");
                enddate = DateTime.Parse(sdate.ToString("yyyy-MM-dd") + " 00:00:00").AddDays(1).AddSeconds(-1);
            }
            var dmrdata = DMRSNVM.RetrieveDMRSNData(prodline,startdate.ToString("yyyy-MM-dd HH:mm:ss")
                ,enddate.ToString("yyyy-MM-dd HH:mm:ss"), this);
            var moduledist = ModuleDistribution(dmrdata);
            var dmrstatuslist = DMRStatusSum(dmrdata);
            var yielddata = DMRModuleYield(prodline,dmrdata);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                moduledist = moduledist,
                dmrstatuslist = dmrstatuslist,
                dmrdata = dmrdata,
                yielddata = yielddata
            };
            return ret;
        }
        
        public JsonResult SNWholeWorkFlow()
        {
            var sn = Request.Form["sn"];
            var snworkflowlist = DMRSNVM.RetrieveSNWorkFlow(sn);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                snworkflowlist = snworkflowlist
            };
            return ret;
        }


        public ActionResult SNFAFStatus()
        { return View(); }

        public JsonResult SNFAFStatusData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var sndatalist = JODetailVM.SnFAFStatus(snlist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sndatalist = sndatalist
            };
            return ret;
        }

        public ActionResult TestExcelFile()
        {

            var data = ExcelReader.RetrieveDataFromExcel(@"E:\video\HCR registry.xlsx", "");
            return View("All");
        }


        public ActionResult TestEmail()
        {
            var to = new List<string>();
            to.Add("brad.qiu@II-VI.COM");
            EmailUtility.SendEmail(this, "TEST NEW EMAIL SERVER3", to, "Hello World3");
            new System.Threading.ManualResetEvent(false).WaitOne(500);
            return View("All");
        }



        //public ActionResult LoadNeoMapData()
        //{
        //    var path = @"\\wux-engsys01\CostData\Apcalc2162\";

        //    var fnames = new string[] { "191007-10.D00.filtered.csv" };
        //    foreach (var f in fnames)
        //    {
        //        var alldata = ExcelReader.RetrieveDataFromExcel_CSV(path + f, "");

        //        var datalist = new List<ProbeTestData>();

        //        var idx = 0;
        //        foreach (var line in alldata)
        //        {
        //            if (idx == 0)
        //            { idx++; continue; }

        //            var wafer = UT.O2S(line[0]);
        //            var famy = UT.O2S(line[1]);
        //            var x = UT.O2S(UT.O2I(line[2].Replace(".000000","")));
        //            var y = UT.O2S(UT.O2I(line[3].Replace(".000000", "")));
        //            var bin = UT.O2S(line[4]);
        //            var apsize = UT.O2S(line[5]);

        //            if (!string.IsNullOrEmpty(wafer)
        //                && !string.IsNullOrEmpty(x)
        //                && !string.IsNullOrEmpty(y)
        //                && !string.IsNullOrEmpty(apsize))
        //            {
        //                datalist.Add(new ProbeTestData(wafer, famy, x, y, bin, apsize));
        //            }
        //        }

        //        if (datalist.Count > 0)
        //        {
        //            ProbeTestData.CleanData(datalist[0].Wafer);
        //        }

        //        if (datalist.Count > 0)
        //        {
        //            var array = ProbeTestData.GetWaferArray(datalist[0].Wafer);
        //            foreach (var item in datalist)
        //            {
        //                item.APVal1 = array;
        //                item.StoreData();
        //            }
        //        }

        //    }
        //    return View("All");
        //}

        public ActionResult VcselScreen()
        {
            return View();
        }

        public JsonResult VcselScreenData()
        {
            string IP = Request.UserHostName;
            string compName = DetermineCompName(IP);

            var wf = Request.Form["wf"].Trim();
            var x = UT.O2I(Request.Form["x"]).ToString();
            var y = UT.O2I(Request.Form["y"]).ToString();

            var rest = "";
            if (wf.Length != 9 || !wf.Contains("-"))
            {
                rest = "<span style='color:red'>WAFER NUMBER WRONG</span>";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wf = wf,
                    x = "X: " + x,
                    y = "Y: " + y,
                    ar = "Array: ",
                    ap = "AP: ",
                    rest = rest
                };
                return ret1;
            }

            var xlist = new List<int>();
            
            var probe = ProbeTestData.GetApSizeByWafer(wf, x, y);
            if (string.IsNullOrEmpty(probe.ApSize))
            {
                if (ProbeTestData.HasData(wf))
                { rest = "<span style='color:red'>NO DATA</span>"; }
                else
                { rest = "<span style='color:red'>"+ ProbeTestData.PrepareWaferAPSizeData(wf) + "</span>"; }
            }
            else
            {
                var aps = UT.O2D(probe.ApSize);
                if (aps > 6.5)
                {
                    var array = UT.O2I(probe.APVal1);
                    if (array > 1)
                    {

                        var idx = (UT.O2I(x)-1) / array;
                        var firstx = idx * array + 1;
                        xlist.Add(firstx);
                        for (var i = 1; i < array; i++)
                        { xlist.Add(firstx + i); }

                        var matchfail = false;
                        foreach (var tempx in xlist)
                        {
                            probe = ProbeTestData.GetApSizeByWafer(wf, tempx.ToString(), y);
                            if (!string.IsNullOrEmpty(probe.ApSize) && UT.O2D(probe.ApSize) <= 6.5)
                            {
                                matchfail = true;
                                break;
                            }
                        }

                        if (matchfail)
                        { rest = "<span style='color:red'>FAIL</span>"; }
                        else
                        { rest = "<span style='color:green'>PASS</span>"; }
                        
                    }
                    else
                    { rest = "<span style='color:green'>PASS</span>"; }
                }
                else
                { rest = "<span style='color:red'>FAIL</span>"; }
            }

            ProbeTestData.UpdateQueryHistory(wf, x, y, compName, rest);
            foreach (var tmpx in xlist)
            { ProbeTestData.UpdateQueryHistory(wf, tmpx.ToString(), y, compName, rest); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wf= wf,
                x = "X: " + x,
                y = "Y: " + y,
                ar = "Array: 1x"+probe.APVal1,
                ap = "AP: "+probe.ApSize,
                rest = rest
            };
            return ret;
        }

        public JsonResult VcselScreenWafer()
        {
            var wflist = ProbeTestData.WaferList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wflist = wflist
            };
            return ret;
        }

        public JsonResult GetVcselScreenHistory()
        {
            var wf = Request.Form["wf"];
            var x = Request.Form["x"];
            var y = Request.Form["y"];
            if (x.Length > 0)
            { x = UT.O2I(x).ToString(); }
            if (y.Length > 0)
            { y = UT.O2I(y).ToString(); }

            var hisdata = ProbeTestData.GetQueryHistory(wf, x, y);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                hisdata = hisdata
            };
            return ret;
        }

        public ActionResult SNApertureSize()
        {
            return View();
        }

        public JsonResult SNApertureSizeData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var aplist = SNApertureSizeVM.LoadData(snlist,this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                aplist = aplist
            };
            return ret;
        }


        public ActionResult PrepareApertureSize()
        { return View(); }

        public JsonResult PrepareApertureSizeData()
        {
            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var aplist = new List<object>();
            foreach (var wf in wflist)
            {
                var stat = ProbeTestData.PrepareWaferAPSizeData(wf);
                aplist.Add(new
                {
                    wf = wf,
                    stat = stat
                });
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                aplist = aplist
            };
            return ret;
        }

        public ActionResult TestOCR()
        {
            try
            {
                //var rest = Ocr.Read(@"E:\video\die2.jpg");
                //var rtxt = rest.Text;

            }
            catch (Exception ex)
            {

            }
            return View("All");
        }

        public ActionResult TestCDF()
        {
            try
            {
                var fi = new FileInfo(@"E:\video\191006-20.D02");
                var CDF = new CDF(fi);
            }
            catch (Exception ex)
            {

            }
            return View("All");
        }

        public ActionResult FR4Binning()
        {
            return View();
        }

        public JsonResult FR4BinningData()
        {
            var marks = Request.Form["marks"];
            List<string> pnlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var sdate = "";
            var edate = "";

            if (!string.IsNullOrEmpty(Request.Form["sdate"]))
            {
                sdate = Request.Form["sdate"] + " 00:00:00";
                edate = Request.Form["edate"] + " 23:59:59";
            }
            else
            {
                sdate = "1982-05-06 10:00:00";
                edate = sdate;
            }


            var datalist = PNSNFR4Binning.GetPNSNData(pnlist, sdate, edate);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sndatalist = datalist
            };
            return ret;
        }

            //public ActionResult CheckOGPCorrection()
            //{
            //    var ogpdict = new Dictionary<string, Dictionary<string, bool>>();
            //    var ogpcorrect = new Dictionary<string, int>();

            //    var sql = "SELECT distinct [Wafer] ,[X] ,[Y] FROM [AIProjects].[dbo].[CouponData] where X<> '' and Y <> '' and timestamp < '2019-11-25 00:00:00' and timestamp > '2019-11-09 00:00:00' order by Wafer";
            //    var dbret = DBUtility.ExeOGPSqlWithRes(sql);
            //    foreach (var line in dbret)
            //    {
            //        var w = UT.O2S(line[0]);
            //        var x = UT.O2S(UT.O2I(line[1]));
            //        var y = UT.O2S(UT.O2I(line[2]));
            //        var k = x + ":::" + y;

            //        if (ogpdict.ContainsKey(w))
            //        {
            //            ogpdict[w].Add(k, true);
            //        }
            //        else
            //        {
            //            var tempdict = new Dictionary<string, bool>();
            //            tempdict.Add(k, true);
            //            ogpdict.Add(w, tempdict);
            //        }
            //    }

            //    foreach (var kv in ogpdict)
            //    {
            //        var allxydict = new Dictionary<string, bool>();
            //        var dict = new Dictionary<string, string>();
            //        dict.Add("@WaferID", kv.Key);
            //        sql = "select distinct Xcoord,Ycoord from [EngrData].[dbo].[VR_Eval_Pts_Data_Basic] where WaferID =@WaferID";
            //        dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            //        foreach (var line in dbret)
            //        {
            //            var x = UT.O2S(UT.O2I(line[0]));
            //            var y = UT.O2S(UT.O2I(line[1]));
            //            var k = x + ":::" + y;
            //            allxydict.Add(k, true);
            //        }

            //        ogpcorrect.Add(kv.Key, 0);
            //        foreach (var ogpxy in kv.Value)
            //        {
            //            if (allxydict.ContainsKey(ogpxy.Key))
            //            {
            //                ogpcorrect[kv.Key] += 1; 
            //            }
            //        }

            //    }

            //    return View("All");
            //}

        }
}