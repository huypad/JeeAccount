using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JeeAccount.Services.AccountManagementService
{
    public class AccountManagementService : IAccountManagementService
    {
        private IAccountManagementReponsitory accountManagementReponsitory;
        private IdentityServerController identityServerController;
        private string ConnectionString;
        private IConfiguration _configuration;

        public AccountManagementService(IAccountManagementReponsitory accountManagementReponsitory, IConfiguration configuration)
        {
            this.accountManagementReponsitory = accountManagementReponsitory;
            this.identityServerController = new IdentityServerController();
            ConnectionString = configuration.GetValue<string>("AppConfig:Connection");
            _configuration = configuration;
        }

        public Task<IEnumerable<AccUsernameModel>> GetListUsernameByCustormerID(long customerID)
        {
            var accUser = accountManagementReponsitory.GetListUsernameByCustormerID(customerID);
            return accUser;
        }

        public Task<long> GetCustormerIDByUsername(string username)
        {
            var customerID = accountManagementReponsitory.GetCustormerIDByUsername(username);
            return customerID;
        }

        public Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerID(long customerID)
        {
            var listAccount = accountManagementReponsitory.GetInfoAdminAccountByCustomerID(customerID);
            return listAccount;
        }

        public Task<InfoCustomerDTO> GetInfoByCustomerID(long customerID)
        {
            var user = accountManagementReponsitory.GetInfoByCustomerID(customerID);
            return user;
        }

        public Task<InfoUserDTO> GetInfoByUsername(string username)
        {
            var user = accountManagementReponsitory.GetInfoByUsername(username);
            return user;
        }

        public Task<IEnumerable<AdminModel>> GetListAdminsByCustomerID(long customerID)
        {
            var admins = accountManagementReponsitory.GetListAdminsByCustomerID(customerID);
            return admins;
        }

        public Task<IEnumerable<AppListDTO>> GetListAppByCustomerID(long customerID)
        {
            var appList = accountManagementReponsitory.GetListAppByCustomerID(customerID);
            return appList;
        }

        public Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcode(long customerID, string appcode)
        {
            var accUser = accountManagementReponsitory.GetListUsernameByAppcode(customerID, appcode);
            return accUser;
        }

        public Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID)
        {
            var accManagement = accountManagementReponsitory.GetListAccountManagement(customerID);
            return accManagement;
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin)
        {
            var update = accountManagementReponsitory.ChangeTinhTrang(customerID, Username, Note, UserIdLogin);
            return update;
        }

        public ReturnSqlModel UpdateAvatar(string AvatarUrl, long userID, long CustomerID)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                var update = accountManagementReponsitory.UpdateAvatar(cnn, AvatarUrl, userID, CustomerID);
                return update;
            }
        }

        public async Task<IdentityServerReturn> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
                    var create = accountManagementReponsitory.CreateAccount(cnn, account, AdminUserID, customerID);
                    if (create.Susscess)
                    {
                        var identity = InitIdentityServerUserModel(cnn, customerID, account);
                        string userId = identity.customData.JeeAccount.UserID.ToString();
                        GeneralService.saveImgNhanVien(account.ImageAvatar, userId, customerID);
                        string urlAvatar = apiUrl + $"images/nhanvien/{customerID}/{userId}.jpg";
                        identity.customData.PersonalInfo.Avatar = urlAvatar;
                        var createUser = await this.identityServerController.addNewUser(identity, Admin_accessToken);
                        var updateAvatar = accountManagementReponsitory.UpdateAvatarFirstTime(cnn, urlAvatar, long.Parse(userId), customerID);
                        if (createUser.statusCode != 0)
                        {
                            cnn.RollbackTransaction();
                            cnn.EndTransaction();
                            return createUser;
                        }

                        cnn.EndTransaction();
                        return createUser;
                    }
                    else
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(create);
                        return identityServerReturn;
                    }
                }
                catch (Exception ex)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    identityServerReturn = GeneralService.TranformIdentityServerException(ex);
                    return identityServerReturn;
                }
            }
        }

        private IdentityServerAddNewUser InitIdentityServerUserModel(DpsConnection cnn, long customerID, AccountManagementModel account)
        {
            IdentityServerAddNewUser identity = new IdentityServerAddNewUser();
            identity.username = account.Username;
            identity.password = account.Password;

            CustomData customData = new CustomData();
            PersonalInfoCustomData personalInfo = new PersonalInfoCustomData();
            personalInfo.Name = GeneralService.getFirstname(account.Fullname);
            personalInfo.Avatar = account.ImageAvatar;
            personalInfo.Departmemt = account.Departmemt;
            personalInfo.Fullname = account.Fullname;
            personalInfo.Jobtitle = account.Jobtitle;
            personalInfo.Birthday = "";
            personalInfo.Phonenumber = account.Phonemumber;
            long idUser = accountManagementReponsitory.GetCurrentIdentity(cnn);
            JeeAccountModel jee = new JeeAccountModel();
            jee.AppCode = account.AppCode;
            jee.CustomerID = customerID;
            jee.UserID = idUser;
            customData.JeeAccount = jee;
            customData.PersonalInfo = personalInfo;

            identity.customData = customData;
            return identity;
        }

        public async Task<IdentityServerReturn> UpdatePersonalInfoCustomData(string Admin_access_token, PersonalInfoCustomData personalInfoCustom, long userId, long customerId)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                IdentityServerReturn identity = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
                    var update = accountManagementReponsitory.UpdatePersonalInfoCustomData(cnn, personalInfoCustom, userId, customerId);
                    if (update.Susscess)
                    {
                        string username = accountManagementReponsitory.GetUsername(cnn, userId, customerId);
                        var updateCustom = await this.identityServerController.updateCustomDataPersonalInfo(Admin_access_token, personalInfoCustom, username);
                        if (updateCustom.data is null)
                        {
                            cnn.RollbackTransaction();
                            cnn.EndTransaction();
                            return updateCustom;
                        }
                        cnn.EndTransaction();
                        return updateCustom;
                    }
                    else
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identity = GeneralService.TranformIdentityServerReturnSqlModel(update);
                        return identity;
                    }
                }
                catch (Exception ex)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    identity.message = ex.Message;
                    identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
                    return identity;
                }
            }
        }

        public async Task<IdentityServerReturn> UppdateCustomData(string Admin_access_token, ObjCustomData objCustomData, long customerId)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            try
            {
                string username = "";

                using (DpsConnection cnn = new DpsConnection(ConnectionString))
                {
                    username = accountManagementReponsitory.GetUsername(cnn, objCustomData.userId, customerId);
                    if (username == null)
                    {
                        identity.message = "UserID không tồn tại";
                        identity.statusCode = Int32.Parse(Constant.ERRORDATA);
                        return identity;
                    }
                }

                var updateCustom = await this.identityServerController.UppdateCustomData(Admin_access_token, username, objCustomData);
                if (updateCustom.data is null)
                {
                    return updateCustom;
                }
                return updateCustom;
            }
            catch (Exception ex)
            {
                identity.message = ex.Message;
                identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
                return identity;
            }
        }

        public async Task<object> login(string username, string password)
        {
            return await this.identityServerController.loginUser(username, password);
        }

        public long GetUserIdByUsername(string Username, long CustomerId)
        {
            return accountManagementReponsitory.GetUserIdByUsername(Username, CustomerId);
        }

        public PersonalInfoCustomData GetPersonalInfoCustomData(long UserID, long CustomerID)
        {
            return accountManagementReponsitory.GetPersonalInfoCustomData(UserID, CustomerID);
        }

        public async Task<IdentityServerReturn> UpdateAvatarWithChangeUrlAvatar(string Admin_access_token, long UserId, string Username, long CustomerID, string apiUrl)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            try
            {
                var person = accountManagementReponsitory.GetPersonalInfoCustomData(UserId, CustomerID);
                if (person.Fullname is null)
                {
                    identity.message = "UserId không tồn tại";
                    identity.statusCode = Int32.Parse(Constant.ERRORCODE_NOTEXIST);
                    return identity;
                }
                string urlAvatar = apiUrl + $"images/nhanvien/{CustomerID}/{UserId}.jpg";
                using (DpsConnection cnn = new DpsConnection(ConnectionString))
                {
                    cnn.BeginTransaction();
                    var updateAvatarToDB = accountManagementReponsitory.UpdateAvatar(cnn, urlAvatar, UserId, CustomerID);
                    if (!updateAvatarToDB.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return GeneralService.TranformIdentityServerReturnSqlModel(updateAvatarToDB);
                    }
                    person.Avatar = urlAvatar;
                    var updateCustom = await this.identityServerController.updateCustomDataPersonalInfo(Admin_access_token, person, Username);
                    if (updateCustom.data is null)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return updateCustom;
                    }
                    cnn.EndTransaction();
                    return updateCustom;
                }
            }
            catch (Exception ex)
            {
                identity.message = ex.Message;
                identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
                return identity;
            }
        }

        public ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID)
        {
            var update = accountManagementReponsitory.UpdateDirectManager(Username, DirectManager, customerID);
            return update;
        }

        public bool checkUserIDInCustomerID(long UserID, long CustomerID)
        {
            long customerID = accountManagementReponsitory.GetCustomerIDByUserID(UserID);
            return (CustomerID == customerID);
        }

        public Task<IEnumerable<AppListDTO>> GetListAppByUserID(long UserID)
        {
            var listapp = accountManagementReponsitory.GetListAppByUserID(UserID);
            return listapp;
        }

        public List<int> GetAppIdByAppCode(DpsConnection cnn, List<string> AppCode)
        {
            return accountManagementReponsitory.GetAppIdByAppCode(cnn, AppCode);
        }

        public List<LoginAccountModel> GetListLogin()
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                return accountManagementReponsitory.GetListLogin(cnn);
            }
        }

        public async Task<ReturnSqlModel> UpdateTool()
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                cnn.BeginTransaction();
                var listlogin = accountManagementReponsitory.GetListLogin(cnn);
                string listAccountempy = "";
                foreach (var login in listlogin)
                {
                    var appAccount = await identityServerController.loginUser(login.Username, login.Password);
                    if (appAccount.AppCode is null)
                    {
                        if (string.IsNullOrEmpty(listAccountempy))
                        {
                            listAccountempy = login.Username + "/" + login.Password;
                        }
                        else
                        {
                            listAccountempy += ", " + login.Username + "/" + login.Password;
                        }
                        continue;
                    }
                    var listId = GetAppIdByAppCode(cnn, appAccount.AppCode);
                    var insert = accountManagementReponsitory.InsertAppCodeAccount(cnn, appAccount.UserId, listId);
                    if (!insert.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return insert;
                    }
                }

                cnn.EndTransaction();
                if (string.IsNullOrEmpty(listAccountempy))
                {
                    return new ReturnSqlModel();
                }
                else
                {
                    return new ReturnSqlModel(listAccountempy, "0");
                }
            }
        }

        public async Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccount(long CustomerID)
        {
            return await accountManagementReponsitory.GetListCustomerAppByCustomerIDFromAccount(CustomerID);
        }

        public async Task<HttpResponseMessage> ResetPasswordRootCustomer(CustomerResetPasswordModel model)
        {
            DataTable dt = new DataTable();

            string sql = @$"select min(UserID) as UserID from AccountList where CustomerID = {model.CustomerID}";

            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                var userID = cnn.ExecuteScalar(sql).ToString();
                var username = GeneralService.GetUsernameByUserIDCnn(cnn, userID).ToString();
                return await this.identityServerController.ResetPasswordRootCustomer(getSecretToken(), username, model);
            }
        }

        public async Task<long> GetCustormerIDByUserID(long UserID)
        {
            var customerID = await accountManagementReponsitory.GetCustormerIDByUserID(UserID);
            return customerID;
        }

        public async Task<InfoUserDTO> GetInfoByUserID(string userid)
        {
            var username = GeneralService.GetUsernameByUserID(ConnectionString, userid);
            if (username == null) return null;
            var data = await accountManagementReponsitory.GetInfoByUsername(username.ToString());
            return data;
        }

        public async Task<string> GetDirectManagerByUsername(string username)
        {
            DataTable dt = new DataTable();

            string sql = @$"select DirectManager from AccountList where Username = '{username}'";

            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql);
                if (dt.Rows.Count == 0)
                    return null;
                var result = dt.AsEnumerable().Select(row => row["DirectManager"].ToString()).SingleOrDefault();

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<string> GetDirectManagerByUserID(string userid)
        {
            DataTable dt = new DataTable();

            SqlConditions Conds = new SqlConditions();
            Conds.Add("userid", userid);
            string sql = @"select DirectManager from AccountList where userid = @userid";

            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds);
                if (dt.Rows.Count == 0)
                    return null;
                var result = dt.AsEnumerable().Select(row => row["DirectManager"].ToString()).SingleOrDefault();

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<UserNameDTO>> GetListJustUsernameAndUserIDByCustormerID(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = @"select UserID, Username from AccountList where CustomerID=@CustomerID";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => new UserNameDTO
                {
                    UserId = Int32.Parse(row["UserID"].ToString()),
                    Username = row["Username"].ToString()
                });

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<string>> GetListJustUsernameByCustormerID(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = @"select Username from AccountList where CustomerID=@CustomerID";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => row["Username"].ToString());

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<long>> GetListJustUserIDByCustormerID(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = @"select UserID from AccountList where CustomerID=@CustomerID";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => long.Parse(row["UserID"].ToString()));

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<long>> GetListJustCustormerID()
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            string sql = @"select RowID from CustomerList";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => long.Parse(row["RowID"].ToString()));

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<long>> GetListJustCustormerIDAppCode(string AppCode)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            string sql = $@"select  CustomerList.RowID from CustomerList
join Customer_App on CustomerList.RowID = Customer_App.CustomerID
join AppList on AppList.AppID = Customer_App.AppID
where AppList.AppCode = '{AppCode}'";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => long.Parse(row["RowID"].ToString()));

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public string GetUsernameByUserID(string UserID, long customerID)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                string sql = $"select Username from AccountList where UserID = {UserID} and customerID = {customerID}";
                var Username = cnn.ExecuteScalar(sql);
                if (Username == null) return null;
                return Username.ToString();
            }
        }

        public async Task<IEnumerable<string>> GetListDirectManager(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);
            string sql = @"select DISTINCT DirectManager from AccountList where DirectManager is not null and CustomerID=@CustomerID";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => row["DirectManager"].ToString());
                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManager(string DirectManager)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("DirectManager", DirectManager);

            string sql = @"select UserID, Username, email, LastName +' '+FirstName as FullName
                           , FirstName as Name, AvartarImgURL as Avatar, Jobtitle,
                           Department, PhoneNumber, CustomerID, cocauid, ChucVuID, Birthday
                           from AccountList where DirectManager=@DirectManager";
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
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

        public string getSecretToken()
        {
            var secret = _configuration.GetValue<string>("Jwt:internal_secret");
            var projectName = _configuration.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }

        public async Task<HttpResponseMessage> UpdateOneStaffIDByInputApiModel(InputApiModel model)
        {
            var username = "";
            var userID = "";
            if (!string.IsNullOrEmpty(model.Username))
            {
                username = model.Username;
                userID = GeneralService.GetUserIDByUsername(ConnectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralService.GetUsernameByUserID(ConnectionString, model.Userid).ToString();
                userID = model.Userid;
            }

            var appCodes = await accountManagementReponsitory.GetListAppByUserID(long.Parse(userID));
            var appCodesName = appCodes.Select(x => x.AppCode).ToList();
            var StaffID = 0;
            var customerID = 0;
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                if (string.IsNullOrEmpty(model.StaffID))
                {
                    var staffidScalar = cnn.ExecuteScalar($"select StaffID from AccountList where UserID = {userID} or Username = '{username}'");
                    if (staffidScalar is not null)
                    {
                        if (!staffidScalar.Equals(DBNull.Value))
                        {
                            StaffID = Int32.Parse(staffidScalar.ToString());
                        }
                    }
                }
                else
                {
                    StaffID = Int32.Parse(model.StaffID);
                    SaveStaffID(cnn, StaffID.ToString(), userID);
                }

                customerID = Int32.Parse(cnn.ExecuteScalar($"select CustomerID from AccountList where UserID = {userID} or Username = '{username}'").ToString());
            }

            var jwt_internal = getSecretToken();

            var objCustom = new ObjCustomData();
            objCustom.userId = Int32.Parse(userID);
            objCustom.updateField = "jee-account";
            objCustom.fieldValue = new JeeAccountModel
            {
                AppCode = appCodesName,
                CustomerID = customerID,
                StaffID = StaffID,
                UserID = Int32.Parse(userID)
            };

            var reponse = await identityServerController.UpdateCustomDataInternal(jwt_internal, username, objCustom);
            return reponse;
        }

        private void SaveStaffID(DpsConnection cnn, string StaffID, string UserID)
        {
            Hashtable val = new Hashtable();
            if (!string.IsNullOrEmpty(StaffID))
            {
                val.Add("StaffID", long.Parse(StaffID));
            }
            SqlConditions Conds = new SqlConditions();
            Conds.Add("UserID", long.Parse(UserID));
            int x = cnn.Update(val, Conds, "AccountList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }
    }
}