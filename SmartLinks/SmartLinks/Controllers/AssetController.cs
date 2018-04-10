using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;
using System.Text;
using System.IO;
using System.Net;
using System.Reflection;

namespace SmartLinks.Controllers
{
    public class AssetController : Controller
    {
        // GET: Asset
        public ActionResult Index(int p = 1, string cn = "", string name = "", string status = "")
        {
            var psize = 10;
            var asset_list = AssetVM.GetAssetList(cn, name, status);
            ViewBag.total = asset_list.Count;
            ViewBag.data = asset_list.Skip((p - 1) * psize).Take(psize);
            ViewBag.showColumn = AssetVM.AssetShowProperty();
            ViewBag.PageNo = p;
            ViewBag.TotalPage = Math.Ceiling(asset_list.Count / psize + 0.0);
            ViewBag.cn = cn;
            ViewBag.name = name;
            ViewBag.status = status;
            return View();
        }

        public ActionResult MyAsset(int p = 1, string key = "")
        {
            ViewBag.PageNo = p;
            ViewBag.TotalPage = 2;
            ViewBag.keywords = key;
            return View();
        }

        public ActionResult BuyRequest(int p = 1, string key = "")
        {
            var psize = 10;
            var buy_list = AssetBuyInfoVM.GetAssetBuyInfo(key);
            ViewBag.total = buy_list.Count;
            ViewBag.data = buy_list.Skip((p - 1) * psize).Take(psize);
            ViewBag.PageNo = p;
            ViewBag.TotalPage = Math.Ceiling(buy_list.Count / psize + 0.0);
            ViewBag.keywords = key;

            return View();
        }

        public ActionResult BorrowRequest(int p = 1, string key = "", string status = "")
        {
            var psize = 10;
            var borrow_list = AssetBorrowHistoryVM.GetBorrowList(key, status);
            ViewBag.total = borrow_list.Count;
            ViewBag.data = borrow_list.Skip((p - 1) * psize).Take(psize);

            ViewBag.PageNo = p;
            ViewBag.TotalPage = Math.Ceiling(borrow_list.Count / psize + 0.0);
            ViewBag.keywords = key;

            return View();
        }

        [HttpPost]
        public JsonResult GetAutoCompleteData()
        {
            var res = new JsonResult();
            res.Data = new { success = true, users = UserVM.RetrieveAllUser(), projects = ProjectVM.RetrieveAllProjectKey(), assets = AssetVM.GetValidAssets() };

            return res;
        }

        [HttpPost]
        public JsonResult AddBorrowRequest()
        {
            var borrow_user = Request.Form["borrow_user"];
            var pro_no = Request.Form["pro_no"];
            var islong = Request.Form["islong"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var comment = Request.Form["comment"];
            var assets = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form["assets"], (new List<string>()).GetType());
            var borrowlist = new List<AssetBorrowHistoryVM>();
            foreach(var item in assets)
            {
                var tmp = new AssetBorrowHistoryVM();
                tmp.RequestID = DateTime.Now.ToString("yyyyMMddHHmmss");
                tmp.AssetID = item;
                tmp.BorrowUser = borrow_user;
                tmp.ProjectNo = pro_no;
                tmp.IsLong = islong;
                tmp.StartDate = sdate;
                tmp.EndDate = edate;
                tmp.BorrowComment = comment;
                borrowlist.Add(tmp);
            }
            AssetBorrowHistoryVM.NewBorrow(borrowlist);

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }

        [HttpPost]
        public JsonResult UpdateBuyRequest()
        {
            var res = new JsonResult();
            var buy_ids = Request.Form["buy_ids[]"].Split(new char[] { ',' });
            foreach (var id in buy_ids)
            {
                var tmp = new AssetBuyInfoVM();
                tmp.ID = id;
                tmp.Status = BuyStatus.Buyed.ToString();

                AssetBuyInfoVM.UpdateBuyRequest(tmp);
            }

            res.Data = new { success = true };

            return res;
        }

        [HttpPost]
        public JsonResult DeleteBuyRequest()
        {
            var res = new JsonResult();
            var del_ids = Request.Form["del_ids[]"].Split(new char[] { ',' });
            foreach(var id in del_ids)
            {
                var tmp = new AssetBuyInfoVM();
                tmp.ID = id;
                tmp.Status = BuyStatus.Invalid.ToString();

                AssetBuyInfoVM.UpdateBuyRequest(tmp);
            }

            res.Data = new { success = true };

            return res;
        }

