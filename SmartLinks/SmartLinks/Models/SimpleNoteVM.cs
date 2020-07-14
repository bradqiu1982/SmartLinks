using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class SimpleNoteVM
    {
        private static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static void StoreNote(string doorcode, string note)
        {
            var notekey = GetUniqKey();
            var notedate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var dict = new Dictionary<string, string>();
            dict.Add("@notekey", notekey);
            dict.Add("@doorcode", doorcode);
            dict.Add("@note", note);
            dict.Add("@notedate", notedate);

            var sql = "insert into SimpleNoteVM(notekey,doorcode,note,notedate) values(@notekey,@doorcode,@note,@notedate)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<SimpleNoteVM> GetNote(string doorcode)
        {
            var ret = new List<SimpleNoteVM>();
            var dict = new Dictionary<string, string>();
            dict.Add("@doorcode", doorcode);
            var sql = "select top 100 notekey,doorcode,note,notedate from SimpleNoteVM where doorcode=@doorcode order by notedate desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(new SimpleNoteVM(UT.O2S(line[0]), UT.O2S(line[1])
                    , UT.O2S(line[2]), Convert.ToDateTime(line[3]).ToString("yyyy-MM-dd")));
            }
            return ret;
        }

        public SimpleNoteVM(string nk,string dc,string nt,string nd)
        {
            notekey = nk;
            doorcode = dc;
            note = nt;
            notedate = nd;
        }

        public SimpleNoteVM()
        {
            notekey = "";
            doorcode = "";
            note = "";
            notedate = "";
        }

        public string notekey { set; get; }
        public string doorcode { set; get; }
        public string note { set; get; }
        public string notedate { set; get; }

    }
}