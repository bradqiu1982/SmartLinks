using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

namespace SmartLinks.Models
{
    public class TraceViewVM
    {
        public static string RetrieveTimeFromTraceViewName(string filename)
        {
            var tracestrs = filename.Split(new string[] { "_TRACEVIEW_" }, StringSplitOptions.RemoveEmptyEntries);
            if (tracestrs.Length > 1)
            {
                var dutstrs = tracestrs[1].Split(new string[] { "_DUT" }, StringSplitOptions.RemoveEmptyEntries);
                var timestr = dutstrs[0].Replace("_", "").Replace(" ", "");
                return timestr.Substring(0, 4) + "-" + timestr.Substring(4, 2) + "-" + timestr.Substring(6, 2) + " " + timestr.Substring(8, 2) + ":" + timestr.Substring(10, 2) + ":" + timestr.Substring(12, 2);
            }
            return null;
        }

        private static List<TraceViewData> RetrieveTestDataFromTraceView_DUTORDERED(bool WithWildMatch, string filename, string testcase, string datafield)
        {
            var ret = new List<TraceViewData>();

            if (!File.Exists(filename))
                return ret;

            var allline = System.IO.File.ReadAllLines(filename);
            var crttemp = 25.0;
            var crtch = 0;

            var entertestcase = false;

            foreach (var line in allline)
            {
                var uline = line.ToUpper();

                if ((string.Compare(testcase, "ALL", true) == 0)
                    || (uline.Contains(testcase.ToUpper()) && uline.Contains("STARTED")))
                {
                    entertestcase = true;
                    if (uline.Contains("C ---") && uline.Contains("@"))
                    {
                        var head = uline.Split(new string[] { "C ---" }, StringSplitOptions.RemoveEmptyEntries);
                        var tempstrs = head[0].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempstrs.Length > 1)
                        {
                            try
                            {
                                crttemp = Convert.ToDouble(tempstrs[1].Trim());
                            }
                            catch (Exception ex) { }
                        }
                    }
                }//end if

                if (uline.Contains(testcase.ToUpper()) && uline.Contains("COMPLETED"))
                {
                    entertestcase = false;
                }

                if (string.Compare(testcase, "ALL", true) == 0)
                {
                    entertestcase = true;
                }

                if (WithWildMatch)
                {
                    if (entertestcase && uline.Contains(datafield.ToUpper()))
                    {
                        var fields = uline.Split(new string[] { datafield.ToUpper() }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            try
                            { crtch = Convert.ToInt32(chstr); }
                            catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0].Replace("<", "").Replace(">", "");
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                else
                {
                    if (entertestcase && line.Contains("] --- ") && uline.Contains(" " + datafield.ToUpper() + " "))
                    {
                        var fields = uline.Split(new string[] { " " + datafield.ToUpper() + " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            try
                            { crtch = Convert.ToInt32(chstr); }
                            catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var tempval = Convert.ToDouble(tmpvals[0]);

                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0];
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }//end if
                    }//en if
                }


            }//end foreach
            return ret;
        }

        private static List<TraceViewData> RetrieveTestDataFromTraceView_DUTx(bool WithWildMatch, string filename, string testcase, string datafield)
        {
            var ret = new List<TraceViewData>();

            if (!File.Exists(filename))
                return ret;

            var allline = System.IO.File.ReadAllLines(filename);
            var crttemp = 25.0;
            var crtch = 0;

            var entertestcase = false;

            foreach (var line in allline)
            {
                var uline = line.ToUpper();

                if ((string.Compare(testcase, "ALL", true) == 0)
                    || (uline.Contains(testcase.ToUpper()) && uline.Contains("STARTED")))
                {
                    entertestcase = true;
                    if (uline.Contains("C ---") && uline.Contains("@"))
                    {
                        var head = uline.Split(new string[] { "C ---" }, StringSplitOptions.RemoveEmptyEntries);
                        var tempstrs = head[0].Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempstrs.Length > 1)
                        {
                            try
                            {
                                crttemp = Convert.ToDouble(tempstrs[1].Trim());
                            }
                            catch (Exception ex) { }
                        }
                    }
                }//end if

                if (uline.Contains(testcase.ToUpper()) && uline.Contains("COMPLETED"))
                {
                    entertestcase = false;
                }

                if (string.Compare(testcase, "ALL", true) == 0)
                {
                    entertestcase = true;
                }

                if (WithWildMatch)
                {
                    if (entertestcase && uline.Contains(datafield.ToUpper()))
                    {
                        var fields = uline.Split(new string[] { datafield.ToUpper() }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            try
                            { crtch = Convert.ToInt32(chstr); }
                            catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0].Replace("<", "").Replace(">", "");
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                else
                {
                    if (entertestcase && line.Contains("--- ") && uline.Contains(" " + datafield.ToUpper() + " "))
                    {
                        var fields = uline.Split(new string[] { " " + datafield.ToUpper() + " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (fields.Length > 1)
                        {
                            //var chstr = fields[0].Replace("\t", "").Replace(" ", "").Replace("[", "").Replace("]", "").Replace("-", "");
                            //try
                            //{ crtch = Convert.ToInt32(chstr); }
                            //catch (Exception ex) { }

                            var tmpvaluestr = fields[1].Trim();
                            var tmpvals = tmpvaluestr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!tmpvals[0].Contains("<NM>"))
                                {
                                    var tempval = Convert.ToDouble(tmpvals[0]);
                                    var temptraceviewdata = new TraceViewData();
                                    temptraceviewdata.Temp = crttemp;
                                    temptraceviewdata.CH = crtch;
                                    temptraceviewdata.Value = tmpvals[0];
                                    ret.Add(temptraceviewdata);
                                }
                            }
                            catch (Exception ex) { }
                        }//end if
                    }//en if
                }

            }//end foreach
            return ret;
        }


        public static List<TraceViewData> RetrieveTestDataFromTraceView(bool WithWildMatch, string filename, string testcase, string datafield)
        {
            if (filename.ToUpper().Contains("_DUTORDERED_"))
            {
                return RetrieveTestDataFromTraceView_DUTORDERED(WithWildMatch, filename, testcase, datafield);
            }
            else
            {
                return RetrieveTestDataFromTraceView_DUTx(WithWildMatch, filename, testcase, datafield);
            }
        }

        public static List<string> LoadTraceView2Local(string tester, string sn, string whichtest, string dbtimestr, Controller ctrl)
        {
            var ret = new List<string>();
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var traceviewscrfolder = syscfgdict["TRACEVIEWFOLDER"] + "\\" + tester;
                var allsrcfiles = DirectoryEnumerateFiles(ctrl, traceviewscrfolder);

                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\TraceView\\";
                if (!DirectoryExists(ctrl, imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                foreach (var srcf in allsrcfiles)
                {
                    var filename = Path.GetFileName(srcf).ToUpper();

                    if (filename.Contains(sn.ToUpper())
                        && filename.Contains(whichtest.ToUpper())
                        && filename.Contains("_DUTORDERED_")
                        && filename.Contains("_TRACEVIEW_"))
                    {
                        var traceviewtimestr = RetrieveTimeFromTraceViewName(filename);
                        if (traceviewtimestr == null)
                            continue;

                        try
                        {
                            var traceviewtime = DateTime.Parse(traceviewtimestr);
                            var dbtime = DateTime.Parse(dbtimestr);
                            if (traceviewtime > dbtime.AddSeconds(-5) && traceviewtime < dbtime.AddSeconds(5))
                            {
                                logthdinfo("\r\nStart to copy file: " + srcf);
                                var desfile = imgdir + filename;
                                FileCopy(ctrl, srcf, desfile, true);
                                if (FileExist(ctrl, desfile))
                                {
                                    logthdinfo("try to add data from file: " + desfile);
                                    ret.Add(desfile);
                                }//copied file exist
                            }
                        }
                        catch (Exception ex)
                        {
                            logthdinfo("LoadTraceView2Local Exception: " + ex.Message);
                        }
                    }//end if
                }//end foreach

                if (ret.Count == 0)
                {
                    foreach (var srcf in allsrcfiles)
                    {
                        var filename = Path.GetFileName(srcf).ToUpper();

                        if (filename.Contains(sn.ToUpper())
                            && filename.Contains(whichtest.ToUpper())
                            && filename.Contains("_DUT")
                            && filename.Contains("_TRACEVIEW_"))
                        {
                            var traceviewtimestr = RetrieveTimeFromTraceViewName(filename);
                            if (traceviewtimestr == null)
                                continue;

                            try
                            {
                                var traceviewtime = DateTime.Parse(traceviewtimestr);
                                var dbtime = DateTime.Parse(dbtimestr);
                                if (traceviewtime > dbtime.AddSeconds(-5) && traceviewtime < dbtime.AddSeconds(5))
                                {
                                    logthdinfo("\r\nStart to copy file: " + srcf);
                                    var desfile = imgdir + filename;
                                    FileCopy(ctrl, srcf, desfile, true);
                                    if (FileExist(ctrl, desfile))
                                    {
                                        logthdinfo("try to add data from file: " + desfile);
                                        ret.Add(desfile);
                                    }//copied file exist
                                }
                            }
                            catch (Exception ex)
                            {
                                logthdinfo("LoadTraceView2Local Exception: " + ex.Message);
                            }
                        }//end if
                    }//end foreach
                }//end if ret == 0
            }
            catch (Exception e) { }


            return ret;
        }

        public static List<string> LoadAllTraceView2Local(string tester, string sn, string whichtest, Controller ctrl)
        {
            var ret = new List<string>();
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var traceviewscrfolder = syscfgdict["TRACEVIEWFOLDER"] + "\\" + tester;
                var traceviewscrfolder2 = syscfgdict["TRACEVIEWFOLDER"] + "\\" + tester + "\\" + tester;
                var allsrcfiles = DirectoryEnumerateFiles(ctrl, traceviewscrfolder);
                var allsrcfiles2 = DirectoryEnumerateFiles(ctrl, traceviewscrfolder2);
                allsrcfiles.AddRange(allsrcfiles2);

                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\docs\\TraceView\\";
                if (!DirectoryExists(ctrl, imgdir))
                {
                    Directory.CreateDirectory(imgdir);
                }

                foreach (var srcf in allsrcfiles)
                {
                    var filename = Path.GetFileName(srcf).ToUpper();

                    if (filename.Contains(sn.ToUpper())
                        && filename.Contains(whichtest.ToUpper())
                        && filename.Contains("_DUTORDERED_")
                        && filename.Contains("_TRACEVIEW_"))
                    {
                        var traceviewtimestr = RetrieveTimeFromTraceViewName(filename);
                        if (traceviewtimestr == null)
                            continue;

                        try
                        {
                            //var traceviewtime = DateTime.Parse(traceviewtimestr);
                            //var dbtime = DateTime.Parse(dbtimestr);
                            //if (traceviewtime > dbtime.AddSeconds(-5) && traceviewtime < dbtime.AddSeconds(5))
                            //{
                                logthdinfo("\r\nStart to copy file: " + srcf);
                                var desfile = imgdir + filename;
                                FileCopy(ctrl, srcf, desfile, true);
                                if (FileExist(ctrl, desfile))
                                {
                                    logthdinfo("try to add data from file: " + desfile);
                                    ret.Add(desfile);
                                }//copied file exist
                            //}
                        }
                        catch (Exception ex)
                        {
                            logthdinfo("LoadTraceView2Local Exception: " + ex.Message);
                        }
                    }//end if
                }//end foreach

                if (ret.Count == 0)
                {
                    foreach (var srcf in allsrcfiles)
                    {
                        var filename = Path.GetFileName(srcf).ToUpper();

                        if (filename.Contains(sn.ToUpper())
                            && filename.Contains(whichtest.ToUpper())
                            && filename.Contains("_DUT")
                            && filename.Contains("_TRACEVIEW_"))
                        {
                            var traceviewtimestr = RetrieveTimeFromTraceViewName(filename);
                            if (traceviewtimestr == null)
                                continue;

                            try
                            {
                                //var traceviewtime = DateTime.Parse(traceviewtimestr);
                                //var dbtime = DateTime.Parse(dbtimestr);
                                //if (traceviewtime > dbtime.AddSeconds(-5) && traceviewtime < dbtime.AddSeconds(5))
                                //{
                                    logthdinfo("\r\nStart to copy file: " + srcf);
                                    var desfile = imgdir + filename;
                                    FileCopy(ctrl, srcf, desfile, true);
                                    if (FileExist(ctrl, desfile))
                                    {
                                        logthdinfo("try to add data from file: " + desfile);
                                        ret.Add(desfile);
                                    }//copied file exist
                                //}
                            }
                            catch (Exception ex)
                            {
                                logthdinfo("LoadTraceView2Local Exception: " + ex.Message);
                            }
                        }//end if
                    }//end foreach
                }//end if ret == 0
            }
            catch (Exception e) { }


            return ret;
        }

        #region FILEOPERATE

        private static bool FileExist(Controller ctrl, string filename)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return File.Exists(filename);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static void FileCopy(Controller ctrl, string src, string des, bool overwrite, bool checklocal = false)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    if (checklocal)
                    {
                        if (File.Exists(des))
                        {
                            return;
                        }
                    }

                    File.Copy(src, des, overwrite);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static bool DirectoryExists(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return Directory.Exists(dirname);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static List<string> DirectoryEnumerateFiles(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var ret = new List<string>();
                    ret.AddRange(Directory.EnumerateFiles(dirname));
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        private static string ConvertToDateStr(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return "1982-05-06 10:00:00";
            }
            try
            {
                return DateTime.Parse(datestr).ToString();
            }
            catch (Exception ex) { return "1982-05-06 10:00:00"; }
        }

        private static double ConvertToDoubleVal(string val)
        {
            if (string.IsNullOrEmpty(val))
                return -99999;
            try
            {
                return Convert.ToDouble(val);
            }
            catch (Exception ex)
            {
                return -99999;
            }
        }

        private static void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\scraphelp-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (File.Exists(filename))
                {
                    var content = System.IO.File.ReadAllText(filename);
                    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                    System.IO.File.WriteAllText(filename, content);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
                }
            }
            catch (Exception ex)
            { }

        }

        #endregion

    }

