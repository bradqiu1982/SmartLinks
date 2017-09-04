<%@ WebHandler Language="C#" Class="ImageUpload" %>

using System;
using System.Web;
using System.IO;

public class ImageUpload : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        try
        {
            HttpPostedFile uploads = context.Request.Files[0];
            string fn = System.IO.Path.GetFileName(uploads.FileName);
            string url = "";

            if(string.Compare(Path.GetExtension(fn),".jpg",true) == 0
                ||string.Compare(Path.GetExtension(fn),".png",true) == 0
                ||string.Compare(Path.GetExtension(fn),".gif",true) == 0
                ||string.Compare(Path.GetExtension(fn),".jpeg",true) == 0)
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = context.Server.MapPath(".") + "\\images\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                fn = Path.GetFileNameWithoutExtension(fn)+"-"+DateTime.Now.ToString("yyyyMMddHHmmss")+Path.GetExtension(fn);
                fn = fn.Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                uploads.SaveAs(imgdir + fn);
                url = "/userfiles/images/" +datestring+"/"+ fn;
            }

            context.Response.Write(url);
            
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
            try
            {

                string url = "Failed to upload file for: " + ex.ToString();
                context.Response.Write(url);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex1)
            { }
        }
    }

    public bool IsReusable {

        get {
            return false;
        }

    }



}
