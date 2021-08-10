using DPSinfra.Kafka;
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
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        private readonly string TopicAddNewCustomerUser;
        private readonly IProducer _producer;

        public AccountManagementService(IAccountManagementReponsitory reponsitory, IConfiguration configuration, IProducer producer)
        {
            _reponsitory = reponsitory;
            identityServerController = new IdentityServerController();
            HOST_MINIOSERVER = configuration.GetValue<string>("MinioConfig:MinioServer");
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _configuration = configuration;
            TopicAddNewCustomerUser = _configuration.GetValue<string>("KafkaConfig:TopicProduce:JeeplatformInitialization");
            _producer = producer;
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
                            { "chucvuid", "AccountList.JobtitleID"},
                            { "phongbanid", "AccountList.DepartmentID" },
                            {"quanlytructiep", "AccountList.DirectManager" }
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
                whereStr += $" and (AccountList.DepartmentID  in ({query.filter["phongbanid"]})) ";
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
            if (!string.IsNullOrEmpty(query.filter["email"]))
            {
                whereStr += $" and (AccountList.Email like N'%{query.filter["email"]}%') ";
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

        public async Task ChangeTinhTrang(string Admin_accessToken, long customerID, string Username, string Note, long UserIdLogin)
        {
            var isActive = _reponsitory.ChangeTinhTrang(customerID, Username, Note, UserIdLogin);
            var identity = new IdentityServerController();
            var response = await identity.changeUserStateAsync(Admin_accessToken, Username, !isActive);
            if (!response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<IdentityServerReturn>(await response.Content.ReadAsStringAsync());
                throw new Exception(res.message);
            }
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

        public async Task CreateAccount(bool isJeeHR, string Admin_accessToken, long customerID, string usernameCreatedBy, AccountManagementModel account)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            account.Fullname = textInfo.ToTitleCase(account.Fullname.Trim());
            var listAppCode = account.AppCode.ToList();
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                if (IsExistUsernameCnn(cnn, account.Username, customerID)) throw new TrungDuLieuExceoption("Username");
                if (!string.IsNullOrEmpty(account.ImageAvatar) && !isJeeHR) account.ImageAvatar = UpdateAvatar(account.Username, account.ImageAvatar);
                try
                {
                    cnn.BeginTransaction();
                    _reponsitory.CreateAccount(isJeeHR, cnn, account, usernameCreatedBy, customerID, false);
                    account.Userid = GeneralReponsitory.GetCommonInfoCnn(cnn, 0, account.Username).UserID;
                    //remove jeehr
                    if (isJeeHR)
                    {
                        var index = account.AppCode.FindIndex(item => item.Equals("JeeHR"));
                        account.AppID.RemoveAt(index);
                    }
                    _reponsitory.InsertAppCodeAccount(cnn, account.Userid, account.AppID, usernameCreatedBy, false);
                    var identity = InitIdentityServerUserModel(customerID, account);
                    string userId = identity.customData.JeeAccount.UserID.ToString();
                    var createUser = await identityServerController.addNewUser(identity, Admin_accessToken);
                    if (!createUser.IsSuccessStatusCode)
                    {
                        string returnValue = await createUser.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                        throw new Exception(res.message);
                    }
                    cnn.EndTransaction();
                    var obj = new
                    {
                        CustomerID = customerID,
                        AppCode = listAppCode,
                        UserID = account.Userid,
                        Username = account.Username,
                        IsInitial = false,
                        IsAdmin = false
                    };
                    await _producer.PublishAsync(TopicAddNewCustomerUser, JsonConvert.SerializeObject(obj));
                }
                catch (Exception)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    throw;
                }
            }
        }

        public async Task UpdateAccount(bool isJeeHR, string Admin_accessToken, long customerID, string usernameCreatedBy, AccountManagementModel account)
        {
            var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, 0, account.Username);
            account.Userid = commonInfo.UserID;
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            account.Fullname = textInfo.ToTitleCase(account.Fullname.Trim());
            var isAdminHeThong = GeneralReponsitory.IsAdminHeThong(_connectionString, account.Userid);
            var listAppCode = account.AppCode.ToList();
            var lstAppCurrentCustomer = await GetListAppByCustomerIDAsync(customerID);
            lstAppCurrentCustomer = lstAppCurrentCustomer.ToList();
            var lstAppIDCurrentCustomer = lstAppCurrentCustomer.Select(item => item.AppID).ToList();
            var lstAppCurrentUserid = GeneralReponsitory.GetListAppByUserID(_connectionString, account.Userid, customerID, false).ToList();
            var lstAppIDCurrentUserid = lstAppCurrentUserid.Select(item => item.AppID).ToList();

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                try
                {
                    cnn.BeginTransaction();
                    _reponsitory.UpdateAccount(isJeeHR, cnn, account, customerID);
                    if (isAdminHeThong)
                    {
                        cnn.EndTransaction();
                        return;
                    };

                    account.Userid = commonInfo.UserID;
                    var listInsertAppid = new List<int>();
                    var listInsertAppCode = new List<string>();
                    for (var index = 0; index < account.AppID.Count; index++)
                    {
                        if (!lstAppIDCurrentUserid.Contains(account.AppID[index])) listInsertAppid.Add(account.AppID[index]);
                        if (!lstAppIDCurrentUserid.Contains(account.AppID[index])) listInsertAppCode.Add(account.AppCode[index]);
                    }
                    //remove jeehr
                    if (isJeeHR)
                    {
                        var index = account.AppCode.FindIndex(item => item.Equals("JeeHR"));
                        listInsertAppid.RemoveAt(index);
                    }

                    _reponsitory.InsertAppCodeAccount(cnn, account.Userid, listInsertAppid, usernameCreatedBy, false);
                    var listRemove = checkNotExisAppID(lstAppIDCurrentUserid, lstAppIDCurrentCustomer);
                    _reponsitory.RemoveAppCodeAccount(cnn, account.Userid, listRemove, usernameCreatedBy);

                    var objCustomDataJeeAccount = identityServerController.JeeAccountCustomData(account.AppCode, commonInfo.UserID, customerID, commonInfo.StaffID);

                    var updateIndentity = await identityServerController.UppdateCustomDataHttpResponse(Admin_accessToken, commonInfo.Username, objCustomDataJeeAccount);

                    if (!updateIndentity.IsSuccessStatusCode)
                    {
                        string returnValue = await updateIndentity.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                        throw new Exception(res.message);
                    }
                    cnn.EndTransaction();
                    if (listInsertAppCode.Count > 0)
                    {
                        if (!isAdminHeThong)
                        {
                            var obj = new
                            {
                                CustomerID = customerID,
                                AppCode = listInsertAppCode,
                                UserID = account.Userid,
                                Username = account.Username,
                                IsInitial = false,
                                IsAdmin = false
                            };
                            await _producer.PublishAsync(TopicAddNewCustomerUser, JsonConvert.SerializeObject(obj));
                        }
                        else
                        {
                            var obj = new
                            {
                                CustomerID = customerID,
                                AppCode = listInsertAppCode,
                                UserID = account.Userid,
                                Username = account.Username,
                                IsInitial = true,
                                IsAdmin = false
                            };
                            await _producer.PublishAsync(TopicAddNewCustomerUser, JsonConvert.SerializeObject(obj));
                        }
                    }

                }
                catch (Exception)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    throw;
                }
            }
        }

        private List<int> checkNotExisAppID(List<int> appIDUser, List<int> appIdCustomer)
        {
            var lst = new List<int>();
            foreach (var id in appIdCustomer)
            {
                if (!appIDUser.Contains(id)) lst.Add(id);
            }
            return lst;
        }

        private IdentityServerAddNewUser InitIdentityServerUserModel(long customerID, AccountManagementModel account)
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
            personalInfo.Birthday = account.Birthday;
            personalInfo.Phonenumber = account.Phonemumber;
            personalInfo.DepartmemtID = account.DepartmemtID;
            personalInfo.Email = account.Email;
            personalInfo.Departmemt = account.Departmemt;
            personalInfo.Jobtitle = account.Jobtitle;
            personalInfo.JobtitleID = account.JobtitleID;
            personalInfo.Structure = "";
            personalInfo.StructureID = account.cocauid.ToString();
            personalInfo.BgColor = GeneralService.GetColorNameUser(personalInfo.Name);
            JeeAccountCustomDataModel jee = new JeeAccountCustomDataModel();
            jee.AppCode = account.AppCode;
            jee.CustomerID = customerID;
            jee.UserID = account.Userid;
            jee.StaffID = account.StaffID;
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
                string avatar = UpdateAvatar(Username, base64);

                _reponsitory.UpdateAvatar(avatar, UserId, CustomerID);
                var personal = GeneralReponsitory.GetPersonalInfoCustomData(UserId, CustomerID, _connectionString);
                var res = await identity.updateCustomDataPersonalInfoInternal(getSecretToken(), personal, Username);
                if (!res.IsSuccessStatusCode)
                {
                    string returnValue = await res.Content.ReadAsStringAsync();
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

        public string UpdateAvatar(string username, string base64)
        {
            try
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
                    throw new Exception("Cập nhật avatar thất bại");
                }
                return linkAvatar;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsExistUsernameCnn(DpsConnection cnn, string username, long customerid = 0)
        {
            string sql = "";
            if (customerid != 0)
            {
                sql = $"select Username from AccountList where Username = '{username}' and CustomerID = {customerid}";
            }
            else
            {
                sql = $"select Username from AccountList where Username = '{username}'";
            }
            return GeneralReponsitory.IsExistCnn(sql, cnn);
        }

        public async Task<IEnumerable<CheckEditAppListByDTO>> GetEditListAppByUserIDByListCustomerId(long userid, long customerid)
        {
            var appListCustomer = await GetListAppByCustomerIDAsync(customerid);
            var appIdCustomer = appListCustomer.Select(item => item.AppID).ToList();
            var appListUserid = await GeneralReponsitory.GetListAppByUserIDAsync(_connectionString, userid, customerid, false);
            var isAdminHeThong = GeneralReponsitory.IsAdminHeThong(_connectionString, userid);
            appListUserid = appListUserid.ToList();
            var lst = new List<CheckEditAppListByDTO>();
            foreach (var item in appListUserid)
            {
                var editApp = new CheckEditAppListByDTO();
                editApp.AppID = item.AppID;
                editApp.AppCode = item.AppCode;
                editApp.AppName = item.AppName;
                if (appIdCustomer.Contains(item.AppID) && item.IsActive)
                {
                    editApp.IsUsed = true;
                }
                else
                {
                    editApp.IsUsed = false;
                }
                if (isAdminHeThong || item.IsAdmin) editApp.Disable = true;
                lst.Add(editApp);
            }
            return lst;
        }

        #endregion api new
    }
}