    public class NativeMethods : IDisposable
    {

        // obtains user token  

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]

        static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword,

            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);



        // closes open handes returned by LogonUser  

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        extern static bool CloseHandle(IntPtr handle);
        [DllImport("Advapi32.DLL")]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        [DllImport("Advapi32.DLL")]
        static extern bool RevertToSelf();
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEWCREDENTIALS = 2;

        private bool disposed;

        public NativeMethods(string sUsername, string sDomain, string sPassword)
        {

            // initialize tokens  

            IntPtr pExistingTokenHandle = new IntPtr(0);
            IntPtr pDuplicateTokenHandle = new IntPtr(0);
            try
            {
                // get handle to token  
                bool bImpersonated = LogonUser(sUsername, sDomain, sPassword,

                    LOGON32_LOGON_NEWCREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref pExistingTokenHandle);
                if (true == bImpersonated)
                {

                    if (!ImpersonateLoggedOnUser(pExistingTokenHandle))
                    {
                        int nErrorCode = Marshal.GetLastWin32Error();
                        throw new Exception("ImpersonateLoggedOnUser error;Code=" + nErrorCode);
                    }
                }
                else
                {
                    int nErrorCode = Marshal.GetLastWin32Error();
                    throw new Exception("LogonUser error;Code=" + nErrorCode);
                }

            }

            finally
            {
                // close handle(s)  
                if (pExistingTokenHandle != IntPtr.Zero)
                    CloseHandle(pExistingTokenHandle);
                if (pDuplicateTokenHandle != IntPtr.Zero)
                    CloseHandle(pDuplicateTokenHandle);
            }

        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                RevertToSelf();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class TraceViewData
    {
        public TraceViewData()
        {
            Temp = 25;
            CH = 0;
            Value = "";
        }

        public double Temp { set; get; }
        public int CH { set; get; }
        public double dValue
        {
            get
            {
                if (Value.ToUpper().Contains("0X"))
                {
                    long parsed = long.Parse(Value.ToUpper().Replace("0X", ""), NumberStyles.AllowHexSpecifier);
                    return Convert.ToDouble(parsed);
                }
                else
                {
                    return Convert.ToDouble(Value);
                }
            }
        }
        public string Value { set; get; }
    }
}