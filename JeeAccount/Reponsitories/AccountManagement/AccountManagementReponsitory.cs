using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public class AccountManagementReponsitory : IAccountManagementReponsitory
    {

        private readonly string _connectionString;
        public AccountManagementReponsitory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<AccUsernameModel>> GetListUsernameByCustormerID(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = @"select UserID, Username, email, LastName +' '+FirstName as FullName
                           , FirstName as Name, AvartarImgURL as Avatar, Jobtitle,
                           Department, PhoneNumber, CustomerID
                           from AccountList where CustomerID=@CustomerID";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                return dt.AsEnumerable().Select(row => new AccUsernameModel
                {
                    UserId = Int32.Parse(row["UserID"].ToString()),
                    Username = row["Username"].ToString(),
                    FullName = row["FullName"].ToString(),
                    AvartarImgURL = row["Avatar"].ToString(),
                    CustomerID = Int32.Parse(row["CustomerID"].ToString()),
                    Department = row["Department"].ToString(),
                    PhoneNumber = row["PhoneNumber"].ToString(),
                    Jobtitle = row["Jobtitle"].ToString(),
                    Email = row["email"].ToString(),
                });
            }

        }

        public async Task<IEnumerable<AccUsernameModel>> GetListUsernameByAppcode(long custormerID, string appcode)
        {
            DataTable dt = new DataTable();

            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);
            Conds.Add("AppCode", appcode);

            string sql = $@"select AccountList.UserID, AccountList.Username from AccountList
join Customer_App on Customer_App.CustomerID = AccountList.CustomerID
join AppList on AppList.AppID = Customer_App.AppID
where AppList.AppCode = @AppCode and CustomerID = @custormerID and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                return dt.AsEnumerable().Select(row => new AccUsernameModel
                {
                    UserId = Int32.Parse(row["UserID"].ToString()),
                    Username = row["Username"].ToString()
                });
            }
        }

        public async Task<IEnumerable<AccUsernameModel>> GetListAdminsByCustomerID(long customerID)
        {
            DataTable dt = new DataTable();

            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("IsAdmin", 1);

            string sql = @"select UserID, Username, email from AccountList 
where CustomerID = @CustomerID and (Disable != 1 or Disable is null) and IsAdmin = @IsAdmin";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                return dt.AsEnumerable().Select(row => new AccUsernameModel
                {
                    UserId = Int32.Parse(row["UserID"].ToString()),
                    Username = row["Username"].ToString(),
                    Email = row["email"].ToString(),
                });
            }
        }


        public async Task<long> GetCustormerIDByUsername(string username)
        {
            DataTable dt = new DataTable();

            SqlConditions Conds = new SqlConditions();
            Conds.Add("Username", username);
            string sql = @"select CustomerID from AccountList 
where Username = @Username and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return 0;
                return dt.AsEnumerable().Select(row => long.Parse(row["CustomerID"].ToString())).SingleOrDefault();
            }
        }

        public async Task<InfoUserDTO> GetInfoByUsername(string username)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("Username", username);

            string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name, AvartarImgURL as Avatar, Jobtitle, Department, LastName, Username, Email, PhoneNumber
from AccountList 
where Username = @username and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return new InfoUserDTO();
                return new InfoUserDTO
                {
                    Fullname = dt.Rows[0]["Fullname"].ToString(),
                    Avatar = dt.Rows[0]["Avatar"].ToString(),
                    Jobtitle = dt.Rows[0]["Jobtitle"].ToString(),
                    Name = dt.Rows[0]["Name"].ToString(),
                    Departmemt = dt.Rows[0]["Department"].ToString(),
                    Email = dt.Rows[0]["Email"].ToString(),
                    LastName = dt.Rows[0]["LastName"].ToString(),
                    PhoneNumber = dt.Rows[0]["PhoneNumber"].ToString(),
                    Username = dt.Rows[0]["Username"].ToString(),
                };
            }
        }

        public async Task<InfoUserDTO> GetInfoByCustomerID(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);

            string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name, AvartarImgURL as Avatar, Jobtitle, Department from AccountList 
where CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return new InfoUserDTO();
                return new InfoUserDTO
                {
                    Fullname = dt.Rows[0]["Fullname"].ToString(),
                    Avatar = dt.Rows[0]["Avatar"].ToString(),
                    Jobtitle = dt.Rows[0]["Jobtitle"].ToString(),
                    Name = dt.Rows[0]["Name"].ToString(),
                    Departmemt = dt.Rows[0]["Department"].ToString()
                };
            }
        }

        public async Task<IEnumerable<AppListDTO>> GetListAppByCustomerID(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);

            string sql = @"select AppList.*,  Customer_App.IsDefaultApply from AppList 
join Customer_App on Customer_App.AppID = AppList.AppID
where CustomerID = @CustomerID";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                return dt.AsEnumerable().Select(row => new AppListDTO
                {
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
                    IsDefaultApp = Convert.ToBoolean((bool)row["IsDefaultApply"]),
                    Icon = row["Icon"].ToString(),
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString())
                });
            }
        }

        public async Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerID(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("isAdmin", 1);

            string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name
                        , AvartarImgURL as Avatar, Jobtitle, Department, Username, Email 
                        from AccountList 
                        where CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);

                return dt.AsEnumerable().Select(row => new InfoAdminDTO
                {
                    Avatar = row["Avatar"].ToString(),
                    Fullname = row["Fullname"].ToString(),
                    Jobtitle = row["Jobtitle"].ToString(),
                    Name = row["Name"].ToString(),
                    Username = row["Username"].ToString(),
                    Departmemt = row["Department"].ToString(),
                    Email = row["Email"].ToString(),
                });
            }
        }

        public async Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);

            string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name, 
                        AvartarImgURL as Avatar, Jobtitle, Department, Username, DirectManager
                        , IsActive, Note, email from AccountList 
                        where CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);

                return dt.AsEnumerable().Select(row => new AccountManagementDTO
                {
                    Avatar = row["Avatar"].ToString(),
                    Fullname = row["Fullname"].ToString(),
                    Jobtitle = row["Jobtitle"].ToString(),
                    Name = row["Name"].ToString(),
                    DirectManager = row["DirectManager"].ToString(),
                    IsActive = Convert.ToBoolean((bool)row["IsActive"]),
                    Note = row["Note"].ToString(),
                    Departmemt = row["Department"].ToString(),
                    Username = row["Username"].ToString(),
                    Email = row["Email"].ToString(),
                });
            }
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin)
        {
            Hashtable val = new Hashtable();
            if (!string.IsNullOrEmpty(Note))
            {
                val.Add("Note", Note);
            }
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("Username", Username);
            string sql = $"select IsActive from AccountList where CustomerID=@CustomerID and Username=@Username";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                DataTable dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                {
                    return new ReturnSqlModel("userId không tồn tại", Constant.ERRORCODE_NOTEXIST);
                }
                string sqlGetUsernameLogin = $"select Username from AccountList where CustomerID=@CustomerID and UserID=@UserID";
                SqlConditions CondsLogin = new SqlConditions();
                CondsLogin.Add("UserID", UserIdLogin);
                CondsLogin.Add("CustomerID", customerID);
                DataTable dtGetUsernameLogin = new DataTable();
                dtGetUsernameLogin = cnn.CreateDataTable(sqlGetUsernameLogin, CondsLogin);
                var isTinhTrang = Convert.ToBoolean((bool)dt.Rows[0][0]);
                if (isTinhTrang)
                {
                    val.Add("DeActiveDate", DateTime.Now);
                    val.Add("DeActiveBy", dtGetUsernameLogin.Rows[0][0]);
                }
                else
                {
                    val.Add("ActiveDate", DateTime.Now);
                    val.Add("ActiveBy", dtGetUsernameLogin.Rows[0][0]);
                }
                val.Add("IsActive", !isTinhTrang);

                int x = cnn.Update(val, Conds, "AccountList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            return new ReturnSqlModel();
        }

        public ReturnSqlModel CreateAccount(DpsConnection cnn, AccountManagementModel account, long userID, long CustomerID, bool isAdmin = false)
        {
            Hashtable val = new Hashtable();
            try
            {
                #region val data
                if (account.Fullname is not null)
                {
                    string FirstName = GeneralService.getFirstname(account.Fullname);
                    string Lastname = GeneralService.getlastname(account.Fullname);
                    val.Add("FirstName", FirstName);
                    val.Add("LastName", Lastname);
                }
                if (account.Username is not null) val.Add("Username", account.Username);
                if (account.Jobtitle is not null) val.Add("Jobtitle", account.Jobtitle);
                if (account.Departmemt is not null) val.Add("Department", account.Departmemt);
                if (account.Phonemumber is not null) val.Add("PhoneNumber", account.Phonemumber);
                val.Add("IsActive", 1);
                val.Add("Disable", 0);
                val.Add("Email", account.Email);
                val.Add("ActiveDate", DateTime.Now);
                val.Add("ActiveBy", userID);
                val.Add("CreatedDate", DateTime.Now);
                val.Add("CreatedBy", userID);
                val.Add("CustomerID", CustomerID);
                val.Add("Password", account.Password);
                if (isAdmin) val.Add("IsAdmin", 1);
                #endregion
                int x = cnn.Insert(val, "AccountList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
            return new ReturnSqlModel();
        }

        public ReturnSqlModel UpdateAvatar(DpsConnection cnn, string AvatarUrl, long userID, long CustomerID)
        {
            Hashtable val = new Hashtable();
            val.Add("AvartarImgURL", AvatarUrl);
            SqlConditions cond = new SqlConditions();
            cond.Add("UserID", userID);
            cond.Add("CustomerID", CustomerID);
            try
            {
                int x = cnn.Update(val, cond, "AccountList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
            return new ReturnSqlModel();
        }

        public ReturnSqlModel UpdateAvatarFirstTime(DpsConnection cnn, string AvatarUrl, long userID, long CustomerID)
        {
            Hashtable val = new Hashtable();
            val.Add("AvartarImgURL", AvatarUrl);
            SqlConditions cond = new SqlConditions();
            cond.Add("UserID", userID);
            cond.Add("CustomerID", CustomerID);

            int x = cnn.Update(val, cond, "AccountList");
            if (x <= 0)
            {
                return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
            }

            return new ReturnSqlModel();
        }
        public long GetCurrentIdentity(DpsConnection cnn)
        {
            var id = cnn.ExecuteScalar("SELECT IDENT_CURRENT ('AccountList') AS Current_Identity;");
            return long.Parse(id.ToString());
        }
        public string GetUsername(DpsConnection cnn, long userId, long customerId)
        {
            var username = cnn.ExecuteScalar($"select Username from AccountList where UserID = {userId} and CustomerID = {customerId}");
            return username.ToString();
        }

        public ReturnSqlModel UpdatePersonalInfoCustomData(DpsConnection cnn, PersonalInfoCustomData personalInfoCustom, long userId, long customerId)
        {
            Hashtable val = new Hashtable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerId);
            Conds.Add("UserID", userId);
            string sql = $"select Username from AccountList where CustomerID = @CustomerID and UserID = @UserID";
            DataTable dt = cnn.CreateDataTable(sql, Conds);
            if (dt.Rows.Count == 0)
            {
                return new ReturnSqlModel("Username không tồn tại", Constant.ERRORCODE_NOTEXIST);
            }

            string firstname = "";
            if (personalInfoCustom.Fullname is not null) firstname = GeneralService.getFirstname(personalInfoCustom.Fullname);
            string lastname = "";
            if (personalInfoCustom.Fullname is not null) lastname = GeneralService.getlastname(personalInfoCustom.Fullname);
            if (personalInfoCustom.Avatar is not null) val.Add("AvartarImgURL", personalInfoCustom.Avatar);
            if (personalInfoCustom.Birthday is not null) val.Add("Birthday", Convert.ToDateTime(personalInfoCustom.Birthday));
            if (personalInfoCustom.Departmemt is not null) val.Add("Department", personalInfoCustom.Departmemt);
            if (personalInfoCustom.Jobtitle is not null) val.Add("Jobtitle", personalInfoCustom.Jobtitle);
            if (personalInfoCustom.Phonenumber is not null) val.Add("Phonenumber", personalInfoCustom.Phonenumber);
            if (personalInfoCustom.Fullname is not null) val.Add("FirstName", firstname);
            if (personalInfoCustom.Fullname is not null) val.Add("LastName", lastname);
            int x = cnn.Update(val, Conds, "AccountList");
            if (x <= 0)
            {
                return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
            }
            return new ReturnSqlModel();
        }

        public long GetUserIdByUsername(string Username, long customerID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                string sql = $"select UserID from AccountList where Username = '{Username}' and customerID = {customerID}";
                var UserId = cnn.ExecuteScalar(sql);
                return long.Parse(UserId.ToString());
            }
        }

        public PersonalInfoCustomData GetPersonalInfoCustomData(long UserID, long CustomerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                Conds.Add("CustomerID", CustomerID);
                Conds.Add("UserID", UserID);

                string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name, AvartarImgURL as Avatar, Jobtitle, Department, PhoneNumber, Birthday from AccountList 
where CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return new PersonalInfoCustomData();
                return new PersonalInfoCustomData
                {
                    Avatar = dt.Rows[0]["Avatar"].ToString(),
                    Birthday = dt.Rows[0]["Birthday"].ToString(),
                    Departmemt = dt.Rows[0]["Department"].ToString(),
                    Fullname = dt.Rows[0]["Fullname"].ToString(),
                    Jobtitle = dt.Rows[0]["Jobtitle"].ToString(),
                    Name = dt.Rows[0]["Name"].ToString(),
                    Phonenumber = dt.Rows[0]["PhoneNumber"].ToString(),
                };
            }
        }

        public ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID)
        {
            Hashtable val = new Hashtable();
            val.Add("DirectManager", DirectManager);
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("Username", Username);
            string sql = $"select Username from AccountList where CustomerID=@CustomerID and Username=@Username";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                DataTable dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                {
                    return new ReturnSqlModel("Username không tồn tại", Constant.ERRORCODE_NOTEXIST);
                }

                int x = cnn.Update(val, Conds, "AccountList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            return new ReturnSqlModel();
        }

        public long GetLastUserID(DpsConnection cnn)
        {
            long userid = -1;
            var rowid = cnn.ExecuteScalar("SELECT IDENT_CURRENT ('AccountList') AS Current_Identity;");
            userid = long.Parse(rowid.ToString());
            return userid;
        }

        public long GetCustomerIDByUserID(long UserID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                long CustomerID = -1;
                var item = cnn.ExecuteScalar($"select CustomerID from AccountList where UserID = {UserID} ");
                CustomerID = long.Parse(item.ToString());
                return CustomerID;
            }
        }

        public async Task<IEnumerable<AppListDTO>> GetListAppByUserID(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);

            string sql = @"select AppList.*,  Customer_App.IsDefaultApply from AppList 
join Customer_App on Customer_App.AppID = AppList.AppID
where CustomerID = @CustomerID";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                return dt.AsEnumerable().Select(row => new AppListDTO
                {
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
                    IsDefaultApp = Convert.ToBoolean((bool)row["IsDefaultApply"]),
                    Icon = row["Icon"].ToString(),
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString())
                });
            }
        }
    }
}
