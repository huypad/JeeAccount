﻿using DPSinfra.Kafka;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Models.JeeHR;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using JeeAccount.Services.AccountManagementService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static JeeAccount.Models.Common.Panigator;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/accountmanagement")]
    [ApiController]
    public class AccountManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountManagementService _accountManagementService;
        private readonly string _JeeCustomerSecrectkey;
        private readonly string _internal_secret;
        private readonly string HOST_JEEHR_API;
        private readonly string _connectionString;
        private readonly IProducer _producer;

        public AccountManagementController(IConfiguration configuration, IAccountManagementService accountManagementService, IProducer producer)
        {
            _config = configuration;
            _accountManagementService = accountManagementService;
            _JeeCustomerSecrectkey = configuration.GetValue<string>("AppConfig:Secrectkey:JeeCustomer");
            _internal_secret = configuration["Jwt:internal_secret"];
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _producer = producer;
        }

        #region giao diện JeeAccount  Management/AccountManagement

        [Route("GetListAccountManagement")]
        [HttpPost]
        public async Task<IActionResult> GetListAccountManagement([FromBody] QueryRequestParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                query = query == null ? new QueryRequestParams() : query;
                BaseModel<object> model = new BaseModel<object>();
                PageModel pageModel = new PageModel();
                ErrorModel error = new ErrorModel();

                string orderByStr = "AccountList.UserID asc";
                string whereStr = "CustomerID = @CustomerID and (Disable != 1 or Disable is null)";

                Dictionary<string, string> filter = new Dictionary<string, string>
                        {
                            { "nhanvien", "AccountList.FullName"},
                            { "tendangnhap", "AccountList.Username"},
                            { "tinhtrang", "AccountList.IsActive"},
                            { "chucvu", "AccountList.Jobtitle"},
                        };
                if (query.Sort != null)
                {
                    if (!string.IsNullOrEmpty(query.Sort.ColumnName) && filter.ContainsKey(query.Sort.ColumnName))
                    {
                        orderByStr = filter[query.Sort.ColumnName] + " " + (query.Sort.Direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc");
                    }
                }

                if (!string.IsNullOrEmpty(query.SearchValue))
                {
                    whereStr += $" and (AccountList.LastName + ' ' + AccountList.FirstName like '%{query.SearchValue}%' " +
                        $"or AccountList.Jobtitle like '%{query.SearchValue}%' " +
                        $"or AccountList.Username like '%{query.SearchValue}%')";
                }

                var lst = await _accountManagementService.GetListAccountManagement(customData.JeeAccount.CustomerID, whereStr, orderByStr);

                pageModel.TotalCount = lst.Count();
                pageModel.AllPage = (int)Math.Ceiling(lst.Count() / (decimal)query.Panigator.PageSize);
                pageModel.Size = query.Panigator.PageSize;
                pageModel.Page = query.Panigator.PageIndex;
                lst = lst.AsEnumerable().Skip((query.Panigator.PageIndex - 1) * query.Panigator.PageSize).Take(query.Panigator.PageSize);

                return Ok(MessageReturnHelper.Ok(lst, pageModel));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("UpdateAvatarWithChangeUrlAvatar")]
        public async Task<IActionResult> UpdateAvatarWithChangeUrlAvatar(PostImgModel img)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var userid = GeneralService.GetUserIDByUsername(_connectionString, img.Username);
                await _accountManagementService.UpdateAvatarWithChangeUrlAvatar(Convert.ToInt64(userid), img.Username, customData.JeeAccount.CustomerID, img.imgFile);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #endregion giao diện JeeAccount  Management/AccountManagement

        [HttpGet("usernamesByCustermerID")]
        public async Task<object> GetListUsernameByCustormerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var usernames = await _accountManagementService.GetListUsernameByCustormerIDAsync(customData.JeeAccount.CustomerID);
                if (usernames is null)
                    return JsonResultCommon.KhongTonTai("CustomerID");
                return JsonResultCommon.ThanhCong(usernames);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("usernamesByCustermerID/internal/{customerID}")]
        public async Task<IActionResult> GetListUsernameByCustormerIDInternal(long customerID)
        {
            try
            {
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return Unauthorized(MessageReturnHelper.Unauthorized());
                }
                var usernames = await _accountManagementService.GetListUsernameByCustormerIDAsync(customerID);
                if (usernames.Count() == 0)
                {
                    return NotFound(MessageReturnHelper.Custom("Không tồn tại khách hàng này"));
                }
                return Ok(usernames);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetListCustomerID/internal")]
        public async Task<IActionResult> GetListCustomerID()
        {
            try
            {
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return Unauthorized(MessageReturnHelper.Unauthorized());
                }
                var list = await _accountManagementService.GetListJustCustormerID();
                if (list.Count() == 0)
                {
                    return NotFound(MessageReturnHelper.Custom("Không tồn tại khách hàng"));
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetListCustomerID/internal/{appCode}")]
        public async Task<IActionResult> GetListCustomerIDAppCode(string appCode)
        {
            try
            {
                var isInternalToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _internal_secret);
                if (!isInternalToken)
                {
                    return Unauthorized(MessageReturnHelper.Unauthorized());
                }
                var list = await _accountManagementService.GetListJustCustormerIDAppCode(appCode);
                if (list.Count() == 0)
                {
                    return NotFound(MessageReturnHelper.Custom("Không tồn tại khách hàng có appCode này"));
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetCustormerIDByUsername/username={username}")]
        public async Task<object> GetCustormerIDByUsername(string username)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var customerid = await _accountManagementService.GetCustormerIDByUsername(username);
                if (customerid == 0)
                    return JsonResultCommon.KhongTonTai("username");
                return JsonResultCommon.ThanhCong(customerid);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("GetCustormerID")]
        public async Task<ActionResult<long>> GetCustormerID(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var customerid = await _accountManagementService.GetCustormerIDByUsername(model.Username);
                    if (customerid != 0)
                        return Ok(customerid);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var customerid = await _accountManagementService.GetCustormerIDByUserID(long.Parse(model.Userid));
                    if (customerid != 0)
                        return Ok(customerid);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetInfoAdminAccountByCustomerID")]
        public async Task<object> GetInfoAdminAccountByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoAdminDTOs = await _accountManagementService.GetInfoAdminAccountByCustomerID(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(infoAdminDTOs);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetInfoByCustomerID")]
        public async Task<object> GetInfoByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoUser = await _accountManagementService.GetInfoByCustomerID(customData.JeeAccount.CustomerID);
                return infoUser is null ? JsonResultCommon.KhongTonTai("CustomerID") : JsonResultCommon.ThanhCong(infoUser);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetInfoByUsername/username={username}")]
        public async Task<object> GetInfoByUsername(string username)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoUser = await _accountManagementService.GetInfoByUsername(username);
                return infoUser is null ? JsonResultCommon.KhongTonTai("username") : JsonResultCommon.ThanhCong(infoUser);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("GetInfo")]
        public async Task<ActionResult<InfoUserDTO>> GetInfo(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var infoUser = await _accountManagementService.GetInfoByUsername(model.Username);
                    if (infoUser != null)
                        return Ok(infoUser);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var infoUser = await _accountManagementService.GetInfoByUserID(model.Userid);
                    if (infoUser != null)
                        return Ok(infoUser);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetListAdminsByCustomerID")]
        public async Task<object> GetListAdminsByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var admins = await _accountManagementService.GetListAdminsByCustomerID(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(admins);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListAppByCustomerID")]
        public async Task<object> GetListAppByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var appList = await _accountManagementService.GetListAppByCustomerID(customData.JeeAccount.CustomerID);

                return JsonResultCommon.ThanhCong(appList);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListAppByUserID")]
        public async Task<object> GetListAppByUserID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var appList = await _accountManagementService.GetListAppByUserID(customData.JeeAccount.UserID, customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(appList);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListUsernameByAppcode/appcode={appcode}")]
        public async Task<object> GetListUsernameByAppcode(string appcode)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var accUser = await _accountManagementService.GetListUsernameByAppcode(customData.JeeAccount.CustomerID, appcode);
                return JsonResultCommon.ThanhCong(accUser);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("ChangeTinhTrang")]
        public async Task<object> changeTinhTrang(AccChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }

                ReturnSqlModel update = _accountManagementService.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.Username, acc.Note, customData.JeeAccount.UserID);
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
        public async Task<object> CreateAccount(AccountManagementModel account)
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
                var create = await _accountManagementService.CreateAccount(token, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID, account, apiUrl);
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

        [HttpPost("GetInfoDirectManager")]
        public async Task<IActionResult> GetInfoDirectManager(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var usernameQuanLy = "";
                if (!string.IsNullOrEmpty(model.Userid))
                {
                    usernameQuanLy = await _accountManagementService.GetDirectManagerByUserID(model.Userid);
                    var result = await _accountManagementService.GetInfoByUsername(usernameQuanLy);
                    if (result.Fullname != null)
                    {
                        return Ok(result);
                    }
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    usernameQuanLy = await _accountManagementService.GetDirectManagerByUsername(model.Username);
                    var result = await _accountManagementService.GetInfoByUsername(usernameQuanLy);
                    if (result.Fullname is not null)
                    {
                        return Ok(result);
                    }
                }

                return BadRequest(MessageReturnHelper.Custom("quản lý trực tiếp không tồn tại hoặc userid và username không hợp lệ"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #region api public bên ngoài

        [HttpPost("UppdatePersonalInfo")]
        public async Task<object> UppdatePersonalInfo(PersonalInfoCustomData personalInfoCustom, long UserID)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var update = await _accountManagementService.UpdatePersonalInfoCustomData(token, personalInfoCustom, UserID, customData.JeeAccount.CustomerID);
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
        public async Task<object> UppdateCustomData(ObjCustomData objCustomData)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var update = await _accountManagementService.UppdateCustomData(token, objCustomData, customData.JeeAccount.CustomerID);
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

        #endregion api public bên ngoài

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
                var update = _accountManagementService.UpdateDirectManager(acc.Username, acc.DirectManager, customData.JeeAccount.CustomerID);
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
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("ListUsernameAndUserid")]
        public async Task<IActionResult> ListUsernameAndUserid()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _accountManagementService.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
                if (lstUsername is not null)
                {
                    return Ok(lstUsername);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("CustomerID"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("ListUsername")]
        public async Task<IActionResult> ListUsername()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _accountManagementService.GetListJustUsernameByCustormerID(customData.JeeAccount.CustomerID);
                if (lstUsername is not null)
                {
                    return Ok(lstUsername);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("CustomerID"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("ListUserID")]
        public async Task<IActionResult> ListUserID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _accountManagementService.GetListJustUserIDByCustormerID(customData.JeeAccount.CustomerID);
                if (lstUsername is not null)
                {
                    return Ok(lstUsername);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("CustomerID"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("UsernameOrUserID")]
        public async Task<IActionResult> UsernameOrUserID(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Userid))
                {
                    {
                        var result = await _accountManagementService.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
                        var item = result.ToList().Where(ele => ele.Username.Equals(model.Username)).SingleOrDefault();
                        if (item != null)
                            return Ok(item);
                        var item2 = result.ToList().Where(ele => ele.UserId == long.Parse(model.Userid)).SingleOrDefault();
                        if (item2 != null)
                            return Ok(item2);
                    }
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var result = _accountManagementService.GetUserIdByUsername(model.Username, customData.JeeAccount.CustomerID);
                    if (result != 0)
                        return Ok(result.ToString());
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var result = _accountManagementService.GetUsernameByUserID(model.Userid, customData.JeeAccount.CustomerID);
                    if (!string.IsNullOrEmpty(result))
                        return Ok(result);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetListDirectManager")]
        public async Task<IActionResult> GetListDirectManager()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _accountManagementService.GetListDirectManager(customData.JeeAccount.CustomerID);
                if (lstUsername is not null)
                {
                    return Ok(lstUsername);
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("CustomerID"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("ListNhanVienCapDuoiDirectManager")]
        public async Task<IActionResult> ListNhanVienCapDuoiDirectManager(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var result = await _accountManagementService.ListNhanVienCapDuoiDirectManagerByDirectManager(model.Username);
                    if (result.Count() > 0)
                        return Ok(result);
                    else
                    {
                        return BadRequest(MessageReturnHelper.KhongTonTai("Nhân viên cấp dưới"));
                    }
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var username = _accountManagementService.GetUsernameByUserID(model.Userid, customData.JeeAccount.CustomerID);
                    var result = await _accountManagementService.ListNhanVienCapDuoiDirectManagerByDirectManager(username);

                    if (result.Count() > 0)
                        return Ok(result);
                    else
                    {
                        return BadRequest(MessageReturnHelper.KhongTonTai("Nhân viên cấp dưới"));
                    }
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("UserId hoặc Username"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("GetDSNhanVienTheoQuanLyTrucTiepFromJeeHR")]
        public async Task<IActionResult> GetDSNhanVienTheoQuanLyTrucTiepFromJeeHR(InputApiModel model)
        {
            try
            {
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (access_token is null)
                {
                    return NotFound();
                }
                long staffid = 0;
                if (!string.IsNullOrEmpty(model.Username))
                {
                    var x = GeneralService.GetStaffIDByUsername(_connectionString, model.Username).ToString();
                    if (string.IsNullOrEmpty(x)) return BadRequest(MessageReturnHelper.KhongTonTai("StaffID"));
                    staffid = long.Parse(x);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var x = GeneralService.GetStaffIDByUserID(_connectionString, model.Userid).ToString();
                    if (string.IsNullOrEmpty(x)) return BadRequest(MessageReturnHelper.KhongTonTai("StaffID"));
                    staffid = long.Parse(x);
                }

                var jeehrController = new JeeHRController(HOST_JEEHR_API);
                var data = await jeehrController.GetDSNhanVienTheoQuanLyTrucTiep(access_token, staffid);
                if (data.status > 0)
                {
                    var res = jeehrController.ConverNhanVienDuocQuanLyTrucTiep_(data.data);
                    return Ok(res);
                }
                else
                {
                    return Ok(data.error);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #region api for customer

        [HttpGet("GetListCustomerAppByCustomerIDFromCustomer")]
        public async Task<IActionResult> GetListCustomerAppByCustomerIDFromCustomer(long CustomerID)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var create = await _accountManagementService.GetListCustomerAppByCustomerIDFromAccount(CustomerID);
                return Ok(create);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ResetPasswordRootCustomer")]
        public async Task<IActionResult> ResetPasswordRootCustomer(CustomerResetPasswordModel model)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var response = await _accountManagementService.ResetPasswordRootCustomer(model);
                if (response.IsSuccessStatusCode)
                    return Ok(response);

                return Unauthorized(new { statusCode = response.StatusCode, message = response.Content.ReadAsStringAsync() });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion api for customer

        #region api for jeehr

        [HttpGet("updateAllStaffIDAll")]
        public async Task<IActionResult> UpdateAllStaffID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstCutomer = await _accountManagementService.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    var lstUsername = await _accountManagementService.GetListJustUsernameByCustormerID(customerid);

                    foreach (string username in lstUsername)
                    {
                        var model = new InputApiModel();
                        model.Username = username;
                        model.StaffID = null;
                        model.Userid = null;
                        var reponse = await _accountManagementService.UpdateOneStaffIDByInputApiModel(model);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("updateAllStaffID")]
        public async Task<IActionResult> UpdateAllStaffIDInternal(long customerid)
        {
            try
            {
                var messageError = "";
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return Unauthorized(MessageReturnHelper.Unauthorized());
                }

                var lstUsername = await _accountManagementService.GetListJustUsernameByCustormerID(customerid);
                if (lstUsername is null) return NotFound(MessageReturnHelper.KhongTonTai("customerid"));
                foreach (string username in lstUsername)
                {
                    var model = new InputApiModel();
                    model.Username = username;
                    model.StaffID = null;
                    model.Userid = null;
                    var reponse = await _accountManagementService.UpdateOneStaffIDByInputApiModel(model);
                    if (!reponse.IsSuccessStatusCode)
                    {
                        if (string.IsNullOrEmpty(messageError))
                        {
                            messageError = model.Username;
                        }
                        else
                        {
                            messageError += " ," + model.Username;
                        }
                    }
                }

                if (string.IsNullOrEmpty(messageError))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(MessageReturnHelper.Custom($"danh sách username bị lỗi: {messageError}"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("updateOneStaffID")]
        public async Task<IActionResult> updateOneStaffID(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                if (model == null)
                {
                    return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
                }
                var reponse = await _accountManagementService.UpdateOneStaffIDByInputApiModel(model);
                if (reponse.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(MessageReturnHelper.Exception(new Exception(reponse.ReasonPhrase)));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("updateOneStaffID/internal")]
        public async Task<IActionResult> updateOneStaffIDInternal(InputApiModel model)
        {
            try
            {
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return Unauthorized(MessageReturnHelper.Unauthorized());
                }
                if (model == null)
                {
                    return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
                }
                var reponse = await _accountManagementService.UpdateOneStaffIDByInputApiModel(model);
                if (reponse.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(MessageReturnHelper.Exception(new Exception(reponse.ReasonPhrase)));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #endregion api for jeehr

        #region code cho anh Thiên

        [HttpGet("UpdateAllBgColorCustomData")]
        public async Task<IActionResult> updateBgColorAll()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstCutomer = await _accountManagementService.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    if (customerid == 25)
                    {
                        var lstUsername = await _accountManagementService.GetListJustUsernameByCustormerID(customerid);

                        foreach (string username in lstUsername)
                        {
                            if (username.Equals("vanluc"))
                            {
                                var model = new InputApiModel();
                                model.Username = username;
                                model.Userid = null;
                                var reponse = await _accountManagementService.UpdateOneBgColorCustomData(model);
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("UpdateAllAppCodesCustomData")]
        public async Task<IActionResult> UpdateAllAppCodesCustomData()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstCutomer = await _accountManagementService.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    if (customerid == 25)
                    {
                        var lstUsername = await _accountManagementService.GetListJustUsernameByCustormerID(customerid);
                        var lstAppCode = await _accountManagementService.GetListAppByCustomerID(customerid);
                        var lstAppID = lstAppCode.Select(x => x.AppID).ToList();
                        foreach (string username in lstUsername)
                        {
                            var model = new InputApiModel();
                            model.Username = username;
                            model.Userid = null;
                            _accountManagementService.UpdateAllAppCodesCustomData(model, lstAppID);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #endregion code cho anh Thiên

        #region api gọi một lân

        [HttpGet("ApiCreateAccountDungMotLanpersonalInfoCustom")]
        public async Task<IActionResult> ApiCreateAccountDungMotLanpersonalInfoCustom()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var jeehr = new JeeHRController(HOST_JEEHR_API);
                var dataJeehr = await jeehr.GetDSNhanVien(access_token);
                var listExist = "";
                var listNotExist = "";
                var customerID = 25;
                var listApp = await _accountManagementService.GetListAppByCustomerID(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listfail = "";
                if (dataJeehr.status == 1)
                {
                    var identity = new IdentityServerController();
                    foreach (var data in dataJeehr.data)
                    {
                        if (data.username is null) continue;

                        var creacteAccount = await identity.addNewUserInternalWithoutCustomData(data.username, data.cmnd, getSecretToken());
                        if (!creacteAccount.IsSuccessStatusCode)
                        {
                            listExist += $"{data.username} ";
                            var personalInfoCustom = new PersonalInfoCustomData();
                            personalInfoCustom.Fullname = data.HoTen;
                            personalInfoCustom.Name = GeneralService.getFirstname(data.HoTen);
                            personalInfoCustom.Avatar = data.avatar;
                            personalInfoCustom.BgColor = GeneralService.GetColorNameUser(GeneralService.getFirstname(data.HoTen).Substring(0, 1));
                            personalInfoCustom.Jobtitle = data.TenChucVu;
                            personalInfoCustom.Structure = data.Structure;
                            personalInfoCustom.Birthday = data.NgaySinh;
                            personalInfoCustom.Email = data.Email;

                            var userid = long.Parse(GeneralService.GetUserIDByUsername(_connectionString, data.username).ToString());

                            var res = await _accountManagementService.UpdatePersonalInfoCustomData(access_token, personalInfoCustom, userid, 25);
                            if (res.statusCode != 0)
                            {
                                listfail += $"{data.username}, ";
                            }
                        }
                        else
                        {
                            listNotExist += $"{data.username} ";
                        }
                    }
                }
                return Ok(new { listExist = listExist, listNotExist = listNotExist, listfail = listfail });
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("ApiCreateAccountDungMotLan")]
        public async Task<IActionResult> ApiCreateAccountDungMotLan()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var jeehr = new JeeHRController(HOST_JEEHR_API);
                var dataJeehr = await jeehr.GetDSNhanVien(access_token);
                var listExist = "";
                var listNotExist = "";
                var customerID = 25;
                var listApp = await _accountManagementService.GetListAppByCustomerID(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listfail = "";
                var lstIdCustomer = await _accountManagementService.GetListJustUserIDByCustormerID(25);
                var ListID = new List<long>();
                var ListIDNotExist = new List<long>();
                if (dataJeehr.status == 1)
                {
                    var identity = new IdentityServerController();
                    foreach (var data in dataJeehr.data)
                    {
                        if (data.username is null) continue;
                        if (data.username == " dps.vanluc")
                        {
                            var userid = long.Parse(GeneralService.GetUserIDByUsername(_connectionString, data.username).ToString());
                            ListID.Add(userid);
                        }
                    }
                }
                foreach (var id in lstIdCustomer)
                {
                    if (!ListID.Contains(id))
                    {
                        ListIDNotExist.Add(id);
                    }
                }

                listNotExist = string.Join(",", ListIDNotExist);
                return Ok(new { ListID = ListID, lstIdCustomer = lstIdCustomer, ListIDNotExist = ListIDNotExist, listNotExist = listNotExist });
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetJeeAccountCustomDataAsync")]
        public async Task<IActionResult> GetJeeAccountCustomDataAsync()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var listExist = "";
                var listNotExist = "";
                var customerID = 25;
                var listApp = await _accountManagementService.GetListAppByCustomerID(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listfail = "";
                var lstIdCustomer = await _accountManagementService.GetListJustUserIDByCustormerID(25);
                var ListID = new List<long>();
                var ListIDNotExist = new List<long>();
                var identity = new IdentityServerController();
                foreach (var userid in lstIdCustomer)
                {
                    InputApiModel model = new InputApiModel();
                    model.Userid = userid.ToString();
                    var value = await _accountManagementService.GetJeeAccountCustomDataAsync(model);
                    var Obj = new ObjCustomData();
                    Obj.userId = userid;
                    Obj.updateField = "jee-account";
                    Obj.fieldValue = value;
                    var username = GeneralService.GetUsernameByUserID(_connectionString, userid.ToString()).ToString();
                    var res = await identity.UppdateCustomDataInternal(getSecretToken(), username, Obj);
                    if (res.statusCode != 0)
                    {
                        listfail += $"{userid}";
                    }
                    else
                    {
                        listExist += $"{userid}";
                    }
                }

                return Ok(new { listExist = listExist, listfail = listfail });
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetInternalToken")]
        public IActionResult GetInternalToken()
        {
            try
            {
                return Ok(getSecretToken());
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("postKafka")]
        public async Task<IActionResult> postKafka()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                var listExist = "";
                var listNotExist = "";
                var customerID = 25;
                var listApp = await _accountManagementService.GetListAppByCustomerID(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listAppCode = listApp.Select(item => item.AppCode).ToList();
                var listfail = "";
                var lstIdCustomer = await _accountManagementService.GetListJustUserIDByCustormerID(25);
                var ListID = new List<long>();
                var ListIDNotExist = new List<long>();
                var identity = new IdentityServerController();
                var lstusername = new List<string>();
                var lstuserid = new List<long>();

                var lstStringApp = new List<string>();
                lstStringApp.Add("JeeHR");

                foreach (var userid in lstIdCustomer)
                {
                    InputApiModel model = new InputApiModel();
                    model.Userid = userid.ToString();
                    var value = await _accountManagementService.GetJeeAccountCustomDataAsync(model);
                    var Obj = new ObjCustomData();
                    Obj.userId = userid;
                    Obj.updateField = "jee-account";
                    Obj.fieldValue = value;
                    var username = GeneralService.GetUsernameByUserID(_connectionString, userid.ToString()).ToString();

                    if (username == " dps.vanluc" || username == "dps.vanluc" || username == "vanluc")
                    {
                        continue;
                    }
                    var obj = new
                    {
                        CustomerID = 25,
                        AppCode = lstStringApp,
                        UserID = userid,
                        Username = username,
                        IsInitial = false,
                        IsAdmin = false
                    };

                    string TopicAddNewCustomer = _config.GetValue<string>("KafkaConfig:TopicProduce:JeeplatformInitialization");
                    await _producer.PublishAsync(TopicAddNewCustomer, JsonConvert.SerializeObject(obj));
                    lstusername.Add(username);
                    lstuserid.Add(userid);
                }

                return Ok(new { username = string.Join(",", lstusername), userid = string.Join(",", lstuserid) });
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        private string getSecretToken()
        {
            var secret = _config.GetValue<string>("Jwt:internal_secret");
            var projectName = _config.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }

        #endregion api gọi một lân
    }
}