using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartLinks.Models;

namespace SmartLinks.Controllers
{
    public class PWDNotesController : Controller
    {
        public ActionResult DoorCode()
        {
            return View();
        }

        public ActionResult SimpleNotes(string doorcode)
        {
            if (string.IsNullOrEmpty(doorcode))
            {
                return RedirectToAction("DoorCode", "PWDNotes");
            }
            ViewBag.doorcode = doorcode;
            return View();
        }

        public JsonResult SimpleNotesData()
        {
            var doorcode = Request.Form["doorcode"];
            var noteslist = SimpleNoteVM.GetNote(doorcode);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                noteslist = noteslist
            };
            return ret;
        }

        public JsonResult SimpleNotesUpload()
        {
            var doorcode = Request.Form["doorcode"];
            var bnote = Request.Form["note"];

            string dummyData = bnote.Trim().Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
            var bytes = Convert.FromBase64String(dummyData);
            var note = System.Text.Encoding.UTF8.GetString(bytes);
            SimpleNoteVM.StoreNote(doorcode, note);

            var noteslist = SimpleNoteVM.GetNote(doorcode);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                noteslist = noteslist
            };
            return ret;
        }

    }
}