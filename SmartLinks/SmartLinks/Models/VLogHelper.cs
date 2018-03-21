using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SmartLinks.Models
{
    public class VLogHelper
    {
        public static void WriteLog(string logContent, Dictionary<string, string> cusProperties)
        {
            foreach(KeyValuePair<string, string> property in cusProperties)
            {
                log4net.LogicalThreadContext.Properties[property.Key] = property.Value;
            }
            WriteLog(null, logContent, VLog4NetLevel.Error);
        }
        
        public static void WriteLog(string logContent, VLog4NetLevel log4Level, Dictionary<string, string> cusProperties)
        {
            foreach (KeyValuePair<string, string> property in cusProperties)
            {
                log4net.LogicalThreadContext.Properties[property.Key] = property.Value;
            }
            WriteLog(null, logContent, log4Level);
        }
        
        public static void WriteLog(Type type, string logContent, VLog4NetLevel log4Level)
        {
            ILog log = type == null ? LogManager.GetLogger("") : LogManager.GetLogger(type);

            switch (log4Level)
            {
                case VLog4NetLevel.Warn:
                    log.Warn(logContent);
                    break;
                case VLog4NetLevel.Debug:
                    log.Debug(logContent);
                    break;
                case VLog4NetLevel.Info:
                    log.Info(logContent);
                    break;
                case VLog4NetLevel.Fatal:
                    log.Fatal(logContent);
                    break;
                case VLog4NetLevel.Error:
                    log.Error(logContent);
                    break;
            }
        }

    }

    public enum VLog4NetLevel
    {
        [Description("Warning")]
        Warn = 1,
        [Description("Debug")]
        Debug = 2,
        [Description("Info")]
        Info = 3,
        [Description("Fatal")]
        Fatal = 4,
        [Description("Error")]
        Error = 5
    }
    
    public class VideoLogVM
    {
        public string ID { set; get; }
        public string UserName { set; get; }
        public string Machine { set; get; }
        public string Url { set; get; }
        public string OperateModule { set; get; }
        public string Operate { set; get; }
        public string VideoID { set; get; }
        public int LogType { set; get; }
        public string LogLevel { set; get; }
        public string Message { set; get; }
        public string CreateAt { set; get; }

        public static void WriteLog(string uName, string machine, string url, 
                string oModule, string op, string vId, int lType, VLog4NetLevel lLevel, string msg)
        {
            var ret = GetVideoLog(vId, "", uName);
            if (ret.Count > 0)
            {
                return;
            }

            var dic = new Dictionary<string, string>();
            dic.Add("uname", uName);
            dic.Add("machine", machine);
            dic.Add("url", url);
            dic.Add("module", oModule);
            dic.Add("operate", op);
            dic.Add("vid", vId);
            dic.Add("ltype", lType.ToString());
            VLogHelper.WriteLog(msg, lLevel, dic);
        }

        public static List<VideoLogVM> GetVideoLog(string vid = "", string vname = "",string uname = "")
        {
            var sql = @"select ID, VideoID, Message, UserName, CreateAt from VideoLog where 1 = 1";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(vid))
            {
                sql += " and VideoID = @vid";
                param.Add("@vid", vid);
            }
            if (!string.IsNullOrEmpty(vname))
            {
                sql += " and Message = @vanme";
                param.Add("@vname", vname);
            }
            if (!string.IsNullOrEmpty(uname))
            {
                sql += " and UserName = @UserName";
                param.Add("@UserName", uname);
            }
            sql += " order by CreateAt DESC ";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new List<VideoLogVM>();
            if (dbret.Count > 0)
            {
                foreach(var line in dbret)
                {
                    var tmp = new VideoLogVM();
                    tmp.ID = Convert.ToString(line[0]);
                    tmp.VideoID = Convert.ToString(line[1]);
                    tmp.Message = Convert.ToString(line[2]);
                    tmp.UserName = Convert.ToString(line[3]);
                    tmp.CreateAt = Convert.ToString(line[4]);
                    res.Add(tmp);
                }
            }
            return res;
        }
    }

    public class VideoLogType
    {
        public static int Default = 0;
        public static int TechVideo = 1;
    }

}
