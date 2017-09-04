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

        // GET: SmartLinks
        public ActionResult All()
        {
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
            }//end

            var vm = LinkVM.RetrieveLinks();

            return View(vm);
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
            return RedirectToAction("SmartLinks", "All");
        }

        public ActionResult RedirectToLink(string linkname)
        {
            var vm = LinkVM.RetrieveLinks();
            var validlink = string.Empty;
            foreach (var item in vm)
            {
                if (string.Compare(linkname, item.LinkName) == 0)
                {
                    validlink = item.Link;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(validlink))
            {
                return Redirect(validlink);
            }
            else
            {
                return RedirectToAction("SmartLinks","All");
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
                
            }

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

    }
}