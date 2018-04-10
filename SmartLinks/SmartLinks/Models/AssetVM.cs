using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

namespace SmartLinks.Models
{
    public class AssetStatus
    {
        //1： Idle 2: Stop 3: Serviced 4: In Calibration 5: Active
        public static int Idle = 1;
        public static int Stop = 2;
        public static int Serviced = 3;
        public static int InCalibration = 4;
        public static int Active = 5;
    }

    public class BorrowStatus
    {
        //1: borrowed 2: returned 3: invalid
        public static int Borrowed = 1;
        public static int Returned = 2;
        public static int Invalid = 3;
    }

    public class BuyStatus
    {
        public static int Request = 1;
        public static int Buyed = 2;
        public static int Invalid = 3;
    }

    public class CheckStatus
    {
        public static int Valid = 1;
        public static int Complete = 2;
        public static int Invalid = 3;
    }

    public class AssetVM
    {
        public AssetVM()
        {
            ID = "";
            CN = "";
            Site = "";
            EnglishName = "";
            ChineseName = "";
            Model = "";
            Supplier = "";
            Manufactures = "";
            SN = "";
            PO = "";
            FinanceNo = "";
            BuyDate = "";
            Department = "";
            IsPM = "";
            IsCal = "";
            IsKey = "";
            ManufactureLocation = "";
            ImportLot = "";
            LotLine = "";
            EquipmentStatus = "";
            Remark = "";
            QualificationDate = "";
            AssetType = "";
            EquipmentType = "";
            ParentCN = "";
            Used = "";
            QualificationReportNo = "";
            UserId = "";
            Cdt = "";
            Udt = "";
            OriginalValue_RMB = "";
            Monthly_depreciation = "";
            Accumulated_depreciation = "";
            NetValue_RMB = "";
            DFNO = "";
            Location = "";
            Station = "";
            ProjectNo = "";
            Length = "";
            Width = "";
            Height = "";
            NetWeight = "";
            Grade = "";
            Voltage = "";
            Power = "";
            Vacuum = "";
            CompressedAir = "";
            NitrogenGas = "";
            LocationRemark = "";
            PictureUrl = "";
            Owner = "";
            CheckDate = null;
            CIP = "";
            Actual_User = "";
            Asset_Dept = "";
            BorrowHistory = new List<AssetBorrowHistoryVM>();
            CheckHistory = new List<AssetCheckHistoryVM>();
        }
        public string ID { set; get; }
        public string CN { set; get; }
        public string Site { set; get; }
        public string EnglishName { set; get; }
        public string ChineseName { set; get; }
        public string Model { set; get; }
        public string Supplier { set; get; }
        public string Manufactures { set; get; }
        public string SN { set; get; }
        public string PO { set; get; }
        public string FinanceNo { set; get; }
        public string BuyDate { set; get; }
        public string Department { set; get; }
        public string IsPM { set; get; }
        public string IsCal { set; get; }
        public string IsKey { set; get; }
        public string ManufactureLocation { set; get; }
        public string ImportLot { set; get; }
        public string LotLine { set; get; }
        public string EquipmentStatus { set; get; }
        public string Remark { set; get; }
        public string QualificationDate { set; get; }
        public string AssetType { set; get; }
        public string EquipmentType { set; get; }
        public string ParentCN { set; get; }
        public string Used { set; get; }
        public string QualificationReportNo { set; get; }
        public string UserId { set; get; }
        public string Cdt { set; get; }
        public string Udt { set; get; }
        public string OriginalValue_RMB { set; get; }
        public string Monthly_depreciation { set; get; }
        public string Accumulated_depreciation { set; get; }
        public string NetValue_RMB { set; get; }
        public string DFNO { set; get; }
        public string Location { set; get; }
        public string Station { set; get; }
        public string ProjectNo { set; get; }
        public string Length { set; get; }
        public string Width { set; get; }
        public string Height { set; get; }
        public string NetWeight { set; get; }
        public string Grade { set; get; }
        public string Voltage { set; get; }
        public string Power { set; get; }
        public string Vacuum { set; get; }
        public string CompressedAir { set; get; }
        public string NitrogenGas { set; get; }
        public string LocationRemark { set; get; }
        public string PictureUrl { set; get; }
        public string Owner { set; get; }
        public string CheckDate { set; get; }
        public string CIP { set; get; }
        public string Actual_User { set; get; }
        public string Asset_Dept { set; get; }

