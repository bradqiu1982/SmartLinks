using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class LINKACTION
    {
        public static string DELETE = "DELETE";
    }

    public class MachineLink
    {
        public MachineLink()
        {
            ReqMachine = "";
            LinkName = "";
            Link = "";
            Comment = "";
            Logo = "";
            Action = "";
        }

        public static void StoreLink(string linkname, string link, string logo, string comment,string machine)
        {
            var sql = "delete from MachineLink where LinkName = '<LinkName>' and ReqMachine='<ReqMachine>'";
            sql = sql.Replace("<LinkName>", linkname).Replace("<ReqMachine>",machine);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into MachineLink(LinkName,Link,Logo,Comment,ReqMachine,Freqence) values(N'<LinkName>',N'<Link>',N'<Logo>',N'<Comment>',N'<ReqMachine>',0)";
            sql = sql.Replace("<LinkName>", linkname).Replace("<Link>", link).Replace("<Logo>", logo)
                .Replace("<Comment>", comment).Replace("<ReqMachine>",machine);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<MachineLink> RetrieveLinks(string machine)
        {
            var ret = new List<MachineLink>();
            var sql = "select LinkName,Link,Logo,Comment,Action from MachineLink where ReqMachine = N'<ReqMachine>' and LinkName <> '' and Link <> '' order by Freqence desc";
            sql = sql.Replace("<ReqMachine>", machine);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var temp = new MachineLink();
                temp.LinkName = Convert.ToString(line[0]);
                temp.Link = Convert.ToString(line[1]);
                temp.Logo = Convert.ToString(line[2]);
                temp.Comment = Convert.ToString(line[3]);
                temp.Action = Convert.ToString(line[4]);
                ret.Add(temp);
            }
            return ret;
        }

        public static void UpdateFrequence(string linkname, string link, string logo, string comment, string machine)
        {
            var sql = "select Freqence from MachineLink where ReqMachine = '<ReqMachine>' and LinkName = '<LinkName>'";
            sql = sql.Replace("<ReqMachine>", machine).Replace("<LinkName>", linkname);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            var frequence = 0;
            if (dbret.Count == 0)
            {
                StoreLink(linkname, link, logo, comment, machine);
                frequence = 1;
            }
            else
            {
                frequence = Convert.ToInt32(dbret[0][0])+1;
            }
            sql = "update MachineLink set Freqence = <Freqence>  where ReqMachine = '<ReqMachine>' and LinkName = '<LinkName>'";
            sql = sql.Replace("<ReqMachine>", machine).Replace("<LinkName>", linkname).Replace("<Freqence>",frequence.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void RemoveCustomLink(string linkname, string link, string logo, string comment, string machine)
        {
            var sql = "select Freqence from MachineLink where ReqMachine = '<ReqMachine>' and LinkName = '<LinkName>'";
            sql = sql.Replace("<ReqMachine>", machine).Replace("<LinkName>", linkname);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count == 0)
            {
                StoreLink(linkname, link, logo, comment, machine);
            }

            sql = "update MachineLink set Action = N'<Action>'  where ReqMachine = '<ReqMachine>' and LinkName = '<LinkName>'";
            sql = sql.Replace("<ReqMachine>", machine).Replace("<LinkName>", linkname).Replace("<Action>", LINKACTION.DELETE);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static void NeverShowIE8Modal(string machine)
        {
            var sql = "delete from MachineLink where LinkName='' and ReqMachine = '<ReqMachine>' and Appv_1 <> ''";
            sql = sql.Replace("<ReqMachine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);
            sql = "insert into MachineLink(ReqMachine,Appv_1) values(N'<ReqMachine>','nevershow')";
            sql = sql.Replace("<ReqMachine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static bool IsNeverShowIE8Modal(string machine)
        {
            var ret = false;
            var sql = "select ReqMachine from MachineLink where  LinkName='' and ReqMachine = '<ReqMachine>' and Appv_1 <> ''";
            sql = sql.Replace("<ReqMachine>", machine);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                ret = true;
            }
            return ret;
        }

        public string ReqMachine { set; get; }
        public string LinkName { set; get; }
        public string Action { set; get; }
        public string Link { set; get; }
        public string Comment { set; get; }
        public string Logo { set; get; }
    }

    public class LinkVM
    {
        public LinkVM()
        {
            LinkName = string.Empty;
            Link = string.Empty;
            Logo = string.Empty;
            Comment = string.Empty;
        }

        public static void StoreLink(string linkname, string link, string logo,string comment)
        {
            var sql = "delete from LinkVM where LinkName = N'<LinkName>'";
            sql = sql.Replace("<LinkName>", linkname);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into LinkVM(LinkName,Link,Logo,UpdateTime,Comment) values(N'<LinkName>',N'<Link>',N'<Logo>',N'<UpdateTime>',N'<Comment>')";
            sql = sql.Replace("<LinkName>", linkname).Replace("<Link>", link).Replace("<Logo>", logo)
                .Replace("<UpdateTime>",DateTime.Now.ToString()).Replace("<Comment>", comment);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public static List<LinkVM> RetrieveLinks()
        {
            var ret = new List<LinkVM>();
            var sql = "select LinkName,Link,Logo,Comment from LinkVM order by UpdateTime asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var temp = new LinkVM();
                temp.LinkName = Convert.ToString(line[0]);
                temp.Link = Convert.ToString(line[1]);
                temp.Logo = Convert.ToString(line[2]);
                temp.Comment = Convert.ToString(line[3]);
                ret.Add(temp);
            }
            return ret;
        }

        public static void RemoveBaseLink(string linkname)
        {
            var sql = "delete from LinkVM where LinkName = '<LinkName>'";
            sql = sql.Replace("<LinkName>", linkname);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "delete from MachineLink where LinkName = '<LinkName>'";
            sql = sql.Replace("<LinkName>", linkname);
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public string LinkName { set; get; }
        public string Link { set; get; }
        public string Logo { set; get; }
        public string Comment { set; get; }
    }
}