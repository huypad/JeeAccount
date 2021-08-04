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
            if (query.more)
            {
                var x = 11;
            }
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

        public async Task<IEnumerable<AccUsernameModel>> GetListAccUsernameModel(long customerid)
        {
            var lst = Enumerable.Empty<AccUsernameModel>();

            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);
            if (checkusedjeehr)
            {
                lst = await _reponsitory.GetListAccUsernameModelIsJeeHRAsync(customerid).ConfigureAwait(false);
            }
            else
            {
                lst = await _reponsitory.GetListAccUsernameModelDefaultAsync(customerid).ConfigureAwait(false);
            }

            return lst;
        }

        public async Task<IEnumerable<long>> GetListJustCustormerID()
        {
            return await _reponsitory.GetListJustCustormerID().ConfigureAwait(false);
        }

        public async Task<IEnumerable<long>> GetListJustCustormerIDAppCode(string appCode)
        {
            return await _reponsitory.GetListJustCustormerIDAppCode(appCode).ConfigureAwait(false);
        }

        public async Task<long> GetCustormerID(InputApiModel model)
        {
            if (!string.IsNullOrEmpty(model.Username))
            {
                var customerid = await GetCustormerIDByUsernameAsync(model.Username);
                if (customerid != 0)
                    return customerid;
            }

            if (!string.IsNullOrEmpty(model.Userid))
            {
                var customerid = await GetCustormerIDByUserIDAsync(long.Parse(model.Userid));
                if (customerid != 0)
                    return customerid;
            }
            return 0;
        }

        public async Task<long> GetCustormerIDByUsernameAsync(string username)
        {
            return await _reponsitory.GetCustormerIDByUsernameAsync(username).ConfigureAwait(false);
        }

        public async Task<long> GetCustormerIDByUserIDAsync(long UserId)
        {
            return await _reponsitory.GetCustormerIDByUserIDAsync(UserId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerIDAsync(long customerID)
        {
            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerID);
            if (checkusedjeehr)
            {
                var infoAdminDTOs = await _reponsitory.GetInfoAdminAccountByCustomerIDIsJeeHRAsync(customerID);
                return infoAdminDTOs;
            }
            else
            {
                var infoAdminDTOs = await _reponsitory.GetInfoAdminAccountByCustomerIDDefaultAsync(customerID);
                return infoAdminDTOs;
            }
        }

        public async Task<InfoCustomerDTO> GetInfoByCustomerIDAsync(long customerID)
        {
            return await _reponsitory.GetInfoByCustomerIDAsync(customerID).ConfigureAwait(false);
        }

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

        public async Task<IEnumerable<AdminModel>> GetListAdminsByCustomerIDAsync(long customerID)
        {
            return await _reponsitory.GetListAdminsByCustomerIDAsync(customerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<AppListDTO>> GetListAppByCustomerIDAsync(long customerID)
        {
            return await _reponsitory.GetListAppByCustomerIDAsync(customerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcodeAsync(long custormerID, string appcode)
        {
            return await _reponsitory.GetListUsernameByAppcodeAsync(custormerID, appcode).ConfigureAwait(false);
        }

        public async Task<string> GetDirectManagerByUserID(string userid)
        {
            return await _reponsitory.GetDirectManagerByUserID(userid).ConfigureAwait(false);
        }

        public async Task<string> GetDirectManagerByUsername(string username)
        {
            return await _reponsitory.GetDirectManagerByUsername(username).ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserNameDTO>> GetListJustUsernameAndUserIDByCustormerID(long custormerID)
        {
            return await _reponsitory.GetListJustUsernameAndUserIDByCustormerID(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetListJustUsernameByCustormerID(long custormerID)
        {
            return await _reponsitory.GetListJustUsernameByCustormerID(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<long>> GetListJustUserIDByCustormerID(long custormerID)
        {
            return await _reponsitory.GetListJustUserIDByCustormerID(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetListDirectManager(long custormerID)
        {
            return await _reponsitory.GetListDirectManager(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccountAsync(long customerID)
        {
            return await _reponsitory.GetListCustomerAppByCustomerIDFromAccountAsync(customerID).ConfigureAwait(false);
        }

        #endregion api giao diện

        public ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin)
        {
            return _reponsitory.ChangeTinhTrang(customerID, Username, Note, UserIdLogin);
        }

        public ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID)
        {
            return _reponsitory.UpdateDirectManager(Username, DirectManager, customerID);
        }

        #region api new

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
                        var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, 0, account.Username);
                        var userid = commonInfo.UserID;
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
                        var commonInfo = GeneralReponsitory.GetCommonInfoCnn(cnn, userId);
                        string username = commonInfo.Username;
                        var updateCustom = await identityServerController.updateCustomDataPersonalInfo(Admin_access_token, personalInfoCustom, username);
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
                    var commonInfo = GeneralReponsitory.GetCommonInfoCnn(cnn, objCustomData.userId);
                    username = commonInfo.Username;
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
                var commoninfo = GeneralReponsitory.GetCommonInfoCnn(cnn, long.Parse(userID));
                var username = commoninfo.Username;
                return await identityServerController.ResetPasswordRootCustomer(getSecretToken(), username, model).ConfigureAwait(false);
            }
        }

        public string getSecretToken()
        {
            var secret = _configuration.GetValue<string>("Jwt:internal_secret");
            var projectName = _configuration.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
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
            var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);

            var personal = GeneralReponsitory.GetPersonalInfoCustomData(commonInfo.UserID, commonInfo.CustomerID, _connectionString);
            personal.BgColor = GeneralService.GetColorNameUser(personal.Name[0].ToString());
            var jwt_internal = getSecretToken();

            var objCustom = new ObjCustomData();
            objCustom.userId = commonInfo.UserID;
            objCustom.updateField = "personalInfo";
            objCustom.fieldValue = personal;
            return await identityServerController.UpdateCustomDataInternal(jwt_internal, commonInfo.Username, objCustom);
        }

        public void UpdateAllAppCodesCustomData(InputApiModel model, List<int> lstAppCode)
        {
            var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                _reponsitory.InsertAppCodeAccount(cnn, commonInfo.UserID, lstAppCode);
            }
        }

        public async Task<JeeAccountCustomData> GetJeeAccountCustomDataAsync(InputApiModel model)
        {
            try
            {
                var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);

                var custom = new JeeAccountCustomData();
                custom.customerID = commonInfo.CustomerID;
                custom.staffID = commonInfo.StaffID;
                var lstApp = await GeneralReponsitory.GetListAppByUserIDAsync(_connectionString, commonInfo.UserID);
                custom.appCode = lstApp.Select(item => item.AppCode).ToList();
                custom.userID = commonInfo.UserID;
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