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

        public void RetrieveVideoTile(List<TechVideoVM> vm)
        {
            foreach (var item in vm)
            {
                var localfn = Server.MapPath(item.VPath);
                if (System.IO.File.Exists(localfn))
                {
                    var onlyname = System.IO.Path.GetFileNameWithoutExtension(localfn);

                    var mp4name = System.IO.Path.GetFileName(localfn);
                    var imgpath = localfn.Replace(mp4name, "") + onlyname + ".jpg";

                    if (System.IO.File.Exists(imgpath))
                    {
                        item.IPath = item.VPath.Replace(mp4name, "") + onlyname + ".jpg";
                    }
                    else
                    {
                        var ffMpeg2 = new NReco.VideoConverter.FFMpegConverter();
                        ffMpeg2.GetVideoThumbnail(localfn, imgpath);
                        item.IPath = item.VPath.Replace(mp4name, "") + onlyname + ".jpg";
                    }
                }//video file exist
            }//foreach
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
                RetrieveVideoTile(vm);
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

                if (ViewBag.ActiveVideo != null)
                {
                    ViewBag.ActiveVideo.TestList = VTestVM.RetrieveRandomTest(ViewBag.ActiveVideo.VID,10);
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
            MachineUserMap.AddMachineUserMap(ViewBag.compName, username);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

        [HttpPost]
        public JsonResult VideoLog()
        {
            UserAuth();

            var uselog = false;
            var cfg = CfgUtility.GetSysConfig(this);
            if (cfg.ContainsKey("UseLog4Net") && cfg["UseLog4Net"].ToUpper().Contains("TRUE")){
                uselog = true;
            }
            else {
                uselog = false;
            }

            var vid = Request.Form["vid"];
            var vname = Request.Form["vname"];
            if (!string.IsNullOrEmpty(vid))
            {
                if (uselog)
                {
                    VideoLogVM.WriteLog(ViewBag.username.ToUpper(), DetermineCompName(Request.UserHostName),
                                                    Request.Url.ToString(), "TVideoSite", "ViewVideo", vid, VideoLogType.TechVideo, VLog4NetLevel.Info, vname);
                }
                else
                {
                    VideoLogVM.WriteLog2(ViewBag.username.ToUpper(), DetermineCompName(Request.UserHostName),
                                                    Request.Url.ToString(), "TVideoSite", "ViewVideo", vid, VideoLogType.TechVideo, VLog4NetLevel.Info, vname);
                }
            }
            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
            
        }

        public ActionResult VideoLogHistory()
        {
            UserAuth();
            var history = VideoLogVM.GetVideoLog();
            ViewBag.history = history;
            return View();
        }

        public List<List<string>> RetrieveExcelTest()
        {
            var ret = new List<List<string>>();

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
                        var allvtype = ".xlsx,.xls,.xlsm,.csv,.ods,.xml";

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
                            return ExcelReader.RetrieveDataFromExcel(imgdir + srvfd, null);
                        }//end if
                    }//end if
                }

            }
            catch (Exception ex) { }
            return ret;
        }

        public string RetrieveGiftImg()
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
                        var allvtype = ".jpg,.jpeg,.png,.bmp";

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
                            return "/userfiles/docs/" + datestring + "/" + srvfd;
                        }//end if
                    }//end if
                }

            }
            catch (Exception ex) { }
            return ret;
        }



        public ActionResult UploadVideoTest()
        {
            var vtests = RetrieveExcelTest();
            var vid =  Request.Form["activevid"];            
            if (vtests.Count > 1)
            {
                var giftoffer = Request.Form["giftoffer"];
                var testnotice = Request.Form["testnotice"];
                var imgpath = RetrieveGiftImg();
                VTestVM.CleanTest(vid);

                
                for (var lidx = 1; lidx < vtests.Count; lidx++)
                {
                    if (!string.IsNullOrEmpty(vtests[lidx][0]) && !string.IsNullOrEmpty(vtests[lidx][2]))
                    {
                        var tempvm = new VTestVM();
                        tempvm.VID = vid;
                        tempvm.TestID = TechVideoVM.GetUniqKey();
                        tempvm.TestNotice = testnotice;
                        tempvm.GiftOffer = giftoffer;
                        tempvm.GiftPath = imgpath;
                        tempvm.TestContent = vtests[lidx][0];
                        tempvm.TestType = vtests[lidx][1];
                        tempvm.Answer = vtests[lidx][2];

                        var aw = new List<string>();
                        for (var idx = 0; idx < 6; idx++)
                        {
                            aw.Add(vtests[lidx][idx + 3]);
                        }
                        tempvm.OptionalAnswers = Newtonsoft.Json.JsonConvert.SerializeObject(aw);
                        tempvm.StoreTestVM();
                    }
                }//end for
            }
            var routedict = new RouteValueDictionary();
            routedict.Add("activeid",vid);
            return RedirectToAction("TechnicalVideo", "TVideoSite", routedict);
        }

        public JsonResult CheckUserAnswer()
        {
            UserAuth();

            var video_id = Request.Form["video_id"];
            var uname = Request.Form["uname"];
            var useranswers = (List<UserAnswer>)Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form["data"], (new List<UserAnswer>()).GetType());

            double score = 0;
            var answers = new List<object>();

            var uns = "";
            var cns = "";

            foreach (var item in useranswers)
            {
                var onetest = VTestVM.RetrieveTestByTestID(item.q_id);
                if (onetest.Count > 0)
                {
                    var uanswer = item.answer.Substring(0, item.answer.Length - 1);
                    var idxanswer = "";
                    var tempanswer = onetest[0].Answer.Trim().ToUpper()
                        .Replace(",","").Replace(" ","").Replace(";","").ToCharArray();

                    for (var aidx = 0; aidx < tempanswer.Length; aidx++)
                    {
                        idxanswer = idxanswer+tempanswer[aidx].ToString() + ",";
                    }
                    idxanswer = idxanswer.Substring(0, idxanswer.Length - 1);

                    uns = uns + uanswer.ToUpper()+";";
                    cns = cns + idxanswer + ";";

                    if (string.Compare(uanswer.ToUpper(), idxanswer) == 0)
                    {
                        score = score + 1;
                    }

                    answers.Add(new
                    {
                        q_id = onetest[0].TestID,
                        q_type = onetest[0].TestType,
                        answer = idxanswer,
                        uanswer = uanswer
                    });
                }
            }

            score = score / useranswers.Count * 100.0;

            VTestScore.StoreUserScore(ViewBag.compName, uname, video_id, cns, uns, score.ToString());

            var ret = new JsonResult();
            ret.Data = new
            {
                score = score,
                answers = answers
            };
            return ret;
        }

        public ActionResult VUserRank(string username)
        {
            var vm = VTestScore.RetrieveSoreWithRank(username);
            return View(vm);
        }

        public ActionResult ReceiveGift(string username)
        {
            VTestScore.UpdateUserRank(username, "-200");
            return RedirectToAction("VUserRank", "TVideoSite");
        }

    }
}