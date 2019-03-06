using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace SmartLinks.Models
{
    public class ExternalDataCollector
    {
        private static XmlDocument StripNamespace(XmlDocument doc)
        {
            if (doc.DocumentElement.NamespaceURI.Length > 0)
            {
                doc.DocumentElement.SetAttribute("xmlns", "");
                // must serialize and reload for this to take effect
                XmlDocument newDoc = new XmlDocument();
                newDoc.LoadXml(doc.OuterXml);
                return newDoc;
            }
            else
            {
                return doc;
            }
        }

        private static void SolveDieSortFile(string diefile, string desfolder)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(diefile);
                var namesp = doc.DocumentElement.GetAttribute("xmlns");
                doc = StripNamespace(doc);

                XmlElement root = doc.DocumentElement;
                var nodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass']");
                foreach (XmlElement nd in nodes)
                {
                    nd.SetAttribute("BinCount", "666");
                }

                foreach (XmlElement nd in root.SelectNodes("//BinCode[@X='240' and @Y='74']"))
                {
                    nd.ParentNode.RemoveChild(nd);
                    //System.Windows.MessageBox.Show(nd.InnerText);
                }

                doc.DocumentElement.SetAttribute("xmlns", namesp);
                doc.Save(Path.Combine(desfolder, Path.GetFileName(diefile) + ".NEW"));

                doc = new XmlDocument();
                doc.Load(diefile);
                doc.Save(Path.Combine(desfolder, Path.GetFileName(diefile)));
            }
            catch (Exception ex) { }
        }

        public static void LoadDieSortFile(Controller ctrl)
        {
            var filetype = "DIESORT";
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var srcfolder = syscfgdict["DIESORTFOLDER"];
            var desfolder = syscfgdict["DIESORTSHARE"];
            var srcfiles = DirectoryEnumerateFiles(ctrl, srcfolder);

            var loadedfiledict = FileLoadedData.LoadedFiles(filetype);
            foreach (var srcf in srcfiles)
            {
                var srcfilename = Path.GetFileName(srcf);
                if (loadedfiledict.ContainsKey(srcfilename))
                { continue; }
                var desfile = DownloadShareFile(srcf, ctrl);
                if (desfile != null && FileExist(ctrl, desfile))
                {
                    //FileLoadedData.UpdateLoadedFile(srcfilename, filetype);
                    SolveDieSortFile(desfile, desfolder);
                }
            }
        }

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

        public static Dictionary<string, bool> LoadParallelBIData(Controller ctrl)
        {
            var ret = new Dictionary<string, bool>();
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var htolfile = syscfgdict["CWDM4SHTOL"];
            var desfile = DownloadShareFile(htolfile, ctrl);
            if (desfile != null && FileExist(ctrl, desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile, "BI SN", 3);
                foreach (var line in rawdata)
                {
                    var sn = line[0].ToUpper().Trim();
                    if (!ret.ContainsKey(sn))
                    {
                        ret.Add(sn, true);
                    }
                }
            }
            return ret;
        }

        public static void LoadORLData(List<CWDM4Data> retdata,Controller ctrl)
        {
            var ret = new Dictionary<string, CWDM4Data>();
            var syscfgdict = CfgUtility.GetSysConfig(ctrl);
            var htolfile = syscfgdict["CWDM4ORL"];
            var desfile = DownloadShareFile(htolfile, ctrl);
            if (desfile != null && FileExist(ctrl, desfile))
            {
                var rawdata = RetrieveDataFromExcelWithAuth(ctrl, desfile,null, 6);
                rawdata.Reverse();
                foreach (var line in rawdata)
                {
                    var sn = line[0].ToUpper().Trim();
                    if (!ret.ContainsKey(sn))
                    {
                        var vm = new CWDM4Data();
                        vm.ORLTX = line[1];
                        vm.ORLRX = line[2];
                        vm.ORLTX70C = line[3];
                        ret.Add(sn, vm);
                    }
                }
            }

            foreach (var item in retdata)
            {
                if (!item.IsCWDM4)
                { continue; }

                if (ret.ContainsKey(item.SN))
                {
                    item.ORLTX = ret[item.SN].ORLTX;
                    item.ORLRX = ret[item.SN].ORLRX;
                    item.ORLTX70C = ret[item.SN].ORLTX70C;
                }
            }

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