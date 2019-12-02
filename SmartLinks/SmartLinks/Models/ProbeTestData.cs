using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartLinks.Models
{
    public class ProbeTestData
    {
        public ProbeTestData(string w, string f, string x, string y, string b, string ap)
        {
            Wafer = w;
            Famy = f;
            X = x;
            Y = y;
            Bin = b;
            ApSize = ap;

            APVal1 = "";
            APVal2 = "";
            APVal3 = "";
            APVal4 = "";
            APVal5 = "";
            APVal6 = "";
            APVal7 = "";
            APVal8 = "";
            APVal9 = "";
        }

        public ProbeTestData()
        {
            Wafer = "";
            Famy = "";
            X = "";
            Y = "";
            Bin = "";
            ApSize = "";

            APVal1 = "";
            APVal2 = "";
            APVal3 = "";
            APVal4 = "";
            APVal5 = "";
            APVal6 = "";
            APVal7 = "";
            APVal8 = "";
            APVal9 = "";
        }

        public static void CleanData(string w)
        {
            if (HasData(w))
            {
                var sql = "delete from ProbeTestData where Wafer = @Wafer";
                var dict = new Dictionary<string, string>();
                dict.Add("@Wafer", w);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        public void StoreData()
        {
            var sql = "insert into ProbeTestData(Wafer,Famy,X,Y,Bin,ApSize,APVal1) values(@Wafer,@Famy,@X,@Y,@Bin,@ApSize,@APVal1)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", Wafer);
            dict.Add("@Famy", Famy);
            dict.Add("@X",X);
            dict.Add("@Y",Y);
            dict.Add("@Bin", Bin);
            dict.Add("@ApSize", ApSize);
            dict.Add("@APVal1", APVal1);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static bool HasData(string w)
        {
            var sql = "select top 1 Wafer from ProbeTestData where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", w);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static ProbeTestData  GetApSizeByWafer(string w,string x,string y)
        {
            var sql = "select ApSize,APVal1 from ProbeTestData where Wafer=@Wafer and X=@X and Y=@Y";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", w);
            dict.Add("@X", x);
            dict.Add("@Y", y);

            var ret = new ProbeTestData();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            {
                ret.ApSize = UT.O2S(dbret[0][0]);
                ret.APVal1 = UT.O2S(dbret[0][1]);
                ret.X = x;
                ret.Y = y;
            }
            return ret;
        }

        public static List<string> WaferList()
        {
            var ret = new List<string>();
            var sql = "select distinct Wafer from ProbeTestData";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { ret.Add(UT.O2S(line[0])); }
            return ret;
        }

        private static void UpdateArrayInfo()
        {
            var wflist = WaferList();
            foreach (var wf in wflist)
            {
                var arraysize = GetWaferArray(wf);
                var sql = "update ProbeTestData set APVal1 = @APVal1 where Wafer = @Wafer";

                var dict = new Dictionary<string, string>();
                dict.Add("@Wafer",wf);
                dict.Add("@APVal1",arraysize);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        private static string GetProductFamilyFromAllen(string wafernum)
        {
            var sql = @"select distinct left(pf.productfamilyname,4) from insite.insite.container c with (nolock) 
                        inner join [Insite].[insite].[ProductBase] pb ON pb.[RevOfRcdId] = c.[ProductId]
                        inner join insite.insite.Product p with(nolock) on p.[ProductBaseId] = pb.[ProductBaseId] 
                        inner join insite.insite.ProductFamily pf with(nolock) on p.ProductFamilyID = pf.ProductFamilyID
                        where containername = @wafernum";

            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);

            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return string.Empty;
        }

        public static string GetWaferArray(string wafernum)
        {
            var productfm = GetProductFamilyFromAllen(wafernum);
            if (string.IsNullOrEmpty(productfm))
            { return "1"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@productfm", productfm);
            var sql = @"select na.Array_Length from  [EngrData].[dbo].[NeoMAP_MWR_Arrays] na with (nolock) where na.product_out = @productfm";
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                if (line[0] != System.DBNull.Value)
                {
                    return UT.O2S(line[0]);
                }
            }

            return "1";
        }

        public static void UpdateQueryHistory(string wafer, string x, string y, string machine, string rest)
        {
            var sql = "update ProbeTestData set APVal7=@machine,APVal8=@rest,APVal9=@updatetime where Wafer=@wafer and X=@x and Y=@y";
            var dict = new Dictionary<string, string>();
            dict.Add("@machine", machine);
            dict.Add("@rest", rest);
            dict.Add("@updatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@wafer", wafer);
            dict.Add("@x",x);
            dict.Add("@y",y);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<ProbeTestData> GetQueryHistory(string wafer,string x,string y)
        {
            var sql = "";
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(wafer) && !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y))
            {
                sql = "select Wafer,X,Y,ApSize,APVal1,APVal7,APVal8,APVal9 from ProbeTestData  where Wafer=@wafer and X=@x and Y=@y and APVal8 <> '' order by APVal9 desc";
                dict.Add("@wafer", wafer);
                dict.Add("@x", x);
                dict.Add("@y", y);
            }
            else if (!string.IsNullOrEmpty(wafer) && !string.IsNullOrEmpty(x))
            {
                sql = "select Wafer,X,Y,ApSize,APVal1,APVal7,APVal8,APVal9 from ProbeTestData  where Wafer=@wafer and X=@x and APVal8 <> ''  order by APVal9 desc";
                dict.Add("@wafer", wafer);
                dict.Add("@x", x);
            }
            else if (!string.IsNullOrEmpty(wafer)  && !string.IsNullOrEmpty(y))
            {
                sql = "select Wafer,X,Y,ApSize,APVal1,APVal7,APVal8,APVal9 from ProbeTestData  where Wafer=@wafer and Y=@y and APVal8 <> ''  order by APVal9 desc";
                dict.Add("@wafer", wafer);
                dict.Add("@y", y);
            }
            else
            {
                sql = "select Wafer,X,Y,ApSize,APVal1,APVal7,APVal8,APVal9 from ProbeTestData  where Wafer=@wafer and APVal8 <> ''  order by APVal9 desc";
                dict.Add("@wafer", wafer);
            }

            var ret = new List<ProbeTestData>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new ProbeTestData();
                tempvm.Wafer = UT.O2S(line[0]);
                tempvm.X = UT.O2S(line[1]);
                tempvm.Y = UT.O2S(line[2]);
                tempvm.ApSize = UT.O2S(line[3]);
                tempvm.APVal1 = UT.O2S(line[4]);
                tempvm.APVal7 = UT.O2S(line[5]);
                tempvm.APVal8 = UT.O2S(line[6]);
                tempvm.APVal9 = UT.O2T(line[7]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public string Wafer { set; get; }
        public string Famy { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string Bin { set; get; }
        public string ApSize { set; get; }

        public string APVal1 { set; get; }
        public string APVal2 { set; get; }
        public string APVal3 { set; get; }
        public string APVal4 { set; get; }
        public string APVal5 { set; get; }
        public string APVal6 { set; get; }
        public string APVal7 { set; get; }
        public string APVal8 { set; get; }
        public string APVal9 { set; get; }
        
    }
}