        public List<AssetBorrowHistoryVM> BorrowHistory { set; get; }
        public List<AssetCheckHistoryVM> CheckHistory { set; get; }

        public static List<string> AssetVMProperty()
        {
            return new List<string> { "CN", "Site", "EnglishName", "ChineseName", "Model", "Supplier", "Manufactures", "SN", "PO", "FinanceNo", "BuyDate", "Department", "IsPM", "IsCal", "ISkey", "ManufactureLocation", "ImportLot", "LotLine", "EquipmentStatus", "Remark", "QualificationDate", "AssetType", "EquipmentType", "ParentCN", "Used", "QualificationReportNo", "UserId", "Cdt", "Udt", "OriginalValue_RMB", "Monthly_depreciation", "Accumulated_depreciation", "NetValue_RMB", "DFNO", "Location", "Station", "ProjectNo", "Length", "Width", "Height", "NetWeight", "Grade", "Voltage", "Power", "Vacuum", "CompressedAir", "NitrogenGas", "LocationRemark", "PictureUrl", "Owner", "CheckDate", "CIP", "Actual_User", "Asset_Dept" };
        }
        public static List<string> AssetShowProperty()
        {
            return new List<string> { "CN", "Site", "EnglishName", "ChineseName", "Model", "Supplier", "Manufactures", "Department", "Location", "Station", "ProjectNo", "Owner", "Actual_User", "Asset_Dept" };
        }

        public static List<string> AssetDatetimeProperty()
        {
            return new List<string>() { "BuyDate", "QualificationDate", "Cdt", "Udt", "CheckDate"};
        }

