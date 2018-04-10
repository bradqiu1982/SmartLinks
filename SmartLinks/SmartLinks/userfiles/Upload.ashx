 <%@ WebHandler Language="C#" Class="Upload" %>

using System;
using System.Web;
using System.IO;
using SmartLinks.Models;

public class Upload : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        try
        {
            HttpPostedFile uploads = context.Request.Files["upload"];
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
            else
            {
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = context.Server.MapPath(".") + "\\docs\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                fn = Path.GetFileNameWithoutExtension(fn)+"-"+DateTime.Now.ToString("yyyyMMddHHmmss")+Path.GetExtension(fn);
                fn = fn.Replace(" ", "_").Replace("#", "").Replace("'", "")
                            .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");

                uploads.SaveAs(imgdir + fn);
                url = "/userfiles/docs/" +datestring+"/"+ fn;             
            }

            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            string txt = "{\"fileName\":\""+fn+"\",\"uploaded\":1,\"url\":\""+url+"\"}";

            context.Response.Write(txt);
            
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
        }
    }

    public bool IsReusable {

        get {

            return false;

        }

    }



}