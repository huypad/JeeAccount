using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Services;
using JeeAccount.Services.AccountManagementService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public class AccountManagementReponsitory : IAccountManagementReponsitory
    {
        private readonly string _connectionString;

        public AccountManagementReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
        }

        public async Task<IEnumerable<AccUsernameModel>> GetListUsernameByCustormerID(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = @"select UserID, Username, email, LastName +' '+FirstName as FullName
                           , FirstName as Name, AvartarImgURL as Avatar, Jobtitle,
                           Department, PhoneNumber, CustomerID, cocauid, ChucVuID, Birthday
                           from AccountList where CustomerID=@CustomerID";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => new AccUsernameModel
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
                    StructureID = row["cocauid"].ToString(),
                    ChucVuID = row["ChucVuID"].ToString(),
                    NgaySinh = (row["Birthday"] != DBNull.Value) ? ((DateTime)row["Birthday"]).ToString("dd/MM/yyyy") : "",
                });

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcode(long custormerID, string appcode)
        {
            DataTable dt = new DataTable();
            string sql = $@"select AccountList.UserID, AccountList.Username from AccountList
join Customer_App on Customer_App.CustomerID = AccountList.CustomerID
join AppList on AppList.AppID = Customer_App.AppID
where AppList.AppCode = '{appcode}' and AccountList.CustomerID = {custormerID} and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql);
                var result = dt.AsEnumerable().Select(row => new UserNameDTO
                {
                    UserId = Int32.Parse(row["UserID"].ToString()),
                    Username = row["Username"].ToString()
                });
                return await Task.FromResult(result);
            }
        }

        public async Task<IEnumerable<AdminModel>> GetListAdminsByCustomerID(long customerID)
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
                var result = dt.AsEnumerable().Select(row => new AdminModel
                {
                    UserId = Int32.Parse(row["UserID"].ToString()),
                    Username = row["Username"].ToString(),
                    Email = row["email"].ToString(),
                });
                return await Task.FromResult(result).ConfigureAwait(false);
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
                var result = dt.AsEnumerable().Select(row => long.Parse(row["CustomerID"].ToString())).SingleOrDefault();

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<InfoUserDTO> GetInfoByUsername(string username)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("Username", username);

            string sql = @"
                        select LastName + ' ' + FirstName as FullName, FirstName as Name, AvartarImgURL as Avatar,
                        Jobtitle, Department, LastName, Username, Email, PhoneNumber, cocauid, ChucVuID, Birthday
                        from AccountList
                        where Username = @username and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return new InfoUserDTO();
                var result = new InfoUserDTO
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
                    StructureID = dt.Rows[0]["cocauid"].ToString(),
                    ChucVuID = dt.Rows[0]["ChucVuID"].ToString(),
                    NgaySinh = (dt.Rows[0]["Birthday"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["Birthday"]).ToString("dd/MM/yyyy") : "",
                };

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<InfoCustomerDTO> GetInfoByCustomerID(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("RowID", customerID);

            string sql = @"select *
                        from CustomerList
                        where RowID = @RowID";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return new InfoCustomerDTO();
                var result = new InfoCustomerDTO
                {
                    Address = dt.Rows[0]["Address"].ToString(),
                    Code = dt.Rows[0]["Code"].ToString(),
                    CompanyName = dt.Rows[0]["CompanyName"].ToString(),
                    LogoImgURL = dt.Rows[0]["LogoImgURL"].ToString(),
                    Phone = dt.Rows[0]["Phone"].ToString(),
                    TaxCode = dt.Rows[0]["TaxCode"].ToString()
                };

                return await Task.FromResult(result).ConfigureAwait(false);
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
                var result = dt.AsEnumerable().Select(row => new AppListDTO
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
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString()),
                });

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerID(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("isAdmin", 1);

            string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name
                        , AvartarImgURL as Avatar, Jobtitle, Department, Username, Email, cocauid
                        from AccountList
                        where CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);

                var result = dt.AsEnumerable().Select(row => new InfoAdminDTO
                {
                    Avatar = row["Avatar"].ToString(),
                    Fullname = row["Fullname"].ToString(),
                    Jobtitle = row["Jobtitle"].ToString(),
                    Name = row["Name"].ToString(),
                    Username = row["Username"].ToString(),
                    Departmemt = row["Department"].ToString(),
                    Email = row["Email"].ToString(),
                    StructureID = row["cocauid"].ToString(),
                });
                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);

            string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name,
                        AvartarImgURL as Avatar, Jobtitle, Department, Username, DirectManager
                        , IsActive, Note, email, cocauid from AccountList
                        where CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);

                var result = dt.AsEnumerable().Select(row => new AccountManagementDTO
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
                    StructureID = row["cocauid"].ToString(),
                });

                return await Task.FromResult(result).ConfigureAwait(false);
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

                #endregion val data

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
            if (username != null)
                return username.ToString();
            return null;
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
                if (UserId == null) return 0;
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

                string sql = @"select LastName + ' ' + FirstName as FullName, FirstName as Name
                                , AvartarImgURL as Avatar, Jobtitle, Department, PhoneNumber, Birthday, cocauid
                                from AccountList
                                where CustomerID = @CustomerID
                                and (Disable != 1 or Disable is null)";

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
                    StructureID = dt.Rows[0]["cocauid"].ToString(),
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
                var result = dt.AsEnumerable().Select(row => new AppListDTO
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
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString()),
                });

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public ReturnSqlModel InsertAppCodeAccount(DpsConnection cnn, long UserID, List<int> AppID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);

            string sql = @"select * from Account_App where UserID = @UserID";
            dt = cnn.CreateDataTable(sql, Conds);
            if (dt.Rows.Count > 0)
            {
                return new ReturnSqlModel();
            }

            foreach (var id in AppID)
            {
                Hashtable val = new Hashtable();
                val.Add("UserID", UserID);
                val.Add("AppID", id);
                val.Add("CreatedDate", DateTime.Now);
                val.Add("CreatedBy", 0);
                val.Add("Disable", 0);

                int x = cnn.Insert(val, "Account_App");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }

            return new ReturnSqlModel();
        }

        public List<int> GetAppIdByAppCode(DpsConnection cnn, List<string> AppCode)
        {
            List<int> appid = new List<int>();
            foreach (string code in AppCode)
            {
                var id = cnn.ExecuteScalar($"select AppId from AppList where AppCode = '{code}'");
                appid.Add(Int32.Parse(id.ToString()));
            }
            return appid;
        }

        public List<LoginAccountModel> GetListLogin(DpsConnection cnn)
        {
            DataTable dt = new DataTable();

            string sql = @"select Username, Password from AccountList";

            dt = cnn.CreateDataTable(sql);
            return dt.AsEnumerable().Select(row => new LoginAccountModel
            {
                Username = row["Username"].ToString(),
                Password = row["Password"].ToString()
            }).ToList<LoginAccountModel>();
        }

        public async Task<IEnumerable<AppListDTO>> GetListInfoAppByCustomerID(long customerID)
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
                var result = dt.AsEnumerable().Select(row => new AppListDTO
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
                    Position = string.IsNullOrEmpty(row["Position"].ToString()) ? 0 : Int32.Parse(row["Position"].ToString()),
                });

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccount(long customerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);

            string sql = @"select Customer_App.*, AppList.AppName from AppList
                            join Customer_App on Customer_App.AppID = AppList.AppID
                            where CustomerID = @CustomerID";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => new CustomerAppDTO
                {
                    AppID = Int32.Parse(row["AppID"].ToString()),
                    CustomerID = Int32.Parse(row["CustomerID"].ToString()),
                    EndDate = (row["EndDate"] != DBNull.Value) ? ((DateTime)row["EndDate"]).ToString("dd/MM/yyyy") : "",
                    PackageID = (row["PackageID"] != DBNull.Value) ? Int32.Parse(row["PackageID"].ToString()) : 0,
                    SoLuongNhanSu = (row["SoLuongNhanSu"] != DBNull.Value) ? Int32.Parse(row["SoLuongNhanSu"].ToString()) : 0,
                    StartDate = (row["StartDate"] != DBNull.Value) ? ((DateTime)row["StartDate"]).ToString("dd/MM/yyyy") : "",
                    Status = Int32.Parse(row["Status"].ToString()),
                    AppName = row["AppName"].ToString()
                });
                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<long> GetCustormerIDByUserID(long UserID)
        {
            DataTable dt = new DataTable();

            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", UserID);
            string sql = @"select CustomerID from AccountList where UserID = @UserID";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds);
                if (dt.Rows.Count == 0)
                    return 0;
                var result = dt.AsEnumerable().Select(row => long.Parse(row["CustomerID"].ToString())).SingleOrDefault();

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }
    }
}