using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace SmartLinks.Models
{
    public class UserVM
    {
        public static List<string> RetrieveAllUser()
        {
            var sql = "select UserName from UserTable";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<string>();

            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]));
            }

            ret.Sort();
            return ret;
        }

    }

}