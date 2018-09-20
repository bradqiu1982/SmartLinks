using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartLinks.Models
{
    public class ExternalDataCollector
    {

        public static Dictionary<string, string> LoadSHTOLData(Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var htolfile = syscfgdict["CWDM4SHTOL"];
            var desfile = DownloadShareFile(htolfile, ctrl);
            if (desfile != null && FileExist(ctrl, desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile, "SN", 3);
                foreach (var line in rawdata)
                {
                    var sn = line[0].ToUpper().Trim();
                    var date = line[1];
                    try
                    {
                        date = DateTime.Parse(date).ToString("yyyy-MM-dd");
                    }
                    catch (Exception ex) { }
                    if (!ret.ContainsKey(sn))
                    {
                        ret.Add(sn, date);
                    }
                }
            }
            return ret;
        }

        public static Dictionary<string, string> LoadORLData(Controller ctrl)
        {
            var ret = new Dictionary<string, string>();
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var htolfile = syscfgdict["CWDM4ORL"];
            var desfile = DownloadShareFile(htolfile, ctrl);
            if (desfile != null && FileExist(ctrl, desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile,null, 5);
                foreach (var line in rawdata)
                {
                    var sn = line[0].ToUpper().Trim();
                    var txval = line[1];
                    var rxval = line[2];

                    if (!ret.ContainsKey(sn))
                    {
                        ret.Add(sn, "TX:"+txval+"/RX:"+rxval);
                    }
                }
            }
            return ret;
        }

        public static string DownloadShareFile(string srcfile, Controller ctrl)
        {
            try
            {
                if (ExternalDataCollector.FileExist(ctrl, srcfile))
                {
                    var filename = System.IO.Path.GetFileName(srcfile);
                    var descfolder = ctrl.Server.MapPath("~/userfiles") + "\\docs\\ShareFile\\";
                    if (!ExternalDataCollector.DirectoryExists(ctrl, descfolder))
                        ExternalDataCollector.CreateDirectory(ctrl, descfolder);
                    var descfile = descfolder + System.IO.Path.GetFileNameWithoutExtension(srcfile) + DateTime.Now.ToString("yyyy-MM-dd") + System.IO.Path.GetExtension(srcfile);
                    ExternalDataCollector.FileCopy(ctrl, srcfile, descfile, true);
                    return descfile;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

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

        public static void CreateDirectory(Controller ctrl, string dirname)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    Directory.CreateDirectory(dirname);
                }
            }
            catch (Exception ex)
            { }

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

        private static List<List<string>> RetrieveDataFromExcelWithAuth(Controller ctrl, string filename, string sheetname = null, int columns = 101)
        {
            try
            {
                var syscfgdict = CfgUtility.GetSysConfig(ctrl);
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];

                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    return ExcelReader.RetrieveDataFromExcel(filename, sheetname, columns);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}