using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Web.Caching;
using System.Web.Mvc;



namespace SmartLinks.Models
{
    public class DBUtility
    {
        private static void logthdinfo(string info)
        {
            try
            {
                var filename = "d:\\log\\sqlexception-" + DateTime.Now.ToString("yyyy-MM-dd");
                if (File.Exists(filename))
                {
                    var content = System.IO.File.ReadAllText(filename);
                    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                    System.IO.File.WriteAllText(filename, content);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
                }
            }
            catch (Exception ex)
            { }

        }

        private static void CloseConnector(SqlConnection conn)
        {
            if (conn == null)
                return;

            try
            {
                conn.Dispose();
            }
            catch (SqlException ex)
            {
                logthdinfo("close conn exception: " + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                logthdinfo("close conn exception: " + ex.Message + "\r\n");
            }
        }

        private static SqlConnection GetPRLConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=wuxinpi;User ID=NPI;Password=NPI@NPI;Database=NPITrace;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the parallel summary database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the parallel summary database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExeLocalSqlNoRes(string sql)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var conn = GetPRLConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }

        }

        public static List<List<object>> ExeLocalSqlWithRes(string sql)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetPRLConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }
    }
}