﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public static void AddMachineUserMap(string machine, string username)
        {
            var sql = "delete from machineusermap where machine = '<machine>'";
            sql = sql.Replace("<machine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);

            var tempname = username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();

            sql = "insert into machineusermap(machine,username) values(@machine,@username)";
            var param = new Dictionary<string, string>();
            param.Add("@machine", machine.ToUpper().Trim());
            param.Add("@username", tempname.ToUpper().Trim());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void TryAddMachineUserMap(string machine, string username)
        {
            var exist = RetrieveUserMap(machine);
            if (exist.Count == 0)
            {
                var tempname = username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();
                var sql = "insert into machineusermap(machine,username) values(@machine,@username)";
                var param = new Dictionary<string, string>();
                param.Add("@machine", machine.ToUpper().Trim());
                param.Add("@username", tempname.ToUpper().Trim());
                DBUtility.ExeLocalSqlNoRes(sql, param);
            }
        }

        public static Dictionary<string,string> RetrieveUserMap(string machine = "",string username = "")
        {
            var ret = new Dictionary<string, string>();

            var sql = "select machine,username from machineusermap where 1 = 1";
            if (!string.IsNullOrEmpty(machine))
            {
                sql = sql + " and machine = '<machine>'";
                sql = sql.Replace("<machine>", machine);
            }
            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " and username = '<username>'";
                sql = sql.Replace("<username>", username);
            }

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

        private static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First().ToUpper();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        public static string GetUseNameByIP(string IP)
        {
            var machinename = DetermineCompName(IP);
            var macuserdict = RetrieveUserMap();
            if (macuserdict.ContainsKey(machinename))
            {
                return macuserdict[machinename];
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool IsSeniorEmployee(string machine, string username)
        {
            var sql = "select machine,username,level from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);

            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " or username = '<username>'";
                sql = sql.Replace("<username>", username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper());
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                return true;
            }

            try
            {
                foreach (var line in dbret)
                {
                    var level = Convert.ToString(line[2]);
                    if (!string.IsNullOrEmpty(level) && level.Length > 1)
                    {
                        var lv = Convert.ToInt32(level.Substring(1));
                        if (lv >= 7 || lv == 0)
                        {
                            return true;
                        }
                    }//end if
                }//en foreach
            }
            catch (Exception ex) { }

            return false;
        }

        public string machine { set; get; }
        public string username { set; get; }
    }
}