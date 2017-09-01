using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class MachineLink
    {
        public MachineLink()
        {
            ReqMachine = "";
            LinkName = "";
            Link = "";
        }

        public string ReqMachine { set; get; }
        public string LinkName { set; get; }
        public string Action { set; get; }
        public string Link { set; get; }
    }

    public class LinkVM
    {
        public LinkVM()
        {
            LinkName = string.Empty;
            Link = string.Empty;
            Logo = string.Empty;
        }

        public static void StoreLink(string linkname, string link, string logo)
        {
            var sql = "delete from LinkVM where LinkName = '<LinkName>'";
            sql = sql.Replace("<LinkName>", linkname);
            DBUtility.ExeLocalSqlNoRes(sql);

            sql = "insert into LinkVM(LinkName,Link,Logo,UpdateTime) values('<LinkName>','<Link>','<Logo>','<UpdateTime>')";
            sql = sql.Replace("<LinkName>", linkname).Replace("<Link>", link).Replace("<Logo>", logo).Replace("<UpdateTime>",DateTime.Now.ToString());
            DBUtility.ExeLocalSqlNoRes(sql);
        }

        public List<LinkVM> RetrieveLinks()
        {
            var ret = new List<LinkVM>();
            var sql = "select LinkName,Link,Logo from LinkVM order by UpdateTime asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var temp = new LinkVM();
                temp.LinkName = Convert.ToString(line[0]);
                temp.Link = Convert.ToString(line[1]);
                temp.Logo = Convert.ToString(line[2]);
                ret.Add(temp);
            }
            return ret;
        }

        public string LinkName { set; get; }
        public string Link { set; get; }
        public string Logo { set; get; }
    }
}