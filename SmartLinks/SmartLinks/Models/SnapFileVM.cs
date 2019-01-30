using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class SnapFileVM
    {
        public static void StoreData(string id,string owner,string shareto,string url,string tag,string now)
        {
            var sql = "insert into SnapFileVM(DocID,Owner,ShareTo,FileAddr,UpdateTime,APVal2) values(@DocID,@Owner,@ShareTo,@FileAddr,@UpdateTime,@Tag)";
            var dict = new Dictionary<string, string>();
            dict.Add("@DocID",id);
            dict.Add("@Owner",owner);
            dict.Add("@ShareTo",shareto);
            dict.Add("@FileAddr",url);
            dict.Add("@UpdateTime", now);
            dict.Add("@Tag", tag);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<SnapFileVM> RetrieveFileListByOwner(string owner)
        {
            var sql = "select distinct DocID,FileAddr,UpdateTime,APVal2 from SnapFileVM where Owner=@Owner and APVal1 <> 'DELETE' order by UpdateTime desc";
            var ret = new List<SnapFileVM>();
            var dict = new Dictionary<string, string>();
            dict.Add("@Owner",owner);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach(var line in dbret)
            {
                var tempvm = new SnapFileVM();
                tempvm.DocID = Convert.ToString(line[0]);
                tempvm.FileAddr = Convert.ToString(line[1]);
                tempvm.UpdateTime = Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd");
                tempvm.Tag = Convert.ToString(line[3]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<SnapFileVM> RetrieveFileListByShareTo(string shareto)
        {
            var sql = "select DocID,FileAddr,Owner,ShareTo,UpdateTime,ReviewTimes,APVal2 from SnapFileVM where ShareTo=@ShareTo order by ReviewTimes desc, UpdateTime desc";
            var ret = new List<SnapFileVM>();
            var dict = new Dictionary<string, string>();
            dict.Add("@ShareTo", shareto);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new SnapFileVM();
                tempvm.DocID = Convert.ToString(line[0]);
                tempvm.FileAddr = Convert.ToString(line[1]);
                tempvm.Owner = Convert.ToString(line[2]);
                tempvm.UpdateTime = Convert.ToDateTime(line[4]).ToString("yyyy-MM-dd");
                tempvm.Tag = Convert.ToString(line[6]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static List<SnapFileVM> RetrieveFileByID(string id, string username)
        {
            var sql = "select DocID,FileAddr,Owner,ShareTo,ReviewTimes,UpdateTime from SnapFileVM where DocID=@DocID";
            var ret = new List<SnapFileVM>();
            var dict = new Dictionary<string, string>();
            dict.Add("@DocID", id);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new SnapFileVM();
                tempvm.DocID = Convert.ToString(line[0]);
                tempvm.FileAddr = Convert.ToString(line[1]);
                tempvm.Owner = Convert.ToString(line[2]);
                tempvm.ShareTo = Convert.ToString(line[3]);
                tempvm.ReviewTimes = Convert.ToInt32(line[4]);

                if (string.Compare(tempvm.ShareTo, username, true) == 0)
                {
                    ret.Add(tempvm);
                    UpdateReviewTimes(tempvm.DocID, tempvm.ShareTo, (tempvm.ReviewTimes + 1));
                    return ret;
                }
                if (string.Compare(tempvm.Owner, username, true) == 0)
                {
                    ret.Add(tempvm);
                    return ret;
                }
            }
            return ret;

        }

        private static void UpdateReviewTimes(string id, string shareto, int times)
        {
            var sql = "update SnapFileVM set ReviewTimes=@ReviewTimes where DocID=@DocID and ShareTo=@ShareTo";
            var dict = new Dictionary<string, string>();
            dict.Add("@DocID", id);
            dict.Add("@ShareTo", shareto);
            dict.Add("@ReviewTimes", times.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void RemoveFileByID(string id)
        {
            var sql = "delete from SnapFileVM where DocID=@DocID and ReviewTimes = 0";
            var dict = new Dictionary<string, string>();
            dict.Add("@DocID", id);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            sql = "update SnapFileVM set APVal1 = 'DELETE'  where DocID=@DocID ";
            dict = new Dictionary<string, string>();
            dict.Add("@DocID", id);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public SnapFileVM()
        {
            DocID = "";
            Owner = "";
            ShareTo = "";
            FileAddr = "";
            ReviewTimes = 0;
            Tag = "";
        }

        public string DocID { set; get; }
        public string Owner { set; get; }
        public string ShareTo { set; get; }
        public string FileAddr { set; get; }
        public int ReviewTimes { set; get; }
        public string Tag { set; get; }
        public string UpdateTime{ set; get; }
    }
}