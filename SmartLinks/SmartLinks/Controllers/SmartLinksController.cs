using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;
using System.IO;

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
                    machine = compName;
                }//end if
            }//end
            else
            {
                machine = ckdict["reqmachine"];
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
                        MachineLink.UpdateFrequence(item.LinkName, item.Link, item.Logo, item.Comment, machine);
                    }
                    break;
                }
            }
            if (!string.IsNullOrEmpty(validlink))
            {
                return Redirect(validlink);
            }
            else
            {
                return RedirectToAction("All","SmartLinks");
            }

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

    }
}