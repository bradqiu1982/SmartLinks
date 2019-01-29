using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;
using System.IO;
using System.Net;
using System.Web.Routing;

namespace SmartLinks.Controllers
{
    public class FileShareController : Controller
    {
        private static string DetermineCompName(string IP)
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

        public ActionResult Welcome()
        {
            return View();
        }

        public JsonResult UpdateMachineUserName()
        {
            var compName = DetermineCompName(Request.UserHostName);
            var username = Request.Form["username"].ToUpper().Trim();
            MachineUserMap.AddMachineUserMap(compName, username);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

        public ActionResult SnapFile()
        {
            var username = MachineUserMap.GetUseNameByIP(Request.UserHostName);
            if (string.IsNullOrEmpty(username))
            { return RedirectToAction("Welcome", "FileShare"); }

            return View();
        }

        private string GetShareFileUrl()
        {
            try
            {
                foreach (string fl in Request.Files)
                {
                    if (fl != null && Request.Files[fl].ContentLength > 0)
                    {
                        var ext = Path.GetExtension(Path.GetFileName(Request.Files[fl].FileName)).ToUpper();
                        if (ext.Contains(".PPT")
                            || ext.Contains(".DOC")
                            || ext.Contains(".XLS")
                            || ext.Contains(".TXT")
                            || ext.Contains(".MSG")
                            || ext.Contains(".PDF")
                            || ext.Contains(".HTML"))
                        {
                            string fn = System.IO.Path.GetFileName(Request.Files[fl].FileName)
                            .Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                            string datestring = DateTime.Now.ToString("yyyyMMdd");
                            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";
                            if (!Directory.Exists(imgdir))
                            {
                                Directory.CreateDirectory(imgdir);
                            }
                            fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                            Request.Files[fl].SaveAs(imgdir + fn);

                            var wholefn = imgdir + fn;

                            if (ext.Contains(".PDF"))
                            {
                                if (System.IO.File.Exists(imgdir + Path.GetFileNameWithoutExtension(fn) + ".pdf"))
                                {
                                    return "/userfiles/docs/" + datestring + "/" + Path.GetFileNameWithoutExtension(fn) + ".pdf";
                                }
                            }
                            else if (ext.Contains(".HTML"))
                            {
                                if (System.IO.File.Exists(imgdir + Path.GetFileNameWithoutExtension(fn) + ".html"))
                                {
                                    return "/userfiles/docs/" + datestring + "/" + Path.GetFileNameWithoutExtension(fn) + ".html";
                                }
                            }
                            else if (ext.Contains(".XLS"))
                            {
                                if (OfficeConverter.ExcelConverter(wholefn))
                                {
                                    if (System.IO.File.Exists(imgdir + Path.GetFileNameWithoutExtension(fn) + ".html"))
                                    {
                                        return "/userfiles/docs/" + datestring + "/" + Path.GetFileNameWithoutExtension(fn) + ".html";
                                    }
                                }
                            }
                            else if (ext.Contains(".DOC") || ext.Contains(".TXT"))
                            {
                                if (OfficeConverter.DocConverter(wholefn))
                                {
                                    if (System.IO.File.Exists(imgdir + Path.GetFileNameWithoutExtension(fn) + ".html"))
                                    {
                                        return "/userfiles/docs/" + datestring + "/" + Path.GetFileNameWithoutExtension(fn) + ".html";
                                    }
                                }
                            }
                            else if (ext.Contains(".MSG"))
                            {
                                if (OfficeConverter.OutlookConverter(wholefn))
                                {
                                    if (System.IO.File.Exists(imgdir + Path.GetFileNameWithoutExtension(fn) + ".html"))
                                    {
                                        return "/userfiles/docs/" + datestring + "/" + Path.GetFileNameWithoutExtension(fn) + ".html";
                                    }
                                }
                            }
                            else if (ext.Contains(".PPT"))
                            {
                                if (OfficeConverter.PPTConverter(wholefn))
                                {
                                    if (System.IO.File.Exists(imgdir + Path.GetFileNameWithoutExtension(fn) + ".pdf"))
                                    {
                                        return "/userfiles/docs/" + datestring + "/" + Path.GetFileNameWithoutExtension(fn) + ".pdf";
                                    }
                                }
                            }
                        }//end if
                    }//end if
                }//end foreach
            }//end try
            catch (Exception ex)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        private void SendShareDocEmail(List<string> towho,string owner, string filename,string docid)
        {
            var routevalue = new RouteValueDictionary();
            routevalue.Add("docid", docid);
            string scheme = Url.RequestContext.HttpContext.Request.Url.Scheme;
            string url = Url.Action("ReviewDocument", "FileShare", routevalue, scheme);
            var netcomputername = EmailUtility.RetrieveCurrentMachineName();
            url = url.Replace("//localhost", "//" + netcomputername);
            var econtent = "Hi Guys,\r\n\r\n"
                +"You are shared file: "+filename +" by "+owner+".\r\n\r\n"
                +"Click Below Link: "+url+"\r\n\r\n"
                + "Thanks\r\n\r\nEngineering System";

            EmailUtility.SendEmail(this, "Document Share From Engineering System", towho, econtent);
            new System.Threading.ManualResetEvent(false).WaitOne(500);
        }

        [HttpPost, ActionName("SnapFile")]
        public ActionResult SnapFilePost()
        {
            var url = GetShareFileUrl();
            if (!string.IsNullOrEmpty(url))
            {
                var owener = MachineUserMap.GetUseNameByIP(Request.UserHostName);
                var sharetolist = Request.Form["shareto"].Split(new string[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var docid = Guid.NewGuid().ToString("N");
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                foreach (var s in sharetolist)
                {
                    SnapFileVM.StoreData(docid, owener,s.Trim().ToUpper(), url,now);
                }

                var sharefilename = Path.GetFileNameWithoutExtension(Server.MapPath("~" + url));
                var towho = new List<string>();
                foreach (var s in sharetolist)
                { towho.Add(s + "@finisar.com"); }

                SendShareDocEmail(towho,owener, sharefilename,docid);
            }
            return RedirectToAction("SnapFile", "FileShare");
        }

        public ActionResult ReviewDocument(string docid)
        {
            var username = MachineUserMap.GetUseNameByIP(Request.UserHostName);
            if (string.IsNullOrEmpty(username))
            { return View("Welcome"); }

            if (string.IsNullOrEmpty(docid))
            { return View("ReviewError"); }

            var filelist = SnapFileVM.RetrieveFileByID(docid, username);
            if (filelist.Count == 0)
            { return View("ReviewError"); }

            var url = filelist[0].FileAddr;
            ViewBag.DocTitle = Path.GetFileNameWithoutExtension(Server.MapPath("~"+url));
            ViewBag.Url = url;
            return View();
        }

        public JsonResult ShareToList()
        {
            var dict = MachineUserMap.RetrieveUserMap();
            var ret = new JsonResult();
            ret.Data = new
            {
                sharetolist = dict.Values.ToList()
            };
            return ret;
        }

        public JsonResult RetrieveReviewDocuments()
        {
            var username = MachineUserMap.GetUseNameByIP(Request.UserHostName);
            var doclist = SnapFileVM.RetrieveFileListByShareTo(username);
            foreach (var item in doclist)
            {
                item.FileAddr = Path.GetFileNameWithoutExtension(Server.MapPath("~" + item.FileAddr));
            }
            var ret = new JsonResult();
            ret.Data = new { doclist = doclist };
            return ret;
        }

        public JsonResult RetrieveShareDocuments()
        {
            var username = MachineUserMap.GetUseNameByIP(Request.UserHostName);
            var doclist = SnapFileVM.RetrieveFileListByOwner(username);
            foreach (var item in doclist)
            {
                item.FileAddr = Path.GetFileNameWithoutExtension(Server.MapPath("~" + item.FileAddr));
            }
            var ret = new JsonResult();
            ret.Data = new { doclist = doclist };
            return ret;
        }

        public JsonResult RemoveDocument()
        {
            var docid = Request.Form["docid"];
            SnapFileVM.RemoveFileByID(docid);
            var ret = new JsonResult();
            ret.Data = new
            {
                success = true
            };
            return ret;
        }
    }
}