        public static void SaveAssets(List<string> assetlist)
        {
            var exist_asset = getAsset(assetlist);
            if (string.IsNullOrEmpty(exist_asset.ID))
            {
                createAsset(assetlist);
            }
        }
        public static AssetVM getAsset(List<string> assetlist)
        {
            var sql = @"select top(1) * from AssetInfo where CN = @CN";
            var param = new Dictionary<string, string>();
            param.Add("@CN", assetlist[0]);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new AssetVM();
            if (dbret.Count > 0)
            {
                res.ID = Convert.ToString(dbret[0][0]);
                PropertyInfo[] properties = typeof(AssetVM).GetProperties();

                var title_info = AssetVMProperty();

                var idx = 1;
                foreach (var title in title_info)
                {
                    SetPropertyValue(title, Convert.ToString(dbret[0][idx]), res);
                    idx++;
                }
            }

            return res;

        }
        public static void createAsset(List<string> assetlist)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var title_info = AssetVMProperty();
            var param = new Dictionary<string, string>();
            var sql = "insert into AssetInfo (" + String.Join(", ", title_info.ToArray()) + ") values (";
            var m = 0;
            foreach (var item in title_info)
            {
                sql += "@" + item + ",";
                if (string.Compare(assetlist[m].Trim(), "/") == 0)
                {
                    param.Add("@" + item, "");
                }
                else
                {
                    param.Add("@" + item, assetlist[m]);
                }
                m++;
            }
            sql = sql.Substring(0, sql.Length - 1) + ")";
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
        public static void updateAsset(List<string> asset, int id)
        {
            var title_info = AssetVMProperty();
            var param = new Dictionary<string, string>();
            var idx = 0;
            var sql = "update AssetInfo set ";
            foreach (var item in title_info)
            {
                sql += item + "= @" + item + ",";
                param.Add("@" + item, asset[idx]);
                idx++;
            }
            sql += "UpdateAt = @uTime where ID = @ID";
            param.Add("@uTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@ID", id.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string, AssetVM> GetAssetList(string cn = "", string name = "", string status = "" )
        {
            var sql = @"select ai.*, abh.*
                    from AssetInfo as ai 
                    left join AssetBorrowHistory as abh on ai.ID = abh.AssetID
                    where 1 = 1";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(name))
            {
                sql += " and (ai.EnglishName like @name or ai.ChineseName like @name) ";
                param.Add("@name", "%" +name+"%");
            }
            if (!string.IsNullOrEmpty(cn))
            {
                sql += " and ai.CN like @CN ";
                param.Add("@CN", "%" + cn + "%");
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql += " and ai.EquipmentStatus = @Status ";
                param.Add("@Status", status);
            }

            //sql += " order by ai.UpdateAt Desc";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new Dictionary<string, AssetVM>();
            var title_info = AssetVMProperty();
            if (dbret.Count > 0)
            {
                foreach (var item in dbret)
                {
                    var tmp = new AssetBorrowHistoryVM();
                    tmp.ID = Convert.ToString(item[61]);
                    tmp.RequestID = Convert.ToString(item[62]);
                    tmp.AssetID = Convert.ToString(item[63]);
                    tmp.BorrowUser = Convert.ToString(item[64]);
                    tmp.ProjectNo = Convert.ToString(item[65]);
                    tmp.IsLong = Convert.ToString(item[66]);
                    tmp.StartDate = Convert.ToString(item[67]);
                    tmp.EndDate = Convert.ToString(item[68]);
                    tmp.AdminConfirm = Convert.ToString(item[69]);
                    tmp.BorrowComment = Convert.ToString(item[70]);
                    tmp.IsReturn = Convert.ToString(item[71]);
                    tmp.ReturnComment = Convert.ToString(item[72]);
                    tmp.ReturnAt = Convert.ToString(item[73]);
                    tmp.CreateAt = Convert.ToString(item[74]);
                    tmp.UpdateAt = Convert.ToString(item[75]);
                    if (res.ContainsKey(Convert.ToString(item[0])))
                    {
                        res[Convert.ToString(item[0])].BorrowHistory.Add(tmp);
                    }
                    else
                    {
                        var asset_tmp = new AssetVM();
                        asset_tmp.ID = Convert.ToString(item[0]);
                        PropertyInfo[] properties = typeof(AssetVM).GetProperties();

                        var idx = 1;
                        foreach (var title in title_info)
                        {
                            SetPropertyValue(title, Convert.ToString(item[idx]), asset_tmp);
                            idx++;
                        }
                        asset_tmp.BorrowHistory.Add(tmp);
                        res.Add(Convert.ToString(item[0]), asset_tmp);
                    }
                }
                var check_history = AssetCheckHistoryVM.GetAssetCheckHistory(cn, name, status);
                if(check_history.Count > 0) {
                    foreach(var item in res)
                    {
                        if (check_history.ContainsKey(item.Key))
                        {
                            item.Value.CheckHistory = check_history[item.Key];
                        }
                    }
                }
            }
            
            return res;
        }

        public static void SetPropertyValue(string propertyName, string value, object objectInstance)
        {
            try
            {
                objectInstance.GetType().GetProperty(propertyName).SetValue(objectInstance, value);
            }
            catch (Exception e){}
        }

        public static object GetPropertyValue(string propertyName, object objectInstance)
        {
            object res = null;
            try
            {
                res = objectInstance.GetType().GetProperty(propertyName).GetValue(objectInstance, null);
                if (AssetDatetimeProperty().Contains(propertyName))
                {
                    res = (string.Compare(Convert.ToDateTime(res).ToString("yyyy-MM-dd"), "1900-01-01") == 0)? "" : Convert.ToDateTime(res).ToString("yyyy-MM-dd");
                }
            }
            catch (Exception e)
            {
                res = null;
            }
            return res;
        }

        public static List<AssetVM> GetValidAssets()
        {
            var sql = @"select ai.ID, ai.CN, ai.EnglishName, ai.ChineseName from AssetInfo as ai 
                    left join AssetBorrowHistory as abh on ai.ID = abh.AssetID
                    where abh.IsReturn = '2' or abh.IsReturn is null";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            var res = new List<AssetVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new AssetVM();
                    tmp.ID = Convert.ToString(item[0]);
                    tmp.CN = Convert.ToString(item[1]);
                    tmp.EnglishName = Convert.ToString(item[2]);
                    tmp.ChineseName = Convert.ToString(item[3]);
                    res.Add(tmp);
                }
            }

            return res;
        }
    }

