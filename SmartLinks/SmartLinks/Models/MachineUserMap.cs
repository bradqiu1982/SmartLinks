using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class MachineUserMap
    {
        public MachineUserMap()
        {
            machine = "";
            username = "";
        }

        public static void UpdateMachineUserMap(string machine, string username)
        {
            var sql = "delete from machineusermap where machine = '<machine>'";
            sql = sql.Replace("<machine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);
            sql = "insert into machineusermap(machine,username) values(@machine,@username)";
            var param = new Dictionary<string, string>();
            param.Add("@machine", machine.ToUpper().Trim());
            param.Add("@username", username.ToUpper().Trim());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string,string> RetrieveUserMap()
        {
            var ret = new Dictionary<string, string>();

            var sql = "select machine,username from machineusermap";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new MachineUserMap();
                tempvm.machine = Convert.ToString(line[0]);
                tempvm.username = Convert.ToString(line[1]);
                if (!ret.ContainsKey(tempvm.machine))
                {
                    ret.Add(tempvm.machine,tempvm.username);
                }
            }
            return ret;
        }

        public string machine { set; get; }
        public string username { set; get; }
    }
}