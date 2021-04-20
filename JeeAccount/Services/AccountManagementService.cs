﻿using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class AccountManagementService
    {
        private IAccountManagementReponsitory accountManagementReponsitory;
        private IdentityServerController identityServerController;
        private readonly string ConnectionString;
        public AccountManagementService(string connectionString)
        {
            this.accountManagementReponsitory = new AccountManagementReponsitory(connectionString);
            this.identityServerController = new IdentityServerController();
            this.ConnectionString = connectionString;
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

        public Task<InfoUserDTO> GetInfoByCustomerID(long customerID)
        {
            var user = accountManagementReponsitory.GetInfoByCustomerID(customerID);
            return user;
        }

        public Task<InfoUserDTO> GetInfoByUsername(string username)
        {
            var user = accountManagementReponsitory.GetInfoByUsername(username);
            return user;
        }

        public Task<IEnumerable<AccUsernameModel>> GetListAdminsByCustomerID(long customerID)
        {
            var admins = accountManagementReponsitory.GetListAdminsByCustomerID(customerID);
            return admins;
        }

        public Task<IEnumerable<AppListDTO>> GetListAppByCustomerID(long customerID)
        {
            var appList = accountManagementReponsitory.GetListAppByCustomerID(customerID);
            return appList;
        }

        public Task<IEnumerable<AccUsernameModel>> GetListUsernameByAppcode(long customerID, string appcode)
        {
            var accUser = accountManagementReponsitory.GetListUsernameByAppcode(customerID, appcode);
            return accUser;
        }
        public Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID)
        {
            var accManagement = accountManagementReponsitory.GetListAccountManagement(customerID);
            return accManagement;
        }

        public Task<ReturnSqlModel> ChangeTinhTrang(long customerID, string Username)
        {
            var update = accountManagementReponsitory.ChangeTinhTrang(customerID, Username);
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
        public async Task<ReturnSqlModel> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
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
                            return new ReturnSqlModel(createUser.message + " Error code: " + createUser.statusCode, Constant.ERRORCODE_EXCEPTION);
                        }
                        cnn.EndTransaction();
                        return create;
                    }
                    else
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return create;
                    }
    
                } catch (Exception ex)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
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

        public async Task<IdentityServerReturn> login(string username, string password)
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
            } catch (Exception ex)
            {
                identity.message = ex.Message;
                identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
                return identity;
            } 
        }
    }
}