    public class AssetBorrowHistoryVM
    {
        public AssetBorrowHistoryVM()
        {
            ID = "";
            RequestID = "";
            AssetID = "";
            BorrowUser = "";
            ProjectNo = "";
            IsLong = "";
            StartDate = "";
            EndDate = "";
            AdminConfirm = "1";
            BorrowComment = "";
            IsReturn = "0";
            ReturnComment = "";
            ReturnAt = "";
            CreateAt = null;
            UpdateAt = null;
            AssetList = new List<AssetVM>();
        }
        public string ID { set; get; }
        public string RequestID { set; get; }
        public string AssetID { set; get; }
        public string BorrowUser { set; get; }
        public string ProjectNo { set; get; }
        public string IsLong { set; get; }
        public string StartDate { set; get; }
        public string EndDate { set; get; }
        public string AdminConfirm { set; get; }
        public string BorrowComment { set; get; }
        public string IsReturn { set; get; }
        public string ReturnComment { set; get; }
        public string ReturnAt { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }
        public List<AssetVM> AssetList { set; get; }

        public static Dictionary<string, AssetBorrowHistoryVM> GetBorrowList(string key = "")
        {
            var sql = @"select abh.*, ai.CN, ai.EnglishName, ai.ChineseName
                    from AssetBorrowHistory as abh 
                    left join AssetInfo as ai on abh.AssetID = ai.ID
                    where 1 = 1";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(key))
            {
                sql += " and (ai.EnglishName like @key or ai.ChineseName like @key or ai.CN like @key)";
                param.Add("@key", "%" + key + "%");
            }

            sql += " Order by abh.UpdateAt Desc";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new Dictionary<string, AssetBorrowHistoryVM>();
            if (dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var asset_tmp = new AssetVM();
                    asset_tmp.ID = Convert.ToString(item[2]);
                    asset_tmp.CN = Convert.ToString(item[15]);
                    asset_tmp.EnglishName = Convert.ToString(item[16]);
                    asset_tmp.ChineseName = Convert.ToString(item[17]);
                    if (res.ContainsKey(Convert.ToString(item[1])))
                    {
                        res[Convert.ToString(item[1])].AssetList.Add(asset_tmp);
                    }
                    else
                    {
                        var tmp = new AssetBorrowHistoryVM();
                        tmp.ID = Convert.ToString(item[0]);
                        tmp.RequestID = Convert.ToString(item[1]);
                        tmp.BorrowUser = Convert.ToString(item[3]);
                        tmp.ProjectNo = Convert.ToString(item[4]);
                        tmp.IsLong = Convert.ToString(item[5]);
                        tmp.StartDate = Convert.ToString(item[6]);
                        tmp.EndDate = Convert.ToString(item[7]);
                        tmp.AdminConfirm = Convert.ToString(item[8]);
                        tmp.BorrowComment = Convert.ToString(item[9]);
                        tmp.IsReturn = Convert.ToString(item[10]);
                        tmp.ReturnComment = Convert.ToString(item[11]);
                        tmp.ReturnAt = Convert.ToString(item[12]);
                        tmp.CreateAt = Convert.ToString(item[13]);
                        tmp.UpdateAt = Convert.ToString(item[14]);
                        var asset_list = new List<AssetVM>();
                        asset_list.Add(asset_tmp);
                        tmp.AssetList = asset_list;
                        res.Add(Convert.ToString(item[0]), tmp);
                    }
                }

            }

            return res;
        }

