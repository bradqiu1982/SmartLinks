using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Web.Routing;
using System.Collections.Specialized;
using SmartLinks.Models;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Web.Caching;

namespace SmartLinks.Controllers
{
    public class TVideoSiteController : Controller
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
            ViewBag.compName = compName.ToUpper();
            var glbcfg = CfgUtility.GetSysConfig(this);

            var usermap = MachineUserMap.RetrieveUserMap();
            
            if (usermap.ContainsKey(ViewBag.compName))
            {
                ViewBag.username = usermap[ViewBag.compName].Trim().ToUpper();
                if (glbcfg["VideoAdmin"].ToUpper().Contains(ViewBag.username))
                {
                    ViewBag.VideoAdmin = true;
                }
            }
            else
            {
                ViewBag.username = ViewBag.compName;
            }
        }

        public ActionResult TechnicalVideo(string activeid, string searchkey)
        {
            UserAuth();

            if (string.Compare(ViewBag.username, ViewBag.compName) == 0)
            {
                return RedirectToAction("Welcome");
            }

            var vm = TechVideoVM.RetrieveVideo(searchkey);
            if (vm.Count > 0)
            {
                if (string.IsNullOrEmpty(activeid))
                {
                    ViewBag.ActiveVideo = vm[0];
                }
                else
                {
                    foreach (var item in vm)
                    {
                        if (string.Compare(activeid, item.VID) == 0)
                        {
                            ViewBag.ActiveVideo = item;

                        }
                    }
                    if (ViewBag.ActiveVideo == null)
                    {
                        ViewBag.ActiveVideo = vm[0];
                    }
                }
            }

            return View(vm);
        }

        public string RetrieveUploadVideo()
        {
            var ret = "";

            try
            {
                foreach (string fl in Request.Files)
                {
                    if (fl != null && Request.Files[fl].ContentLength > 0)
                    {
                        string fn = Path.GetFileName(Request.Files[fl].FileName)
                            .Replace(" ", "_").Replace("#", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                        var ext = Path.GetExtension(fn).ToLower();
                        var allvtype = ".mp4,.mp3,.h264,.wmv,.wav,.avi,.flv,.mov,.mkv,.webm,.ogg,.mov,.mpg";

                        if (allvtype.Contains(ext))
                        {
                            string datestring = DateTime.Now.ToString("yyyyMMdd");
                            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                            if (!Directory.Exists(imgdir))
                            {
                                Directory.CreateDirectory(imgdir);
                            }

                            var srvfd = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                            Request.Files[fl].SaveAs(imgdir + srvfd);

                            if (!ext.Contains("mp4"))
                            {
                                var mp4name = Path.GetFileNameWithoutExtension(srvfd) + ".mp4";
                                var mp4path = imgdir + mp4name;
                                var ffMpeg2 = new NReco.VideoConverter.FFMpegConverter();
                                ffMpeg2.ConvertMedia(imgdir + srvfd, mp4path, NReco.VideoConverter.Format.mp4);

                                try { System.IO.File.Delete(imgdir + srvfd); } catch (Exception ex) { }
                                return "/userfiles/docs/" + datestring + "/" + mp4name;
                            }

                            return "/userfiles/docs/" + datestring + "/" + srvfd;

                        }//end if
                    }//end if
                }

            }
            catch (Exception ex) { }
            return ret;
        }

        public ActionResult UploadTechnicalVideo()
        {
            UserAuth();

            var mp4url = RetrieveUploadVideo();
            var vsubject = Request.Form["vsubject"];
            var vdesc = Request.Form["vdesc"];
            if (!string.IsNullOrEmpty(mp4url) && !string.IsNullOrEmpty(vsubject))
            {
                TechVideoVM.StoreVideo(vsubject, vdesc, mp4url, ViewBag.username);
            }
            return RedirectToAction("TechnicalVideo", "TVideoSite");
        }

        public ActionResult Welcome()
        {
            return View();
        }

        public JsonResult UpdateMachineUserName()
        {
            UserAuth();
            var username = Request.Form["username"].ToUpper().Trim();
            MachineUserMap.UpdateMachineUserMap(ViewBag.compName, username);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

    }
}