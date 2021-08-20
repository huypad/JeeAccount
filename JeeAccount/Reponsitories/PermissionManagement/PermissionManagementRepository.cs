using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.PermissionManagement
{
    public class PermissionManagementRepository : IPermissionManagementRepository
    {
        private readonly string _connectionString;

        private const string SQL_DSADMIN_APP_DEFAULT = @"select AccountList.UserID, Username, Email, LastName +' ' + FirstName as FullName
                           , FirstName as Name, LastName, AvartarImgURL as Avatar, JobtitleList.JobtitleName, JobtitleID , DepartmentID,
                           DepartmentList.DepartmentName, PhoneNumber, AccountList.CustomerID, cocauid, ChucVuID, Birthday, DirectManager,
                           AccountList.IsActive, AccountList.IsAdmin as AdminHeThong, AccountList.Note, Account_App.IsAdmin as AdminApp
                           from AccountList
left join DepartmentList on DepartmentList.RowID = AccountList.DepartmentID
left join JobtitleList on JobtitleList.RowID = AccountList.JobtitleID
join Account_App on Account_App.UserID = AccountList.UserID";

        private const string SQL_DSADMIN_APP_JEEHR = @"select AccountList.UserID, Username, email, LastName +' '+FirstName as FullName,
                           FirstName as Name, LastName, AvartarImgURL as Avatar, Jobtitle, JobtitleID, AccountList.IsActive, AccountList.IsAdmin as AdminHeThong, Note,
                           Department, DepartmentID, PhoneNumber, CustomerID, cocauid, ChucVuID, Birthday, DirectManager, Account_App.IsAdmin as AdminApp
                           from AccountList
join Account_App on Account_App.UserID = AccountList.UserID";

        private readonly IAccountManagementReponsitory _accountReponsitory;

        public PermissionManagementRepository(IConfiguration configuration, IAccountManagementReponsitory accountManagementReponsitory)
        {
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _accountReponsitory = accountManagementReponsitory;
        }

        public async Task<IEnumerable<AccountManagementDTO>> GetListAccountAdminAppNotAdminHeThongDefaultAsync(long customerID, int AppID, string where = "", string orderBy = "")
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("AppID", AppID);

            string where_order = "";
            if (!string.IsNullOrEmpty(where))
            {
                where += " and AccountList.CustomerID = @CustomerID and AppID=@AppID and (AccountList.Disable != 1 or AccountList.Disable is null) and Account_App.IsAdmin = 1 and AccountList.IsAdmin = 0";
            }
            else
            {
                where += " AccountList.CustomerID = @CustomerID and AppID=@AppID and (AccountList.Disable != 1 or AccountList.Disable is null) and Account_App.IsAdmin = 1 and AccountList.IsAdmin = 0";
            }
            where_order += $"where {where} ";

            if (!string.IsNullOrEmpty(orderBy)) where_order += $"order by {orderBy}";

            string sql = @$"{SQL_DSADMIN_APP_DEFAULT} { where_order}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);

                var result = dt.AsEnumerable().Select(row => new AccountManagementDTO
                {
                    UserId = GeneralService.ConvertToLong(row["UserID"]),
                    Username = row["Username"].ToString(),
                    FullName = row["FullName"].ToString(),
                    AvartarImgURL = row["Avatar"].ToString(),
                    CustomerID = GeneralService.ConvertToLong(row["CustomerID"]),
                    PhoneNumber = row["PhoneNumber"].ToString(),
                    Email = row["Email"].ToString(),
                    Jobtitle = row["JobtitleName"].ToString(),
                    JobtitleID = GeneralService.ConvertToInt(row["JobtitleID"]),
                    NgaySinh = GeneralService.ConvertDateToString(row["Birthday"]),
                    BgColor = GeneralService.GetColorNameUser(row["Name"].ToString().Substring(0, 1)),
                    FirstName = row["Name"].ToString(),
                    LastName = row["LastName"].ToString(),
                    Department = row["DepartmentName"].ToString(),
                    DepartmentID = GeneralService.ConvertToInt(row["DepartmentID"]),
                    ChucVuID = row["ChucVuID"].ToString(),
                    StructureID = row["cocauid"].ToString(),
                    DirectManager = row["DirectManager"] != DBNull.Value ? dt.AsEnumerable().Where(x => x["Username"].ToString() == row["DirectManager"].ToString())
                                                                .Select(x => x["FullName"].ToString()).SingleOrDefault() : "",
                    DirectManagerUserID = row["DirectManager"] != DBNull.Value ? dt.AsEnumerable().Where(x => x["Username"].ToString() == row["DirectManager"].ToString())
                                                                .Select(x => Convert.ToInt64(x["UserID"])).SingleOrDefault() : 0,
                    DirectManagerUsername = row["DirectManager"].ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    Note = row["Note"].ToString()
                });

                return result;
            }
        }

        public async Task<IEnumerable<AccountManagementDTO>> GetListAccountAdminAppNotAdminHeThongJeeHRAsync(long customerID, int AppID, string where = "", string orderBy = "")
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("AppID", AppID);

            string where_order = "";
            if (!string.IsNullOrEmpty(where))
            {
                where += " and AccountList.CustomerID = @CustomerID and AppID=@AppID and (AccountList.Disable != 1 or AccountList.Disable is null) and Account_App.IsAdmin = 1 and AccountList.IsAdmin = 0";
            }
            else
            {
                where += " AccountList.CustomerID = @CustomerID and AppID=@AppID and (AccountList.Disable != 1 or AccountList.Disable is null) and Account_App.IsAdmin = 1 and AccountList.IsAdmin = 0";
            }
            where_order += $"where {where} ";

            if (!string.IsNullOrEmpty(orderBy)) where_order += $"order by {orderBy}";

            string sql = @$"{SQL_DSADMIN_APP_JEEHR} { where_order}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);

                var result = dt.AsEnumerable().Select(row => new AccountManagementDTO
                {
                    UserId = GeneralService.ConvertToLong(row["UserID"]),
                    Username = row["Username"].ToString(),
                    FullName = row["FullName"].ToString(),
                    AvartarImgURL = row["Avatar"].ToString(),
                    CustomerID = GeneralService.ConvertToLong(row["CustomerID"]),
                    PhoneNumber = row["PhoneNumber"].ToString(),
                    Email = row["Email"].ToString(),
                    Jobtitle = row["Jobtitle"].ToString(),
                    JobtitleID = GeneralService.ConvertToInt(row["JobtitleID"]),
                    NgaySinh = GeneralService.ConvertDateToString(row["Birthday"]),
                    BgColor = GeneralService.GetColorNameUser(row["Name"].ToString().Substring(0, 1)),
                    FirstName = row["Name"].ToString(),
                    LastName = row["LastName"].ToString(),
                    Department = row["Department"].ToString(),
                    DepartmentID = GeneralService.ConvertToInt(row["DepartmentID"]),
                    ChucVuID = row["ChucVuID"].ToString(),
                    StructureID = row["cocauid"].ToString(),
                    DirectManager = row["DirectManager"] != DBNull.Value ? dt.AsEnumerable().Where(x => x["Username"].ToString() == row["DirectManager"].ToString())
                                                                .Select(x => x["FullName"].ToString()).SingleOrDefault() : "",
                    DirectManagerUserID = row["DirectManager"] != DBNull.Value ? dt.AsEnumerable().Where(x => x["Username"].ToString() == row["DirectManager"].ToString())
                                                                .Select(x => Convert.ToInt64(x["UserID"])).SingleOrDefault() : 0,
                    DirectManagerUsername = row["DirectManager"].ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    Note = row["Note"].ToString()
                });

                return result;
            }
        }

        public async Task CreateAdminHeThong(long userid, long customerid, long updateBy)
        {
            var lstApp = await _accountReponsitory.GetListAppByCustomerIDAsync(customerid);
            var lstAppId = lstApp.Select(item => item.AppID).ToList();
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                string sql = $"select * from AccountList where UserID = {userid} and CustomerID = {customerid}";
                var dt = await cnn.CreateDataTableAsync(sql);
                var common = GeneralReponsitory.GetCommonInfoCnn(cnn, updateBy);
                if (dt.Rows.Count == 0) throw new KhongCoDuLieuException();
                _accountReponsitory.InsertAppCodeAccount(cnn, userid, lstAppId, common.Username, true);
                UpdateIsAdminHeThong(cnn, userid, customerid, updateBy);
            }
        }

        public async Task RemoveAdminHeThong(long userid, long customerid, long updateBy)
        {
            var lstApp = await _accountReponsitory.GetListAppByCustomerIDAsync(customerid);
            var lstAppId = lstApp.Select(item => item.AppID).ToList();
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                string sql = $"select * from AccountList where UserID = {userid} and CustomerID = {customerid}";
                var dt = await cnn.CreateDataTableAsync(sql);
                var common = GeneralReponsitory.GetCommonInfoCnn(cnn, updateBy);
                if (dt.Rows.Count == 0) throw new KhongCoDuLieuException();
                _accountReponsitory.InsertAppCodeAccount(cnn, userid, lstAppId, common.Username, true);
                RemoveIsAdminHeThong(cnn, userid, customerid, updateBy);
            }
        }

        public async Task CreateAdminApp(long userid, long customerid, long UpdateBy, List<int> lstAppID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                string sql = $"select * from AccountList where UserID = {userid} and CustomerID = {customerid}";
                var dt = await cnn.CreateDataTableAsync(sql);
                var common = GeneralReponsitory.GetCommonInfoCnn(cnn, UpdateBy);
                _accountReponsitory.InsertAppCodeAccount(cnn, userid, lstAppID, common.Username, true);
                if (dt.Rows.Count == 0) throw new KhongCoDuLieuException();
                foreach (var id in lstAppID)
                {
                    UpdateIsAdminAccountApp(cnn, userid, UpdateBy, id);
                }
            }
        }

        public async Task RemoveAdminApp(long userid, long customerid, long UpdateBy, List<int> lstAppID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                string sql = $"select * from AccountList where UserID = {userid} and CustomerID = {customerid}";
                var dt = await cnn.CreateDataTableAsync(sql);
                var common = GeneralReponsitory.GetCommonInfoCnn(cnn, UpdateBy);
                if (dt.Rows.Count == 0) throw new KhongCoDuLieuException();
                foreach (var id in lstAppID)
                {
                    RemoveIsAdminAccountApp(cnn, userid, UpdateBy, id);
                }
            }
        }

        private void UpdateIsAdminHeThong(DpsConnection cnn, long userid, long customerid, long updateBy)
        {
            Hashtable val = new Hashtable();
            val.Add("IsAdmin", 1);
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerid);
            Conds.Add("UserID", userid);
            int x = cnn.Update(val, Conds, "AccountList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }

            Hashtable val2 = new Hashtable();
            val2.Add("IsAdmin", 1);
            val2.Add("LastUpdate", DateTime.Now);
            val2.Add("UpdatedBy", updateBy);

            SqlConditions Conds2 = new SqlConditions();
            Conds2.Add("UserID", userid);
            int y = cnn.Update(val2, Conds2, "Account_App");
            if (y <= 0)
            {
                throw cnn.LastError;
            }
        }

        private void RemoveIsAdminHeThong(DpsConnection cnn, long userid, long customerid, long updateBy)
        {
            Hashtable val = new Hashtable();
            val.Add("IsAdmin", 0);
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerid);
            Conds.Add("UserID", userid);
            int x = cnn.Update(val, Conds, "AccountList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }

            Hashtable val2 = new Hashtable();
            val2.Add("IsAdmin", 0);
            val2.Add("LastUpdate", DateTime.Now);
            val2.Add("UpdatedBy", updateBy);

            SqlConditions Conds2 = new SqlConditions();
            Conds2.Add("UserID", userid);
            int y = cnn.Update(val2, Conds2, "Account_App");
            if (y <= 0)
            {
                throw cnn.LastError;
            }
        }

        private void UpdateIsAdminAccountApp(DpsConnection cnn, long userid, long updateBy, int AppID)
        {
            Hashtable val = new Hashtable();
            val.Add("IsAdmin", 1);
            val.Add("LastUpdate", DateTime.Now);
            val.Add("UpdatedBy", updateBy);
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", userid);
            Conds.Add("AppID", AppID);
            int x = cnn.Update(val, Conds, "Account_App");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }

        private void RemoveIsAdminAccountApp(DpsConnection cnn, long userid, long updateBy, int AppID)
        {
            Hashtable val = new Hashtable();
            val.Add("IsAdmin", 0);
            val.Add("LastUpdate", DateTime.Now);
            val.Add("UpdatedBy", updateBy);
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", userid);
            Conds.Add("AppID", AppID);
            int x = cnn.Update(val, Conds, "Account_App");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }
    }
}