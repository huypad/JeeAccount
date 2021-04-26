using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common = JeeAccount.Classes.Common;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/accountmanagement")]
    [ApiController]
    public class AccountManagementController: ControllerBase
    {
        private readonly IConfiguration _config;


        private readonly AccountManagementService accountManagementService;
        public AccountManagementController(IConfiguration configuration)
        {
            _config = configuration;
            accountManagementService = new AccountManagementService(_config.GetConnectionString("DefaultConnection"));

        }

        [HttpGet("usernamesByCustermerID")]
        public async Task<BaseModel<object>> GetListUsernameByCustormerID()
        {

            try
            {
        
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var usernames = await accountManagementService.GetListUsernameByCustormerID(customData.JeeAccount.CustomerID);
                if (usernames is null)
                    return JsonResultCommon.KhongTonTai("CustomerID");
                return JsonResultCommon.ThanhCong(usernames);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetCustormerIDByUsername/username={username}")]
        public async Task<BaseModel<object>> GetCustormerIDByUsername(string username)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var customerid = await accountManagementService.GetCustormerIDByUsername(username);
                if (customerid == 0)
                    return JsonResultCommon.KhongTonTai("username");
                return JsonResultCommon.ThanhCong(customerid);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetInfoAdminAccountByCustomerID")]
        public async Task<BaseModel<object>> GetInfoAdminAccountByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoAdminDTOs = await accountManagementService.GetInfoAdminAccountByCustomerID(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(infoAdminDTOs);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetInfoByCustomerID")]
        public async Task<BaseModel<object>> GetInfoByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoUser = await accountManagementService.GetInfoByCustomerID(customData.JeeAccount.CustomerID);
                return infoUser.Name is null ? JsonResultCommon.KhongTonTai("CustomerID") : JsonResultCommon.ThanhCong(infoUser);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetInfoByUsername/username={username}")]
        public async Task<BaseModel<object>> GetInfoByUsername(string username)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoUser = await accountManagementService.GetInfoByUsername(username);
                return infoUser.Name is null ? JsonResultCommon.KhongTonTai("CustomerID") : JsonResultCommon.ThanhCong(infoUser);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListAdminsByCustomerID")]
        public async Task<BaseModel<object>> GetListAdminsByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var admins = await accountManagementService.GetListAdminsByCustomerID(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(admins);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }


        [HttpGet("GetListAppByCustomerID")]
        public async Task<BaseModel<object>> GetListAppByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var appList = await accountManagementService.GetListAppByCustomerID(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(appList);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListUsernameByAppcode/appcode={appcode}")]
        public async Task<BaseModel<object>> GetListUsernameByAppcode(string appcode)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var accUser = await accountManagementService.GetListUsernameByAppcode(customData.JeeAccount.CustomerID, appcode);
                return JsonResultCommon.ThanhCong(accUser);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [Route("GetListAccountManagement")]
        [HttpGet]
        public async Task<BaseModel<object>> GetListAccountManagement([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var accManagement = await accountManagementService.GetListAccountManagement(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(accManagement);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("ChangeTinhTrang")]
        public async Task<BaseModel<object>> changeTinhTrang(AccChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                
                ReturnSqlModel update = accountManagementService.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.Username, acc.Note, customData.JeeAccount.UserID);
                if (!update.Susscess)
                {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST))
                    {
                        return JsonResultCommon.KhongTonTai("tài khoản");
                    }
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION))
                    {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return JsonResultCommon.ThatBai(update.ErrorMessgage);
                    }
                }
                return JsonResultCommon.ThanhCong(update);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("createAccount")]
        public async Task<BaseModel<object>> CreateAccount(AccountManagementModel account)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                string apiUrl = _config.GetValue<string>("JeeAccount:API");
                var create = await accountManagementService.CreateAccount(token, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID, account, apiUrl);
                if (create.data is null)
                {
                
                    return JsonResultCommon.ThatBai(create.message); 
                }
                return JsonResultCommon.ThanhCong(create.data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        #region api public bên ngoài 
        [HttpPost("UppdatePersonalInfo")]
        public async Task<BaseModel<object>> UppdatePersonalInfo(PersonalInfoCustomData personalInfoCustom, long UserID)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var update = await accountManagementService.UpdatePersonalInfoCustomData(token, personalInfoCustom, UserID, customData.JeeAccount.CustomerID);
                if (update.data is null)
                {
                    return JsonResultCommon.ThatBai(update.message);
                }
                return JsonResultCommon.ThanhCong(update.data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("UppdateCustomData")]
        public async Task<BaseModel<object>> UppdateCustomData(ObjCustomData objCustomData)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var update = await accountManagementService.UppdateCustomData(token, objCustomData, customData.JeeAccount.CustomerID);
                if (update.data is null)
                {
                    return JsonResultCommon.ThatBai(update.message);
                }
                return JsonResultCommon.ThanhCong(update.data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
        #endregion
        [HttpPost("UpdateAvatar")]
        public BaseModel<object> UpdateAvatar(PostImgModel img)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var UserID =  accountManagementService.GetUserIdByUsername(img.Username, customData.JeeAccount.CustomerID);
                GeneralService.saveImgNhanVien(img.imgFile, UserID.ToString(), customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong();
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
        [HttpPost("UpdateAvatarWithChangeUrlAvatar")]
        public async Task<BaseModel<object>> UpdateAvatarWithChangeUrlAvatar(PostImgModel img)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                string apiUrl = _config.GetValue<string>("JeeAccount:API");
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var UserID = accountManagementService.GetUserIdByUsername(img.Username, customData.JeeAccount.CustomerID);
                var updateAvatar = await accountManagementService.UpdateAvatarWithChangeUrlAvatar(token, UserID, img.Username, customData.JeeAccount.CustomerID, apiUrl);
                if (updateAvatar.data is null)
                {
                    return JsonResultCommon.ThatBai(updateAvatar.message);
                }
                GeneralService.saveImgNhanVien(img.imgFile, UserID.ToString(), customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(updateAvatar.data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
        [HttpPost("UpdateDirectManager")]
        public async Task<object> UpdateDirectManager(AccDirectManagerModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var update = accountManagementService.UpdateDirectManager(acc.Username, acc.DirectManager, customData.JeeAccount.CustomerID);
                if (!update.Susscess)
                {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION))
                    {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return JsonResultCommon.ThatBai(update.ErrorMessgage);
                    }
                }
                return JsonResultCommon.ThanhCong(update);

            } catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
    }

}
