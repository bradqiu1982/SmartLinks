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


                if (validlink.Contains("wuxinpi"))
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
        
        public JsonResult DMRWIPData()
        {
            var prodline = Request.Form["prodline"];
            var dmrdata = DMRSNVM.RetrieveDMRSNData(prodline,null,null,this);
            var moduledist = ModuleDistribution(dmrdata);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                moduledist = moduledist,
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

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                moduledist = moduledist,
                dmrdata = dmrdata
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


    }
}