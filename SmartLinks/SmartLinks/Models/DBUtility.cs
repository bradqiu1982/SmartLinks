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
using System.Data.Odbc;
//using Oracle.DataAccess.Client;

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

        private static SqlConnection GetLocalConnector()
        {
            var conn = new SqlConnection();
            try
            {
                //conn.ConnectionString = "Server=WUX-D80008792;User ID=dbg;Password=dbgpwd;Database=DebugDB;Connection Timeout=120;";
                conn.ConnectionString = "Server=wuxinpi;User ID=NPI;Password=NPI@NPI;Database=NPITrace;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the local database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the local database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExeLocalSqlNoRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var conn = GetLocalConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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

        public static List<List<object>> ExeLocalSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetLocalConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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

        public static SqlConnection GetWATConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=wuxinpi;User ID=WATApp;Password=WATApp@123;Database=WAT;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                logthdinfo("open connect exception: " + ex.Message + "\r\n");
                CloseConnector(conn);
                return null;
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                logthdinfo("open connect exception: " + ex.Message + "\r\n");
                CloseConnector(conn);
                return null;
            }
        }


        public static bool ExeWATSqlNoRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var conn = GetWATConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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

        public static List<List<object>> ExeWATSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetWATConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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


        public static SqlConnection GetOGPConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=Wux-Parallel.chn.ii-vi.net;User ID=AiProject;Password=Ai@parallel;Database=AIProjects;Connection Timeout=120";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                logthdinfo("open connect exception: " + ex.Message + "\r\n");
                CloseConnector(conn);
                return null;
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                logthdinfo("open connect exception: " + ex.Message + "\r\n");
                CloseConnector(conn);
                return null;
            }
        }

        public static List<List<object>> ExeOGPSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetOGPConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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


        private static SqlConnection GetAllenConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=TEX-CSSQL.texas.ads.finisar.com;User ID=jmpuser;Password=UhnWNcgHo;Database=EngrData;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the allen database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the allen database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExeAllenSqlNoRes(string sql, Dictionary<string, string> parameters = null)
        {

            var conn = GetAllenConnector();
            if (conn == null)
                return false;
            SqlCommand command = null;

            try
            {
                command = conn.CreateCommand();
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                command.ExecuteNonQuery();
                CloseConnector(conn);

                return true;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                try
                {
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e) { }
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                try
                {
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e) { }
                CloseConnector(conn);
                //System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static List<List<object>> ExeAllenSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetAllenConnector();
            if (conn == null)
                return ret;
            SqlDataReader sqlreader = null;
            SqlCommand command = null;

            try
            {
                command = conn.CreateCommand();
                command.CommandTimeout = 180;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {
                    while (sqlreader.Read())
                    {
                        Object[] values = new Object[sqlreader.FieldCount];
                        sqlreader.GetValues(values);
                        ret.Add(values.ToList<object>());
                    }

                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }

                CloseConnector(conn);

                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }

                CloseConnector(conn);

                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetLocalBSConnector()
        {
            var conn = new SqlConnection();
            try
            {
                //conn.ConnectionString = "Server=WUX-D80008792;User ID=dbg;Password=dbgpwd;Database=DebugDB;Connection Timeout=120;";
                conn.ConnectionString = "Server=wuxinpi;User ID=BSApp;Password=magic@123;Database=BSSupport;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the local database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the local database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static List<List<object>> ExeBSSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetLocalBSConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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


        private static SqlConnection GetMESReportConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wux-prod02\mes_report;uid=Active_NPI;pwd=Active@123;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                return null;
            }
        }

        public static List<List<object>> ExeMESReportSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetMESReportConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 180;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetNebulaConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wuxinpi;User ID=NebulaNPI;Password=abc@123;Database=NebulaTrace;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeNebulaSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetNebulaConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
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



        private static SqlConnection GetDMRConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=wux-prod03\prod01;uid=OA_FAI_Reader;pwd=123_FAI_#;Database=eDMR;Connection Timeout=90;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the DMR pdms database:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the DMR pdms database" + ex.Message);
                return null;
            }
        }

        public static List<List<object>> ExeDMRSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetDMRConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 180;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }



        private static SqlConnection GetRealMESConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=CN-CSSQL;uid=SHG_Read;pwd=shgread;Database=InsiteDB;Connection Timeout=30;";
                //conn.ConnectionString = "Server=wux-csods;uid=NPI_FA;pwd=msW2TH95Pd;Database=InsiteDB;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<List<object>> ExeRealMESSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetRealMESConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 120;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetOSABurnInConnector(string machine)
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server="+machine+ @";uid=sa;pwd=cml@shg629;Connection Timeout=30;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                return null;
            }
        }

        public static List<List<object>> ExeOSABurnInSqlWithRes(string machine,string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetOSABurnInConnector(machine);
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 180;
                command.CommandText = sql;
                command.CommandText = "SET ARITHABORT ON;" + command.CommandText;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
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
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }


        private static OdbcConnection GetCDPConnector()
        {
            var conn = new OdbcConnection();
            try
            {
                conn.ConnectionString = @"driver={Cloudera ODBC Driver for Impala};Database=wuxi;host=10.20.10.202;port=21050;uid=bqiu;pwd=finisar123";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the FAI summary database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the FAI summary database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static void CloseOConnector(OdbcConnection conn)
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

        /* parameters: 
         * if you want to defense SQL injection,
         * you can prepare @param in sql,
         * and give @param-values in parameters.
         */
        public static List<List<object>> ExeCDPSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            //var syscfgdict = CfgUtility.GetSysConfig(ctrl);

            var ret = new List<List<object>>();
            var conn = GetCDPConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
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
                CloseOConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseOConnector(conn);
                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
                CloseOConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        //public static List<List<object>> ExeATESqlWithRes(string sql)
        //{

        //    var ret = new List<List<object>>();

        //    OracleConnection Oracleconn = null;
        //    try
        //    {
        //        var ConnectionStr = "User Id=extviewer;Password=extviewer;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=shg-oracle)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ateshg)));";

        //        Oracleconn = new OracleConnection(ConnectionStr);
        //        try
        //        {
        //            if (Oracleconn.State == ConnectionState.Closed)
        //            {
        //                Oracleconn.Open();
        //            }
        //            else if (Oracleconn.State == ConnectionState.Broken)
        //            {
        //                Oracleconn.Close();
        //                Oracleconn.Open();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            //System.Windows.MessageBox.Show(e.Message);
        //        }

        //        OracleCommand cmd = new OracleCommand(sql, Oracleconn);
        //        cmd.CommandType = CommandType.Text;
        //        OracleDataReader dr = cmd.ExecuteReader();

        //        while (dr.Read())
        //        {
        //            var line = new List<object>();
        //            for (int idx = 0; idx < dr.FieldCount; idx++)
        //            {
        //                line.Add(dr[idx]);
        //            }
        //            ret.Add(line);
        //        }

        //        Oracleconn.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

        //        //System.Windows.MessageBox.Show(ex.Message);

        //        try
        //        {
        //            if (Oracleconn != null)
        //            {
        //                Oracleconn.Close();
        //            }
        //        }
        //        catch (Exception ex1) { }

        //    }
        //    return ret;

        //}

    }
}