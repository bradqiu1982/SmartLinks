using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using Docx = Microsoft.Office.Interop.Word;
using Pptx = Microsoft.Office.Interop.PowerPoint;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace SmartLinks.Models
{
    public class OfficeConverter
    {

        private static void ReleaseRCM(object o)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
            }
            catch
            {
            }
            finally
            {
                o = null;
            }
        }

        public static bool ExcelConverter(string srcfile)
        {
            try
            {
                var format = Excel.XlFileFormat.xlHtml;
                var desfile = srcfile.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0] + ".html";

                var excel = new Excel.Application();
                excel.DisplayAlerts = false;
                var books = excel.Workbooks;

                Excel.Workbook book = books.Open(
                srcfile, false, true,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, false, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing);

                book.SaveAs(desfile,format);

                book.Close();
                books.Close();
                excel.Quit();

                Marshal.ReleaseComObject(book);
                Marshal.ReleaseComObject(books);
                Marshal.ReleaseComObject(excel);

                if (book != null)
                    ReleaseRCM(book);
                if (excel != null)
                    ReleaseRCM(excel);

                book = null;
                books = null;
                excel = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex) {
                //System.Windows.MessageBox.Show(ex.ToString());
                return false; }

            return true;
        }

        public static bool DocConverter(string srcfile)
        {
            try
            {
                var format = Docx.WdSaveFormat.wdFormatHTML;
                var desfile = srcfile.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0] + ".html";

                var docx = new Docx.Application();
                docx.DisplayAlerts = Docx.WdAlertLevel.wdAlertsNone;
                docx.Visible = false;
                var docs = docx.Documents;
                var opf = docs.Open(srcfile);
                
                opf.SaveAs2(desfile, format);

                opf.Close();
                docx.Quit();
                Marshal.ReleaseComObject(opf);
                Marshal.ReleaseComObject(docs);
                Marshal.ReleaseComObject(docx);

                if (opf != null)
                    ReleaseRCM(opf);
                if (docx != null)
                    ReleaseRCM(docx);

                opf=null;
                docs=null;
                docx=null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex) {
                //System.Windows.MessageBox.Show(ex.ToString());
                return false; }
            return true;
        }

        public static bool PPTConverter(string srcfile)
        {
            try
            {
                var format = Pptx.PpSaveAsFileType.ppSaveAsPDF;
                var desfile = srcfile.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0] + ".pdf";

                var pptx = new Pptx.Application();
                pptx.DisplayAlerts = Pptx.PpAlertLevel.ppAlertsNone;
                //pptx.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                var ppts = pptx.Presentations;
                var opf = ppts.Open(srcfile,Microsoft.Office.Core.MsoTriState.msoTrue,Microsoft.Office.Core.MsoTriState.msoFalse,Microsoft.Office.Core.MsoTriState.msoFalse);
                opf.SaveAs(desfile, format);

                opf.Close();
                pptx.Quit();

                Marshal.ReleaseComObject(opf);
                Marshal.ReleaseComObject(ppts);
                Marshal.ReleaseComObject(pptx);

                if (opf != null)
                    ReleaseRCM(opf);
                if (pptx != null)
                    ReleaseRCM(pptx);

                opf = null;
                ppts = null;
                pptx = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(ex.ToString());
                return false; }
            return true;
        }

        public static bool OutlookConverter(string srcfile)
        {
            try
            {
                var format = Outlook.OlSaveAsType.olHTML;
                var desfile = srcfile.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[0] + ".html";

                var outlook = new Outlook.Application();
                Outlook.MailItem opf = outlook.Session.OpenSharedItem(srcfile);
                opf.SaveAs(desfile, format);
                opf.Close(Outlook.OlInspectorClose.olDiscard);
                outlook.Quit();

                Marshal.ReleaseComObject(opf);
                Marshal.ReleaseComObject(outlook);

                if (opf != null)
                    ReleaseRCM(opf);
                if (outlook != null)
                    ReleaseRCM(outlook);

                opf = null;
                outlook = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

    }
}