        public static void NewBorrow(List<AssetBorrowHistoryVM> borrow)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into AssetBorrowHistory (RequestID, AssetID, BorrowUser, ProjectNo, 
                    IsLong, StartDate, EndDate, AdminConfirm, BorrowComment, IsReturn, ReturnComment,
                    ReturnAt, CreateAt, UpdateAt) values ";
            var param = new Dictionary<string, string>();
            var idx = 0;
            foreach(var item in borrow)
            {
                sql += "( @RequestID_" + idx + ", @AssetID_" + idx + ", @BorrowUser_" + idx + ", @ProjectNo_" + idx + ", @IsLong_" + idx
                        + ", @StartDate_" + idx + ", @EndDate_" + idx + ", @AdminConfirm_" + idx + ", @BorrowComment_"
                        + idx + ", @IsReturn_" + idx + ", @ReturnComment_" + idx + ", @ReturnAt_" + idx + ", @CreateAt_" + idx + ", @UpdateAt_" + idx + "),";
                param.Add("@RequestID_" + idx, item.RequestID);
                param.Add("@AssetID_" + idx, item.AssetID);
                param.Add("@BorrowUser_" + idx, item.BorrowUser.ToUpper());
                param.Add("@ProjectNo_" + idx, item.ProjectNo);
                param.Add("@IsLong_" + idx, item.IsLong);
                param.Add("@StartDate_" + idx, item.StartDate);
                param.Add("@EndDate_" + idx, item.EndDate);
                param.Add("@AdminConfirm_" + idx, item.AdminConfirm);
                param.Add("@BorrowComment_" + idx, item.BorrowComment);
                param.Add("@IsReturn_" + idx, item.IsReturn);
                param.Add("@ReturnComment_" + idx, item.ReturnComment);
                param.Add("@ReturnAt_" + idx, item.ReturnAt);
                param.Add("@CreateAt_" + idx, now);
                param.Add("@UpdateAt_" + idx, now);
                idx++;
            }
            sql = sql.Substring(0, sql.Length - 1);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void UpdateBorrow(AssetBorrowHistoryVM borrow)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var param = new Dictionary<string, string>();
            var sql = @"update AssetBorrowHistory set ";
            if (!string.IsNullOrEmpty(borrow.AssetID))
            {
                sql += " AssetID = @AssetID, ";
                param.Add("@AssetID", borrow.AssetID);
            }
            if ( ! string.IsNullOrEmpty(borrow.BorrowUser))
            {
                sql += " BorrowUser = @uName, ";
                param.Add("@uName", borrow.BorrowUser);
            }
            if (!string.IsNullOrEmpty(borrow.BorrowComment))
            {
                sql += " BorrowComment = @BorrowComment, ";
                param.Add("@BorrowComment", borrow.BorrowComment);
            }
            if (!string.IsNullOrEmpty(borrow.IsReturn))
            {
                sql += " IsReturn = @IsReturn, ";
                param.Add("@IsReturn", borrow.IsReturn);
            }
            if (!string.IsNullOrEmpty(borrow.ReturnComment))
            {
                sql += " ReturnComment = @ReturnComment, ";
                param.Add("@ReturnComment", borrow.ReturnComment);
            }
            if (!string.IsNullOrEmpty(borrow.ReturnAt))
            {
                sql += " ReturnAt = @ReturnAt ,";
                param.Add("@ReturnAt", borrow.ReturnAt);
            }
            sql += " UpdateAt = @UpdateAt where ID = @ID";
            param.Add("@UpdateAt", now);
            param.Add("@ID", borrow.ID);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
    }

    public class AssetBuyInfoVM
    {
        public AssetBuyInfoVM()
        {
            ID = "";
            EngName = "";
            ChName = "";
            UnitPrice = "";
            Brand = "";
            Model = "";
            OriginCountry = "";
            Picture = "";
            Purpose = "";
            Functions = "";
            WorkPrinciple = "";
            CorporatePurposes = "";
            Status = "1";
            CreateAt = "";
            UpdateAt = "";
        }

        public string ID { set; get; }
        public string EngName { set; get; }
        public string ChName { set; get; }
        public string UnitPrice { set; get; }
        public string Brand { set; get; }
        public string Model { set; get; }
        public string OriginCountry { set; get; }
        public string Picture { set; get; }
        public string Purpose { set; get; }
        public string Functions { set; get; }
        public string WorkPrinciple { set; get; }
        public string CorporatePurposes { set; get; }
        public string Status { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }

        public static List<AssetBuyInfoVM> GetAssetBuyInfo(string key = "", string id = "")
        {
            var sql = @"select abi.* from AssetBuyInfo as abi where Status != @Status ";
            var param = new Dictionary<string, string>();
            param.Add("@Status", BuyStatus.Invalid.ToString());
            if (!string.IsNullOrEmpty(id))
            {
                sql += " and abi.ID = @ID ";
                param.Add("@ID", id);
            }
            if (!string.IsNullOrEmpty(key))
            {
                sql += " and abi.EngName like @key or abi.ChName like @key ";
                param.Add("@key", "%" + key + "%");
            }
            sql += " order by abi.UpdateAt Desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new List<AssetBuyInfoVM>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new AssetBuyInfoVM();
                    tmp.ID = Convert.ToString(item[0]);
                    tmp.EngName = Convert.ToString(item[1]);
                    tmp.ChName = Convert.ToString(item[2]);
                    tmp.UnitPrice = Convert.ToString(item[3]);
                    tmp.Brand = Convert.ToString(item[4]);
                    tmp.Model = Convert.ToString(item[5]);
                    tmp.OriginCountry = Convert.ToString(item[6]);
                    tmp.Picture = Convert.ToString(item[7]);
                    tmp.Purpose = Convert.ToString(item[8]);
                    tmp.Functions = Convert.ToString(item[9]);
                    tmp.WorkPrinciple = Convert.ToString(item[10]);
                    tmp.CorporatePurposes = Convert.ToString(item[11]);
                    tmp.Status = Convert.ToString(item[12]);
                    tmp.CreateAt = Convert.ToString(item[13]);
                    tmp.UpdateAt = Convert.ToString(item[14]);
                    res.Add(tmp);
                }
            }

            return res;
        }

        public static void AddBuyRequest(AssetBuyInfoVM buy)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into AssetBuyInfo (EngName, ChName, UnitPrice,
                    Brand, Model, OriginCountry, Picture, Purpose, Functions,
                    WorkPrinciple, CorporatePurposes, Status, CreateAt, UpdateAt) 
                    values (@EngName, @ChName, @UnitPrice,
                    @Brand, @Model, @OriginCountry, @Picture, @Purpose, @Functions,
                    @WorkPrinciple, @CorporatePurposes, @Status, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@EngName", buy.EngName);
            param.Add("@ChName", buy.ChName);
            param.Add("@UnitPrice", buy.UnitPrice);
            param.Add("@Brand", buy.Brand);
            param.Add("@Model", buy.Model);
            param.Add("@OriginCountry", buy.OriginCountry);
            param.Add("@Picture", buy.Picture);
            param.Add("@Purpose", buy.Purpose);
            param.Add("@Functions", buy.Functions);
            param.Add("@WorkPrinciple", buy.WorkPrinciple);
            param.Add("@CorporatePurposes", buy.CorporatePurposes);
            param.Add("@Status", BuyStatus.Request.ToString());
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static void UpdateBuyRequest(AssetBuyInfoVM buy)
        {
            var sql = "update AssetBuyInfo set ";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(buy.EngName))
            {
                sql += " EngName = @EngName, ";
                param.Add("@EngName", buy.EngName);
            }
            if (!string.IsNullOrEmpty(buy.ChName))
            {
                sql += " ChName = @ChName, ";
                param.Add("@ChName", buy.ChName);
            }
            if (!string.IsNullOrEmpty(buy.UnitPrice))
            {
                sql += " UnitPrice = @UnitPrice, ";
                param.Add("@UnitPrice", buy.UnitPrice);
            }
            if (!string.IsNullOrEmpty(buy.Brand))
            {
                sql += " Brand = @Brand, ";
                param.Add("@Brand", buy.Brand);
            }
            if (!string.IsNullOrEmpty(buy.Model))
            {
                sql += " Model = @Model, ";
                param.Add("@Model", buy.Model);
            }
            if (!string.IsNullOrEmpty(buy.OriginCountry))
            {
                sql += " OriginCountry = @OriginCountry, ";
                param.Add("@OriginCountry", buy.OriginCountry);
            }
            if (!string.IsNullOrEmpty(buy.Picture))
            {
                sql += " Picture = @Picture, ";
                param.Add("@Picture", buy.Picture);
            }
            if (!string.IsNullOrEmpty(buy.Purpose))
            {
                sql += " Purpose = @Purpose, ";
                param.Add("@Purpose", buy.Purpose);
            }
            if (!string.IsNullOrEmpty(buy.Functions))
            {
                sql += " Functions = @Functions, ";
                param.Add("@Functions", buy.Functions);
            }
            if (!string.IsNullOrEmpty(buy.WorkPrinciple))
            {
                sql += " WorkPrinciple = @WorkPrinciple, ";
                param.Add("@WorkPrinciple", buy.WorkPrinciple);
            }
            if (!string.IsNullOrEmpty(buy.CorporatePurposes))
            {
                sql += " CorporatePurposes = @CorporatePurposes, ";
                param.Add("@CorporatePurposes", buy.CorporatePurposes);
            }
            if (!string.IsNullOrEmpty(buy.Status))
            {
                sql += " Status = @Status, ";
                param.Add("@Status", buy.Status);
            }

            sql += " UpdateAt = @UpdateAt where ID = @ID";
            param.Add("@UpdateAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            param.Add("@ID", buy.ID);

            DBUtility.ExeLocalSqlNoRes(sql, param);

        }
    }

    public class AssetCheckHistoryVM
    {
        public AssetCheckHistoryVM()
        {
            ID = "";
            AssetID = "";
            Status = "1";
            Comment = "";
            CreateAt = "";
            UpdateAt = "";
        }

        public string ID { set; get; }
        public string AssetID { set; get; }
        public string Status { set; get; }
        public string Comment { set; get; }
        public string CreateAt { set; get; }
        public string UpdateAt { set; get; }


        public static Dictionary<string, List<AssetCheckHistoryVM>> GetAssetCheckHistory(string cn = "", string name = "", string status = "")
        {
            var sql = @"select ach.* from AssetCheckHistory as ach 
                    inner join AssetInfo as ai on ach.AssetID = ai.ID
                    where 1 = 1";
            var param = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(name))
            {
                sql += " and (ai.EnglishName like @name or ai.ChineseName like @name) ";
                param.Add("@name", "%" + name + "%");
            }
            if (!string.IsNullOrEmpty(cn))
            {
                sql += " and ai.CN like @CN ";
                param.Add("@CN", "%" + cn + "%");
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql += " and ai.EquipmentStatus = @Status ";
                param.Add("@Status", status);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new Dictionary<string, List<AssetCheckHistoryVM>>();
            if(dbret.Count > 0)
            {
                foreach(var item in dbret)
                {
                    var tmp = new AssetCheckHistoryVM();
                    tmp.ID = Convert.ToString(item[0]);
                    tmp.AssetID = Convert.ToString(item[1]);
                    tmp.Status = Convert.ToString(item[2]);
                    tmp.Comment = Convert.ToString(item[3]);
                    tmp.CreateAt = Convert.ToString(item[4]);
                    tmp.UpdateAt = Convert.ToString(item[5]);
                    if (res.ContainsKey(Convert.ToString(item[1])))
                    {
                        res[Convert.ToString(item[1])].Add(tmp);
                    }
                    else
                    {
                        res.Add(Convert.ToString(item[1]), new List<AssetCheckHistoryVM> { tmp });
                    }
                }
            }
            return res;
        }

        public static void AddCheckHistory(AssetCheckHistoryVM check)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"insert into AssetCheckHistory (AssetID, Status, 
                    Comment, CreateAt, UpdateAt) values ( @AssetID, @Status, 
                    @Comment, @CreateAt, @UpdateAt)";
            var param = new Dictionary<string, string>();
            param.Add("@AssetID", check.AssetID);
            param.Add("@Status", CheckStatus.Valid.ToString());
            param.Add("@Comment", check.Comment);
            param.Add("@CreateAt", now);
            param.Add("@UpdateAt", now);

            DBUtility.ExeLocalSqlNoRes(sql, param);

        }

        public static AssetCheckHistoryVM GetCheckInfo(string assetid)
        {
            var sql = @"select * from AssetCheckHistory where AssetID = @assetid and Status = @Status ";
            var param = new Dictionary<string, string>();
            param.Add("@assetid", assetid);
            param.Add("@Status", CheckStatus.Valid.ToString());
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, param);
            var res = new AssetCheckHistoryVM();
            if(dbret.Count > 0)
            {
                res.ID = Convert.ToString(dbret[0][0]);
                res.AssetID = Convert.ToString(dbret[0][1]);
                res.Status = Convert.ToString(dbret[0][2]);
                res.Comment = Convert.ToString(dbret[0][3]);
                res.CreateAt = Convert.ToString(dbret[0][4]);
                res.UpdateAt = Convert.ToString(dbret[0][5]);
            }
            return res;
        }

        public static void UpdateCheckHistory(AssetCheckHistoryVM check)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = @"update AssetCheckHistory set ";
            var param = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(check.Comment))
            {
                sql += " Comment = @Comment, ";
                param.Add("@Comment", check.Comment);
            }
            if (!String.IsNullOrEmpty(check.Status))
            {
                sql += " Status = @Status, ";
                param.Add("@Status", check.Status);
            }
            sql += " UpdateAt = @UpdateAt where ID = @ID";
            param.Add("@UpdateAt", now);
            param.Add("@ID", check.ID);
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }
    }
}