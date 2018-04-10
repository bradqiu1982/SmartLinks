using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SmartLinks.Models
{
    public class ProjectVM
    {

        public static List<string> RetrieveAllProjectKey()
        {
            var sql = "select ProjectKey from Project order by ProjectKey ASC";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,null);
            var ret = new List<string>();
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString( line[0]));
            }
            return ret;
        }

    }

}
