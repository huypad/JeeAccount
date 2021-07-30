using DPSinfra.UploadFile;
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
        private IAccountManagementReponsitory _accountManagementReponsitory;
        private IdentityServerController identityServerController;
        private readonly string _connectionString;
        private IConfiguration _configuration;
        private readonly string HOST_MINIOSERVER;

        public AccountManagementService(IAccountManagementReponsitory reponsitory, IConfiguration configuration)
        {
            _accountManagementReponsitory = reponsitory;
            identityServerController = new IdentityServerController();
            HOST_MINIOSERVER = configuration.GetValue<string>("MinioConfig:MinioServer");
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _configuration = configuration;
        }

        public async Task<IdentityServerReturn> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
                    var create = _accountManagementReponsitory.CreateAccount(cnn, account, AdminUserID, customerID);
                    if (create.Susscess)
                    {
                        var identity = InitIdentityServerUserModel(cnn, customerID, account);
                        string userId = identity.customData.JeeAccount.UserID.ToString();
                        //GeneralService.saveImgNhanVien(account.ImageAvatar, userId, customerID);
                        string urlAvatar = apiUrl + $"images/nhanvien/{customerID}/{userId}.jpg";
                        identity.customData.PersonalInfo.Avatar = urlAvatar;
                        var createUser = await this.identityServerController.addNewUser(identity, Admin_accessToken);
                        var updateAvatar = _accountManagementReponsitory.UpdateAvatarFirstTime(cnn, urlAvatar, long.Parse(userId), customerID);
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

        public void SaveNewAccountInAccount_AppAndAccountList(long customerID, long AdminUserID, AccountManagementModel account, List<int> ListAppID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                try
                {
                    cnn.BeginTransaction();
                    var create = _accountManagementReponsitory.CreateAccount(cnn, account, AdminUserID, customerID);
                    if (create.Susscess)
                    {
                        var userid = long.Parse(GeneralService.GetUserIDByUsernameCnn(cnn, account.Username).ToString());
                        var saveAppList = _accountManagementReponsitory.InsertAppCodeAccount(cnn, userid, ListAppID);
                        if (!saveAppList.Susscess)
                        {
                            cnn.RollbackTransaction();
                            cnn.EndTransaction();
                            throw new Exception(saveAppList.ErrorMessgage);
                        }
                    }
                    else
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        throw new Exception(create.ErrorMessgage);
                    }
                    cnn.EndTransaction();
                }
                catch (Exception ex)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
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
            long idUser = _accountManagementReponsitory.GetCurrentIdentity(cnn);
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                IdentityServerReturn identity = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
                    var update = _accountManagementReponsitory.UpdatePersonalInfoCustomData(cnn, personalInfoCustom, userId, customerId);
                    if (update.Susscess)
                    {
                        string username = _accountManagementReponsitory.GetUsername(cnn, userId, customerId);
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

                using (DpsConnection cnn = new DpsConnection(_connectionString))
                {
                    username = _accountManagementReponsitory.GetUsername(cnn, objCustomData.userId, customerId);
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

        public async Task UpdateAvatarWithChangeUrlAvatar(long UserId, string Username, long CustomerID, string base64)
        {
            var identity = new IdentityServerController();
            try
            {
                string avatar = UpdateAvatarCdn(Username, base64);

                _accountManagementReponsitory.UpdateAvatar(avatar, UserId, CustomerID);
                var personal = _accountManagementReponsitory.GetPersonalInfoCustomData(UserId, CustomerID);
                var res = await identity.updateCustomDataPersonalInfoInternal(getSecretToken(), personal, Username);
                if (!res.IsSuccessStatusCode)
                {
                    string returnValue = res.Content.ReadAsStringAsync().Result;
                    throw new Exception(returnValue);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> ResetPasswordRootCustomer(CustomerResetPasswordModel model)
        {
            DataTable dt = new DataTable();

            string sql = @$"select min(UserID) as UserID from AccountList where CustomerID = {model.CustomerID}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                var userID = cnn.ExecuteScalar(sql).ToString();
                var username = GeneralService.GetUsernameByUserIDCnn(cnn, userID).ToString();
                return await this.identityServerController.ResetPasswordRootCustomer(getSecretToken(), username, model);
            }
        }

        public async Task<string> GetDirectManagerByUsername(string username)
        {
            DataTable dt = new DataTable();

            string sql = @$"select DirectManager from AccountList where Username = '{username}'";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
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

            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => long.Parse(row["RowID"].ToString()));

                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public string GetUsernameByUserID(string UserID, long customerID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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
                userID = GeneralService.GetUserIDByUsername(_connectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralService.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                userID = model.Userid;
            }
            var appCodes = await _accountManagementReponsitory.GetListAppByUserIDAsync(long.Parse(userID));
            var appCodesName = appCodes.Select(x => x.AppCode).ToList();
            var StaffID = 0;
            var customerID = 0;
            using (DpsConnection cnn = new DpsConnection(_connectionString))
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

        public async Task<HttpResponseMessage> UpdateOneBgColorCustomData(InputApiModel model)
        {
            var username = "";
            var userID = "";
            if (!string.IsNullOrEmpty(model.Username))
            {
                username = model.Username;
                userID = GeneralService.GetUserIDByUsername(_connectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralService.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                userID = model.Userid;
            }

            var customerId = GeneralService.GetCustomerIDByUsername(_connectionString, username);

            var personal = _accountManagementReponsitory.GetPersonalInfoCustomData(Int32.Parse(userID), Int32.Parse(customerId.ToString()));
            personal.BgColor = GeneralService.GetColorNameUser(personal.Name[0].ToString());
            var jwt_internal = getSecretToken();

            var objCustom = new ObjCustomData();
            objCustom.userId = Int32.Parse(userID);
            objCustom.updateField = "personalInfo";
            objCustom.fieldValue = personal;
            return await identityServerController.UpdateCustomDataInternal(jwt_internal, username, objCustom);
        }

        public void UpdateAllAppCodesCustomData(InputApiModel model, List<int> lstAppCode)
        {
            var username = "";
            var userID = "";
            if (!string.IsNullOrEmpty(model.Username))
            {
                username = model.Username;
                userID = GeneralService.GetUserIDByUsername(_connectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralService.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                userID = model.Userid;
            }

            var customerId = GeneralService.GetCustomerIDByUsername(_connectionString, username);
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                _accountManagementReponsitory.InsertAppCodeAccount(cnn, Int32.Parse(userID), lstAppCode);
            }
        }

        public async Task<JeeAccountCustomData> GetJeeAccountCustomDataAsync(InputApiModel model)
        {
            try
            {
                var username = "";
                var userID = "";
                if (!string.IsNullOrEmpty(model.Username))
                {
                    username = model.Username;
                    userID = GeneralService.GetUserIDByUsername(_connectionString, model.Username).ToString();
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    username = GeneralService.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                    userID = model.Userid;
                }
                var customerId = GeneralService.GetCustomerIDByUsername(_connectionString, username);
                var staffid = GeneralService.GetStaffIDByUserID(_connectionString, userID);

                var custom = new JeeAccountCustomData();
                custom.customerID = long.Parse(customerId.ToString());
                if (staffid is not null)
                {
                    custom.staffID = long.Parse(staffid.ToString());
                }
                var lstApp = await _accountManagementReponsitory.GetListAppByUserIDAsync(long.Parse(userID));
                custom.appCode = lstApp.Select(item => item.AppCode).ToList();
                custom.userID = long.Parse(userID);
                return custom;
            }
            catch (Exception ex)
            {
                throw new Exception($"{model.Userid}-{model.Username}-{ex.Message}");
            }
        }

        public string UpdateAvatarCdn(string username, string base64)
        {
            var linkAvatar = "";
            upLoadFileModel up = new upLoadFileModel()
            {
                bs = Convert.FromBase64String(base64),
                FileName = $"{username}.png",
                Linkfile = $"images/avatars"
            };
            var upload = UploadFile.UploadFileImageMinio(up, _configuration);
            if (upload.status)
            {
                linkAvatar = $"https://{HOST_MINIOSERVER}{upload.link}";
            }
            else
            {
                throw new Exception("CDN Error");
            }
            return linkAvatar;
        }
    }
}