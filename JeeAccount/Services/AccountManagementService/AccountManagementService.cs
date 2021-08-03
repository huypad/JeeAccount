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
        private IAccountManagementReponsitory _reponsitory;
        private IdentityServerController identityServerController;
        private readonly string _connectionString;
        private IConfiguration _configuration;
        private readonly string HOST_MINIOSERVER;

        public AccountManagementService(IAccountManagementReponsitory reponsitory, IConfiguration configuration)
        {
            _reponsitory = reponsitory;
            identityServerController = new IdentityServerController();
            HOST_MINIOSERVER = configuration.GetValue<string>("MinioConfig:MinioServer");
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _configuration = configuration;
        }

        #region api giao diện

        public async Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(QueryParams query, long customerid)
        {
            query = query == null ? new QueryParams() : query;

            string orderByStr = "AccountList.UserID asc";
            string whereStr = " AccountList.Disable != 1 ";

            Dictionary<string, string> sortableFields = new Dictionary<string, string>
                        {
                            { "nhanvien", "AccountList.LastName"},
                            { "tendangnhap", "AccountList.Username"},
                            { "tinhtrang", "AccountList.IsActive"},
                            { "chucvu", "AccountList.JobtitleID"},
                            { "phongban", "AccountList.DepartmentID" }
                        };

            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);

            if (!string.IsNullOrEmpty(query.sortField) && sortableFields.ContainsKey(query.sortField))
            {
                orderByStr = sortableFields[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
            }

            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                if (!checkusedjeehr)
                {
                    whereStr += $" and (AccountList.LastName + ' ' + AccountList.FirstName like N'%{query.filter["keyword"]}%' " +
$"or JobtitleList.JobtitleName like N'%{query.filter["keyword"]}%' " +
$"or AccountList.Username like N'%{query.filter["keyword"]}%'" +
$"or DepartmentList.DepartmentName like N'%{query.filter["keyword"]}%')";
                }
                else
                {
                    whereStr += $" and (AccountList.LastName + ' ' + AccountList.FirstName like N'%{query.filter["keyword"]}%' " +
$"or AccountList.Jobtitle like N'%{query.filter["keyword"]}%' " +
$"or AccountList.Username like N'%{query.filter["keyword"]}%'" +
$"or AccountList.Department like N'%{query.filter["keyword"]}%')";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["username"]))
            {
                whereStr += $" and (AccountList.Username like '%{query.filter["username"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["tennhanvien"]))
            {
                whereStr += $" and (AccountList.FirstName like N'%{query.filter["tennhanvien"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                if (!checkusedjeehr)
                {
                    whereStr += $" and (DepartmentList.DepartmentName like N'%{query.filter["phongban"]}%') ";
                }
                else
                {
                    whereStr += $" and (AccountList.Department like N'%{query.filter["phongban"]}%') ";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                whereStr += $" and (AccountList.DepartmentID  = {query.filter["phongbanid"]}) ";
            }

            if (!string.IsNullOrEmpty(query.filter["chucvu"]))
            {
                if (!checkusedjeehr)
                {
                    whereStr += $" and (JobtitleList.Jobtitle like N'%{query.filter["chucvu"]}%') ";
                }
                else
                {
                    whereStr += $" and (AccountList.Jobtitle like N'%{query.filter["chucvu"]}%') ";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["chucvuid"]))
            {
                whereStr += $" and (AccountList.JobtitleID like N'%{query.filter["chucvuid"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["isadmin"]))
            {
                if (Convert.ToBoolean(query.filter["isadmin"]))
                {
                    whereStr += $" and (AccountList.IsAdmin = 1) ";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["dakhoa"]))
            {
                if (Convert.ToBoolean(query.filter["dakhoa"]))
                {
                    whereStr += $" and (AccountList.IsActive = 0) ";
                }
            }

            var lst = Enumerable.Empty<AccountManagementDTO>();
            if (!checkusedjeehr)
            {
                var res = await _reponsitory.GetListAccountManagementDefaultAsync(customerid, whereStr, orderByStr);
                lst = res;
            }
            else
            {
                var res = await _reponsitory.GetListAccountManagementIsJeeHRAsync(customerid, whereStr, orderByStr);
                lst = res;
            }
            return lst;
        }

        #endregion api giao diện

        #region api new

        public async Task<InfoUserDTO> GetInfoByUsernameAsync(string username, long customerid)
        {
            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);
            if (checkusedjeehr)
            {
                var infoUser = await _reponsitory.GetInfoByUsernameIsJeeHRAsync(username);
                return infoUser;
            }
            else
            {
                var infoUser = await _reponsitory.GetInfoByUsernameDefaultAsync(username);
                return infoUser;
            }
        }

        public async Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManager(string username, long customerid)
        {
            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);
            if (checkusedjeehr)
            {
                var accs = await _reponsitory.ListNhanVienCapDuoiDirectManagerByDirectManagerJeeHRAsync(username);
                return accs;
            }
            else
            {
                var accs = await _reponsitory.ListNhanVienCapDuoiDirectManagerByDirectManagerDefaultAsync(username);
                return accs;
            }
        }

        public async Task<IdentityServerReturn> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
                    var create = _reponsitory.CreateAccount(cnn, account, AdminUserID, customerID);
                    if (create.Susscess)
                    {
                        var identity = InitIdentityServerUserModel(cnn, customerID, account);
                        string userId = identity.customData.JeeAccount.UserID.ToString();
                        //GeneralService.saveImgNhanVien(account.ImageAvatar, userId, customerID);
                        string urlAvatar = apiUrl + $"images/nhanvien/{customerID}/{userId}.jpg";
                        identity.customData.PersonalInfo.Avatar = urlAvatar;
                        var createUser = await this.identityServerController.addNewUser(identity, Admin_accessToken);
                        var updateAvatar = _reponsitory.UpdateAvatarFirstTime(cnn, urlAvatar, long.Parse(userId), customerID);
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
                        identityServerReturn = TranferDataHelper.TranformIdentityServerReturnSqlModel(create);
                        return identityServerReturn;
                    }
                }
                catch (Exception ex)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    identityServerReturn = TranferDataHelper.TranformIdentityServerException(ex);
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
                    var create = _reponsitory.CreateAccount(cnn, account, AdminUserID, customerID);
                    if (create.Susscess)
                    {
                        var userid = long.Parse(GeneralReponsitory.GetUserIDByUsernameCnn(cnn, account.Username).ToString());
                        var saveAppList = _reponsitory.InsertAppCodeAccount(cnn, userid, ListAppID);
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
                catch (Exception)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    throw;
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
            personalInfo.Name = GeneralService.GetFirstname(account.Fullname);
            personalInfo.Avatar = account.ImageAvatar;
            personalInfo.Departmemt = account.Departmemt;
            personalInfo.Fullname = account.Fullname;
            personalInfo.Jobtitle = account.Jobtitle;
            personalInfo.Birthday = "";
            personalInfo.Phonenumber = account.Phonemumber;
            long idUser = _reponsitory.GetCurrentIdentity(cnn);
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
                    var update = _reponsitory.UpdatePersonalInfoCustomData(cnn, personalInfoCustom, userId, customerId);
                    if (update.Susscess)
                    {
                        string username = _reponsitory.GetUsername(cnn, userId, customerId);
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
                        identity = TranferDataHelper.TranformIdentityServerReturnSqlModel(update);
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
                    username = _reponsitory.GetUsername(cnn, objCustomData.userId, customerId);
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

                _reponsitory.UpdateAvatar(avatar, UserId, CustomerID);
                var personal = GeneralReponsitory.GetPersonalInfoCustomData(UserId, CustomerID, _connectionString);
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
                var username = GeneralReponsitory.GetUsernameByUserIDCnn(cnn, userID).ToString();
                return await this.identityServerController.ResetPasswordRootCustomer(getSecretToken(), username, model);
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
                userID = GeneralReponsitory.GetUserIDByUsername(_connectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralReponsitory.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                userID = model.Userid;
            }
            var appCodes = await _reponsitory.GetListAppByUserIDAsync(long.Parse(userID));
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
                userID = GeneralReponsitory.GetUserIDByUsername(_connectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralReponsitory.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                userID = model.Userid;
            }

            var customerId = GeneralReponsitory.GetCustomerIDByUsername(_connectionString, username);

            var personal = GeneralReponsitory.GetPersonalInfoCustomData(Int32.Parse(userID), Int32.Parse(customerId.ToString()), _connectionString);
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
                userID = GeneralReponsitory.GetUserIDByUsername(_connectionString, model.Username).ToString();
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                username = GeneralReponsitory.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                userID = model.Userid;
            }

            var customerId = GeneralReponsitory.GetCustomerIDByUsername(_connectionString, username);
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                _reponsitory.InsertAppCodeAccount(cnn, Int32.Parse(userID), lstAppCode);
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
                    userID = GeneralReponsitory.GetUserIDByUsername(_connectionString, model.Username).ToString();
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    username = GeneralReponsitory.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                    userID = model.Userid;
                }
                var customerId = GeneralReponsitory.GetCustomerIDByUsername(_connectionString, username);
                var staffid = GeneralReponsitory.GetStaffIDByUserID(_connectionString, userID);

                var custom = new JeeAccountCustomData();
                custom.customerID = long.Parse(customerId.ToString());
                if (staffid is not null)
                {
                    custom.staffID = long.Parse(staffid.ToString());
                }
                var lstApp = await _reponsitory.GetListAppByUserIDAsync(long.Parse(userID));
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

        #endregion api new
    }
}