        [HttpPost]
        public JsonResult AddBuyRequest()
        {
            var buy = new AssetBuyInfoVM();
            buy.ID = Request.Form["id"];
            buy.EngName = Request.Form["eName"];
            buy.ChName = Request.Form["cName"];
            buy.UnitPrice = Request.Form["price"];
            buy.Brand = Request.Form["brand"];
            buy.Model = Request.Form["model"];
            buy.OriginCountry = Request.Form["country"];
            buy.Purpose = Request.Form["purpose"];
            buy.Functions = Request.Form["funcs"];
            buy.WorkPrinciple = Request.Form["principle"];
            buy.CorporatePurposes = Request.Form["corpurpose"];
            buy.Status = BuyStatus.Request.ToString();

            foreach (string fl in Request.Files)
            {
                if (fl != null && Request.Files[fl].ContentLength > 0)
                {
                    var fn = System.IO.Path.GetFileName(Request.Files[fl].FileName);
                    if (string.Compare(Path.GetExtension(fn), ".jpg", true) == 0
                        || string.Compare(Path.GetExtension(fn), ".png", true) == 0
                        || string.Compare(Path.GetExtension(fn), ".gif", true) == 0
                        || string.Compare(Path.GetExtension(fn), ".jpeg", true) == 0)
                    {
                        string datestring = DateTime.Now.ToString("yyyyMMdd");
                        string imgdir = Server.MapPath("~/userfiles") + "\\images\\" + datestring + "\\";

                        if (!Directory.Exists(imgdir))
                        {
                            Directory.CreateDirectory(imgdir);
                        }

                        fn = System.IO.Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + System.IO.Path.GetExtension(fn);
                        Request.Files[fl].SaveAs(imgdir + fn);

                        buy.Picture = "/userfiles/images/" + datestring + "/" + fn;
                    }
                }
            }

            if (!string.IsNullOrEmpty(buy.ID))
            {
                AssetBuyInfoVM.UpdateBuyRequest(buy);
            }
            else
            {
                AssetBuyInfoVM.AddBuyRequest(buy);
            }

            var res = new JsonResult();
            res.Data = new { success = true };

            return res;
        }

        [HttpPost]
        public JsonResult UploadAsset()
        {
            var file_path = Request.Form["path"];
            List<List<string>> data = ExcelReader.RetrieveDataFromExcel(Server.MapPath(file_path), null);
            
            data.RemoveAt(0);
            if(data.Count > 0)
            {
                foreach(var item in data)
                {
                    AssetVM.SaveAssets(item);
                }
            }

            var res = new JsonResult();
            res.Data = new { success = true};

            return res;
        }

        [HttpPost]
        public JsonResult BorrowReturn()
        {
            var id = Request.Form["id"];
            var assetid = Request.Form["assetid"];
            var comment = Request.Form["comment"];
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var borrow_tmp = new AssetBorrowHistoryVM();
            borrow_tmp.ID = id;
            borrow_tmp.AssetID = assetid;
            borrow_tmp.IsReturn = BorrowStatus.Returned.ToString();
            borrow_tmp.ReturnComment = comment;
            borrow_tmp.ReturnAt = now;
            AssetBorrowHistoryVM.UpdateBorrow(borrow_tmp);

            var res = new JsonResult();
            res.Data = new { success = true };

            return res;
        }

        [HttpPost]
        public JsonResult GetBuyInfo()
        {
            var id = Request.Form["id"];
            var buy_info = AssetBuyInfoVM.GetAssetBuyInfo("", id);
            var res = new JsonResult();
            if (buy_info.Count > 0)
            {
                res.Data = new { success = true, data = buy_info[0] };
            }
            else
            {
                res.Data = new { success = false};
            }

            return res;
        }

        [HttpPost]
        public JsonResult AddCheck()
        {
            var assetid = Request.Form["assetid"];
            var comment = Request.Form["comment"];

            var exist = AssetCheckHistoryVM.GetCheckInfo(assetid);
            var res = new JsonResult();
            if (!string.IsNullOrEmpty(exist.ID))
            {
                res.Data = new { success = false };
            }
            else
            {
                var check = new AssetCheckHistoryVM();
                check.AssetID = assetid;
                check.Status = CheckStatus.Valid.ToString();
                check.Comment = comment;
                AssetCheckHistoryVM.AddCheckHistory(check);
                res.Data = new { success = true };
            }

            return res;
        }

        [HttpPost]
        public JsonResult UpdateCheck()
        {
            var id = Request.Form["id"];
            var check = new AssetCheckHistoryVM();
            check.ID = id;
            check.Status = CheckStatus.Complete.ToString();
            AssetCheckHistoryVM.UpdateCheckHistory(check);

            var res = new JsonResult();
            res.Data = new { success = true };
            return res;
        }
    }
}