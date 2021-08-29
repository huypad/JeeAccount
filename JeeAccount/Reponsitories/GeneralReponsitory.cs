using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.JeeHR;
using JeeAccount.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public static class GeneralReponsitory
    {
        public static bool IsExist(string sql, string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
                return true;
            }
        }

        public static bool IsExistCnn(string sql, DpsConnection cnn)
        {
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsExistCnn(string sql, DpsConnection cnn, SqlConditions conds)
        {
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql, conds);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsActiveByUserIDCnn(DpsConnection cnn, string UserId)
        {
            string sql = $"select * from AccountList where UserId = {UserId} and IsActive = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsActiveByUsernameCnn(DpsConnection cnn, string username)
        {
            string sql = $"select * from AccountList where username = '{username}' and IsActive = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsAdminByUsernameCnn(DpsConnection cnn, string username)
        {
            string sql = $"select * from AccountList where username = '{username} 'and IsAdmin = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static CommonInfo GetCommonInfoCnn(DpsConnection cnn, long UserID = 0, string Username = "", long StaffID = 0)
        {
            string where = "";

            if (UserID > 0)
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += $" UserID = {UserID}";
                }
            }
            if (!string.IsNullOrEmpty(Username))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += $" Username = '{Username}'";
                }
                else
                {
                    where += $" or Username = '{Username}'";
                }
            }
            if (StaffID > 0)
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += $" StaffID = {StaffID}";
                }
                else
                {
                    where += $" or StaffID = {StaffID}";
                }
            }
            if (string.IsNullOrEmpty(where)) throw new Exception("UserID or Username or Staffid");
            string sql = $"select UserID, Username, StaffID, CustomerID from AccountList where {where}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) throw new Exception("UserID or Username or Staffid");
            return dt.AsEnumerable().Select(row => new CommonInfo
            {
                CustomerID = Convert.ToInt32(row["CustomerID"]),
                StaffID = row["StaffID"] != DBNull.Value ? Convert.ToInt64(row["StaffID"]) : 0,
                UserID = Convert.ToInt64(row["UserID"]),
                Username = row["Username"].ToString()
            }).SingleOrDefault();
        }

        public static CommonInfo GetCommonInfo(string connectionString, long UserID = 0, string Username = "", long StaffID = 0)
        {
            string where = "";

            if (UserID > 0)
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += $" UserID = {UserID}";
                }
            }
            if (!string.IsNullOrEmpty(Username))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += $" Username = '{Username}'";
                }
                else
                {
                    where += $" or Username = '{Username}'";
                }
            }
            if (StaffID > 0)
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += $" StaffID = {StaffID}";
                }
                else
                {
                    where += $" or StaffID = {StaffID}";
                }
            }
            if (string.IsNullOrEmpty(where)) throw new ArgumentNullException("UserID or Username or Staffid");
            string sql = $"select UserID, Username, StaffID, CustomerID from AccountList where {where}";
            DataTable dt = new DataTable();
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) throw new ArgumentNullException("UserID or Username or Staffid");
                return dt.AsEnumerable().Select(row => new CommonInfo
                {
                    CustomerID = Convert.ToInt32(row["CustomerID"]),
                    StaffID = row["StaffID"] != DBNull.Value ? Convert.ToInt64(row["StaffID"]) : 0,
                    UserID = Convert.ToInt64(row["UserID"]),
                    Username = row["Username"].ToString()
                }).SingleOrDefault();
            }
        }

        public static List<long> GetLstCustomerid(string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var ListCustomerid = new List<long>();
                string sql = $"select RowID from CustomerList where RowID > 0";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    ListCustomerid.Add(GeneralService.ConvertToLong(dr["RowID"]));
                }
                return ListCustomerid;
            }
        }

        public static CommonInfo GetCommonInfoByInputApiModel(string connectionString, InputApiModel model)
        {
            var commonInfo = new CommonInfo();
            if (!string.IsNullOrEmpty(model.Username) && string.IsNullOrEmpty(model.Userid))
            {
                commonInfo = GetCommonInfo(connectionString, 0, model.Username);
            }
            else
            {
                commonInfo = GetCommonInfo(connectionString, long.Parse(model.Userid));
            }
            return commonInfo;
        }

        public static CommonInfo GetCommonInfoByInputApiModelCnn(DpsConnection cnn, InputApiModel model)
        {
            var commonInfo = new CommonInfo();
            if (!string.IsNullOrEmpty(model.Username) && string.IsNullOrEmpty(model.Userid))
            {
                commonInfo = GetCommonInfoCnn(cnn, 0, model.Username);
            }
            else
            {
                commonInfo = GetCommonInfoCnn(cnn, long.Parse(model.Userid));
            }
            return commonInfo;
        }

        public static List<long> GetLstCustomeridCnn(DpsConnection cnn)
        {
            var ListCustomerid = new List<long>();
            string sql = $"select RowID from CustomerList where RowID > 0";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                ListCustomerid.Add(GeneralService.ConvertToLong(dr["RowID"]));
            }
            return ListCustomerid;
        }

        public static List<long> GetLstUserIDByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var ListCustomerid = new List<long>();
                string sql = $"select UserID from AccountList where CustomerID = {customerid} and Disable = 0";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    ListCustomerid.Add(GeneralService.ConvertToLong(dr["UserID"]));
                }
                return ListCustomerid;
            }
        }

        public static List<long> GetLstUserIDByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var ListCustomerid = new List<long>();
            string sql = $"select UserID from AccountList where CustomerID = {customerid} and Disable = 0";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                ListCustomerid.Add(GeneralService.ConvertToLong(dr["UserID"]));
            }
            return ListCustomerid;
        }

        public static List<string> GetLstUsernameByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var lst = new List<string>();
                string sql = $"select Username from AccountList where CustomerID = {customerid} and Disable = 0";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(dr["Username"].ToString());
                }
                return lst;
            }
        }

        public static List<long> GetLstStaffIDByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var lst = new List<long>();
                string sql = $"select StaffID from AccountList where CustomerID = {customerid} and Disable = 0";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(long.Parse(dr["StaffID"].ToString()));
                }
                return lst;
            }
        }

        public static List<long> GetLstStaffIDByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var lst = new List<long>();
            string sql = $"select StaffID from AccountList where CustomerID = {customerid} and Disable = 0";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                lst.Add(long.Parse(dr["StaffID"].ToString()));
            }
            return lst;
        }

        public static bool IsAdminHeThongCnn(DpsConnection cnn, long UserID)
        {
            string sql1 = $"select * from AccountList where IsAdmin = 1 and UserID = {UserID} and Disable = 0";
            DataTable dtCheck = cnn.CreateDataTable(sql1);
            if (dtCheck.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsAdminAppCnn(DpsConnection cnn, long UserID, int AppID)
        {
            string sql1 = $"select * from Account_App where UserID = {UserID} and AppID = {AppID} and IsAdmin = 1 and Account_App.Disable = 0";
            DataTable dtCheck = cnn.CreateDataTable(sql1);
            if (dtCheck.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsAdminHeThong(string connectionString, long UserID)
        {
            string sql1 = $"select * from AccountList where IsAdmin = 1 and UserID = {UserID}";
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql1);
                if (dt.Rows.Count == 0) return false;
            }
            return true;
        }

        public static bool IsAdminApp(string connectionString, long UserID, int AppID)
        {
            string sql1 = $"select * from Account_App where UserID = {UserID} and AppID = {AppID} and IsAdmin = 1 and Account_App.Disable = 0";
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql1);
                if (dt.Rows.Count == 0) return false;
            }
            return true;
        }

        public static List<string> GetLstUsernameByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var lst = new List<string>();
            string sql = $"select Username from AccountList where CustomerID = {customerid} and Disable = 0";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                lst.Add(dr["Username"].ToString());
            }
            return lst;
        }

        public static bool IsUsedJeeHRCustomeridCnn(DpsConnection cnn, long customerid)
        {
            string sql = $"select * from Customer_App where AppID = 1 and CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsUsedJeeHRCustomerid(string connectionString, long customerid)
        {
            string sql = $"select * from Customer_App where AppID = 1 and CustomerID = {customerid}";
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
            }
            return true;
        }

        public static PersonalInfoCustomData GetPersonalInfoCustomData(long UserID, long CustomerID, string connectionString)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConditions Conds = new SqlConditions();
                using (DpsConnection cnn = new DpsConnection(connectionString))
                {
                    Conds.Add("CustomerID", CustomerID);
                    Conds.Add("UserID", UserID);

                    var isJeeHR = IsUsedJeeHRCustomeridCnn(cnn, CustomerID);

                    string SQL_DSINFO_DEFAULT = @"select UserID, Username, Email, LastName +' ' + FirstName as FullName
                           , FirstName as Name, LastName, AvartarImgURL as Avatar, JobtitleList.JobtitleName as Jobtitle, JobtitleID , DepartmentID,
                           DepartmentList.DepartmentName as Department, PhoneNumber, AccountList.CustomerID, cocauid, ChucVuID, Birthday, DirectManager,
                           AccountList.IsActive, IsAdmin, AccountList.Note
                           from AccountList
left join DepartmentList on DepartmentList.RowID = AccountList.DepartmentID
left join JobtitleList on JobtitleList.RowID = AccountList.JobtitleID where AccountList.CustomerID = @CustomerID and AccountList.UserID = @UserID";

                    string SQL_DSINFO_JEEHR = @"select UserID, Username, email, LastName +' '+FirstName as FullName,
                           FirstName as Name, LastName, AvartarImgURL as Avatar, Jobtitle, JobtitleID, IsActive, IsAdmin, Note,
                           Department, DepartmentID, PhoneNumber, CustomerID, cocauid, ChucVuID, Birthday, DirectManager
                           from AccountList where AccountList.CustomerID = @CustomerID and AccountList.UserID = @UserID";

                    string sql = SQL_DSINFO_DEFAULT;
                    if (isJeeHR) sql = SQL_DSINFO_JEEHR;

                    dt = cnn.CreateDataTable(sql, Conds);
                    if (dt.Rows.Count == 0)
                        throw new KhongCoDuLieuException("Tài khoản");

                    return new PersonalInfoCustomData
                    {
                        Avatar = (dt.Rows[0]["Avatar"] != DBNull.Value) ? dt.Rows[0]["Avatar"].ToString() : "",
                        Birthday = (dt.Rows[0]["Birthday"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["Birthday"]).ToString("dd/MM/yyyy") : "",
                        Departmemt = (dt.Rows[0]["Department"] != DBNull.Value) ? dt.Rows[0]["Department"].ToString() : "",
                        Fullname = (dt.Rows[0]["Fullname"] != DBNull.Value) ? dt.Rows[0]["Fullname"].ToString() : "",
                        Jobtitle = (dt.Rows[0]["Jobtitle"] != DBNull.Value) ? dt.Rows[0]["Jobtitle"].ToString() : "",
                        Name = (dt.Rows[0]["Name"] != DBNull.Value) ? dt.Rows[0]["Name"].ToString() : "",
                        Phonenumber = (dt.Rows[0]["Phonenumber"] != DBNull.Value) ? dt.Rows[0]["PhoneNumber"].ToString() : "",
                        StructureID = (dt.Rows[0]["cocauid"] != DBNull.Value) ? dt.Rows[0]["cocauid"].ToString() : "",
                        BgColor = GeneralService.GetColorNameUser(dt.Rows[0]["Name"].ToString().Substring(0, 1)),
                        DepartmemtID = (dt.Rows[0]["DepartmentID"] != DBNull.Value) ? dt.Rows[0]["DepartmentID"].ToString() : "",
                        Email = (dt.Rows[0]["Email"] != DBNull.Value) ? dt.Rows[0]["Email"].ToString() : "",
                        JobtitleID = (dt.Rows[0]["JobtitleID"] != DBNull.Value) ? dt.Rows[0]["JobtitleID"].ToString() : "",
                        Structure = (dt.Rows[0]["Department"] != DBNull.Value) ? dt.Rows[0]["Department"].ToString() : "",
                        ChucvuID = (dt.Rows[0]["ChucvuID"] != DBNull.Value) ? dt.Rows[0]["ChucvuID"].ToString() : "",
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<HttpResponseMessage> UpdateJeeAccountCustomDataByInputApiModel(long StaffID, string connectionString, string jwt_internal)
        {
            try
            {
                var indentityController = new IdentityServerController();
                var commonInfo = GetCommonInfo(connectionString, 0, "", StaffID);
                var appCodes = GetListAppByUserID(connectionString, commonInfo.UserID, commonInfo.CustomerID, true);
                var appCodesName = appCodes.Select(x => x.AppCode).ToList();
                var objCustom = new ObjCustomData();
                objCustom.userId = commonInfo.UserID;
                objCustom.updateField = "jee-account";
                objCustom.fieldValue = new JeeAccountCustomDataModel
                {
                    AppCode = appCodesName,
                    CustomerID = commonInfo.CustomerID,
                    StaffID = commonInfo.StaffID,
                    UserID = commonInfo.UserID
                };
                var reponse = await indentityController.UpdateCustomDataInternal(jwt_internal, commonInfo.Username, objCustom);
                return reponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SaveStaffID(long UserID, long staffID, string connectionString)
        {
            Hashtable val = new Hashtable();

            val.Add("StaffID", staffID);

            SqlConditions conditions = new SqlConditions();
            conditions.Add("UserID", UserID);

            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                int x = cnn.Update(val, conditions, "AccountList");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
        }

        public static void InsertAppCodeJeeHRKafka(string connectionString, long UserID, bool IsAdmin)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);

            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql2 = @$"select AppID from Account_App where UserID = @UserID and AppID = 1 and (Disable = 0 or Disable is null)";
                var dtnew = cnn.CreateDataTable(sql2, Conds);
                if (dtnew.Rows.Count == 0)
                {
                    Hashtable val = new Hashtable();
                    val.Add("UserID", UserID);
                    val.Add("AppID", 1);
                    val.Add("CreatedDate", DateTime.Now.ToUniversalTime());
                    val.Add("CreatedBy", "kafka");
                    val.Add("Disable", 0);
                    if (!IsAdmin) val.Add("IsAdmin", 0);
                    if (IsAdmin) val.Add("IsAdmin", 1);
                    val.Add("IsActive", 1);
                    int x = cnn.Insert(val, "Account_App");
                    if (x <= 0)
                    {
                        throw cnn.LastError;
                    }
                }
            }
        }

        public static void RemoveAppCodeJeeHRKafka(string connectionString, long UserID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);

            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql2 = @$"select AppID from Account_App where UserID = @UserID and AppID = 1 and (Disable = 0 or Disable is null)";
                var dtnew = cnn.CreateDataTable(sql2, Conds);

                if (dtnew.Rows.Count > 0)
                {
                    Conds.Add("AppID", 1);
                    Hashtable val = new Hashtable();
                    val.Add("Disable", 1);
                    int x = cnn.Update(val, Conds, "Account_App");
                    if (x <= 0)
                    {
                        throw cnn.LastError;
                    }
                }
            }
        }

        public static void SaveStaffIDCnn(long UserID, long staffID, DpsConnection cnn)
        {
            Hashtable val = new Hashtable();
            val.Add("StaffID", staffID);
            SqlConditions conditions = new SqlConditions();
            conditions.Add("UserID", UserID);

            int x = cnn.Update(val, conditions, "AccountList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }

        public static string IDENT_CURRENTCnn(string Tablename, DpsConnection cnn)
        {
            try
            {
                return cnn.ExecuteScalar($"SELECT IDENT_CURRENT ('{Tablename}') AS Current_Identity;").ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<IEnumerable<AppListDTO>> GetListAppByUserIDAsync(string connectionString, long UserID, long CustomerID, bool IsActive = true)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);
            string selection = " AppList.*, Account_App.IsActive, Account_App.IsAdmin as AdminApp";
            string join = @"join Account_App on Account_App.UserID = AccountList.UserID
join AppList on AppList.AppID = Account_App.AppID";
            string where = "where AccountList.UserID = @UserID and (AccountList.Disable = 0 or AccountList.Disable is null) and AccountList.Disable = 0";

            selection += " , Customer_App.SoLuongNhanSu, Customer_App.StartDate, Customer_App.EndDate ";
            join += " join Customer_App on Customer_App.AppID = AppList.AppID ";
            where += " and Customer_App.CustomerID = @CustomerID";
            Conds.Add("CustomerID", CustomerID);

            if (IsActive)
            {
                where += " and  Account_App.IsActive = 1 ";
            }

            string sql = @$"select {selection} from AccountList {join} {where} and Account_App.Disable = 0 order by Position";

            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);

                var result = dt.AsEnumerable().Select(row => new AppListDTO
                {
                    EndDate = (dt.Rows[0]["EndDate"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["EndDate"]).ToString("dd/MM/yyyy") : "",
                    StartDate = (dt.Rows[0]["StartDate"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["StartDate"]).ToString("dd/MM/yyyy") : "",
                    AppID = Int32.Parse(row["AppID"].ToString()),
                    APIUrl = row["APIUrl"].ToString(),
                    AppCode = row["AppCode"].ToString(),
                    AppName = row["AppName"].ToString(),
                    BackendURL = row["BackendURL"].ToString(),
                    CurrentVersion = row["CurrentVersion"].ToString(),
                    Description = row["Description"].ToString(),
                    LastUpdate = row["LastUpdate"].ToString(),
                    Note = row["Note"].ToString(),
                    ReleaseDate = row["ReleaseDate"].ToString(),
                    Icon = row["Icon"].ToString(),
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString()),
                    SoLuongNhanSu = (row["SoLuongNhanSu"] != DBNull.Value) ? Int32.Parse(row["SoLuongNhanSu"].ToString()) : 0,
                    IsShowApp = Convert.ToBoolean(row["IsShowApp"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    IsAdmin = Convert.ToBoolean(row["AdminApp"]),
                });
                return result;
            }
        }

        public static async Task<IEnumerable<AppListDTO>> GetListAppByUserIDAsyncCnn(DpsConnection cnn, long UserID, long CustomerID, bool IsActive = true)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);
            string selection = " AppList.*, Account_App.IsActive, Account_App.IsAdmin as AdminApp";
            string join = @"join Account_App on Account_App.UserID = AccountList.UserID
join AppList on AppList.AppID = Account_App.AppID";
            string where = "where AccountList.UserID = @UserID and (AccountList.Disable = 0 or AccountList.Disable is null) and AccountList.Disable = 0";

            selection += " , Customer_App.SoLuongNhanSu, Customer_App.StartDate, Customer_App.EndDate ";
            join += " join Customer_App on Customer_App.AppID = AppList.AppID ";
            where += " and Customer_App.CustomerID = @CustomerID";
            Conds.Add("CustomerID", CustomerID);

            if (IsActive)
            {
                where += " and  Account_App.IsActive = 1";
            }

            string sql = @$"select {selection} from AccountList {join} {where} and Account_App.Disable = 0 order by Position";

            dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);

            var result = dt.AsEnumerable().Select(row => new AppListDTO
            {
                EndDate = (dt.Rows[0]["EndDate"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["EndDate"]).ToString("dd/MM/yyyy") : "",
                StartDate = (dt.Rows[0]["StartDate"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["StartDate"]).ToString("dd/MM/yyyy") : "",
                AppID = Int32.Parse(row["AppID"].ToString()),
                APIUrl = row["APIUrl"].ToString(),
                AppCode = row["AppCode"].ToString(),
                AppName = row["AppName"].ToString(),
                BackendURL = row["BackendURL"].ToString(),
                CurrentVersion = row["CurrentVersion"].ToString(),
                Description = row["Description"].ToString(),
                LastUpdate = row["LastUpdate"].ToString(),
                Note = row["Note"].ToString(),
                ReleaseDate = row["ReleaseDate"].ToString(),
                Icon = row["Icon"].ToString(),
                Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString()),
                SoLuongNhanSu = (row["SoLuongNhanSu"] != DBNull.Value) ? Int32.Parse(row["SoLuongNhanSu"].ToString()) : 0,
                IsShowApp = Convert.ToBoolean(row["IsShowApp"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                IsAdmin = Convert.ToBoolean(row["AdminApp"]),
            });
            return result;
        }

        public static IEnumerable<AppListDTO> GetListAppByUserID(string connectionString, long UserID, long CustomerID, bool CheckIsActive = true)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);
            string selection = " AppList.*, Account_App.IsActive, Account_App.IsAdmin as AdminApp";
            string join = @"join Account_App on Account_App.UserID = AccountList.UserID
join AppList on AppList.AppID = Account_App.AppID";
            string where = "where AccountList.UserID = @UserID and (AccountList.Disable = 0 or AccountList.Disable is null) and AccountList.Disable = 0";
            if (CustomerID > 0)
            {
                selection += " , Customer_App.SoLuongNhanSu, Customer_App.StartDate, Customer_App.EndDate ";
                join += " join Customer_App on Customer_App.AppID = AppList.AppID ";
                where += " and Customer_App.CustomerID = @CustomerID";
                Conds.Add("CustomerID", CustomerID);
            }
            if (CheckIsActive)
            {
                where += " and Account_App.IsActive = 1";
            }

            string sql = @$"select {selection} from AccountList {join} {where} and Account_App.Disable = 0 order by Position";

            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => new AppListDTO
                {
                    EndDate = (dt.Rows[0]["EndDate"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["EndDate"]).ToString("dd/MM/yyyy") : "",
                    StartDate = (dt.Rows[0]["StartDate"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["StartDate"]).ToString("dd/MM/yyyy") : "",
                    AppID = Int32.Parse(row["AppID"].ToString()),
                    APIUrl = row["APIUrl"].ToString(),
                    AppCode = row["AppCode"].ToString(),
                    AppName = row["AppName"].ToString(),
                    BackendURL = row["BackendURL"].ToString(),
                    CurrentVersion = row["CurrentVersion"].ToString(),
                    Description = row["Description"].ToString(),
                    LastUpdate = row["LastUpdate"].ToString(),
                    Note = row["Note"].ToString(),
                    ReleaseDate = row["ReleaseDate"].ToString(),
                    Icon = row["Icon"].ToString(),
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString()),
                    SoLuongNhanSu = (row["SoLuongNhanSu"] != DBNull.Value) ? Int32.Parse(row["SoLuongNhanSu"].ToString()) : 0,
                    IsShowApp = Convert.ToBoolean(row["IsShowApp"]),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    IsAdmin = Convert.ToBoolean(row["AdminApp"]),
                });
                return result;
            }
        }
    }
}