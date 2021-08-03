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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly IAccountManagementService _service;
        private readonly string _JeeCustomerSecrectkey;
        private readonly string _internal_secret;
        private readonly string HOST_JEEHR_API;
        private readonly string _connectionString;
        private readonly IProducer _producer;
        private readonly IAccountManagementReponsitory _reponsitory;

        public AccountManagementController(IConfiguration configuration, IAccountManagementService accountManagementService, IProducer producer, IAccountManagementReponsitory reponsitory)
        {
            _config = configuration;
            _service = accountManagementService;
            _JeeCustomerSecrectkey = configuration.GetValue<string>("AppConfig:Secrectkey:JeeCustomer");
            _internal_secret = configuration["Jwt:internal_secret"];
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _producer = producer;
            _reponsitory = reponsitory;
        }

        #region giao diện JeeAccount  Management/AccountManagement

        [Route("GetListAccountManagement")]
        [HttpGet]
        public async Task<ActionResult> GetListAccountManagement([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                BaseModel<object> model = new BaseModel<object>();
                PageModel pageModel = new PageModel();
                var lst = await _service.GetListAccountManagement(query, customData.JeeAccount.CustomerID).ConfigureAwait(false);
                int total = lst.Count();
                if (total == 0) return BadRequest(MessageReturnHelper.KhongCoDuLieu("danh sách tài khoản"));
                pageModel.TotalCount = lst.Count();
                pageModel.AllPage = (int)Math.Ceiling(total / (decimal)query.record);
                pageModel.Size = query.record;
                pageModel.Page = query.page;
                if (query.more)
                {
                    query.page = 1;
                    query.record = pageModel.TotalCount;
                }
                var list = lst.Skip((query.page - 1) * query.record).Take(query.record);

                return Ok(MessageReturnHelper.Ok(list, pageModel));
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
                await _service.UpdateAvatarWithChangeUrlAvatar(Convert.ToInt64(userid), img.Username, customData.JeeAccount.CustomerID, img.imgFile);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #endregion giao diện JeeAccount  Management/AccountManagement

        #region api public

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
                var checkusedjeehr = GeneralService.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (checkusedjeehr)
                {
                    var usernames = await _reponsitory.GetListAccUsernameModelIsJeeHRAsync(customData.JeeAccount.CustomerID);
                    if (usernames is null)
                        return JsonResultCommon.KhongTonTai("Danh sách thông tin");
                    return JsonResultCommon.ThanhCong(usernames);
                }
                else
                {
                    var usernames = await _reponsitory.GetListAccUsernameModelDefaultAsync(customData.JeeAccount.CustomerID);
                    if (usernames is null)
                        return JsonResultCommon.KhongTonTai("Danh sách thông tin");
                    return JsonResultCommon.ThanhCong(usernames);
                }
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

                var checkusedjeehr = GeneralService.IsUsedJeeHRCustomerid(_connectionString, customerID);
                if (checkusedjeehr)
                {
                    var usernames = await _reponsitory.GetListAccUsernameModelIsJeeHRAsync(customerID);
                    if (usernames is null)
                        return BadRequest(MessageReturnHelper.KhongTonTai("Danh sách thông tin"));
                    return Ok(usernames);
                }
                else
                {
                    var usernames = await _reponsitory.GetListAccUsernameModelDefaultAsync(customerID);
                    if (usernames is null)
                        return BadRequest(MessageReturnHelper.KhongTonTai("Danh sách thông tin"));
                    return Ok(usernames);
                }
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
                var list = await _reponsitory.GetListJustCustormerID();
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
                var list = await _reponsitory.GetListJustCustormerIDAppCode(appCode);
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
                var customerid = await _reponsitory.GetCustormerIDByUsernameAsync(username);
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
                    var customerid = await _reponsitory.GetCustormerIDByUsernameAsync(model.Username);
                    if (customerid != 0)
                        return Ok(customerid);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var customerid = await _reponsitory.GetCustormerIDByUserIDAsync(long.Parse(model.Userid));
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

                var checkusedjeehr = GeneralService.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (checkusedjeehr)
                {
                    var infoAdminDTOs = await _reponsitory.GetInfoAdminAccountByCustomerIDIsJeeHRAsync(customData.JeeAccount.CustomerID);
                    if (infoAdminDTOs is null)
                        return JsonResultCommon.KhongTonTai("Danh sách admin");
                    return JsonResultCommon.ThanhCong(infoAdminDTOs);
                }
                else
                {
                    var infoAdminDTOs = await _reponsitory.GetInfoAdminAccountByCustomerIDDefaultAsync(customData.JeeAccount.CustomerID);
                    if (infoAdminDTOs is null)
                        return JsonResultCommon.KhongTonTai("Danh sách admin");
                    return JsonResultCommon.ThanhCong(infoAdminDTOs);
                }
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
                var infoUser = await _reponsitory.GetInfoByCustomerIDAsync(customData.JeeAccount.CustomerID);
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

                var infoUser = await _service.GetInfoByUsernameAsync(username, customData.JeeAccount.CustomerID);
                if (infoUser is null)
                    return JsonResultCommon.KhongTonTai("Username");
                return JsonResultCommon.ThanhCong(infoUser);
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
                var username = "";
                if (!string.IsNullOrEmpty(model.Username))
                {
                    username = model.Username;
                }
                if (!string.IsNullOrEmpty(model.Userid))
                {
                    username = GeneralService.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                }
                var infoUser = await _service.GetInfoByUsernameAsync(username, customData.JeeAccount.CustomerID);
                if (infoUser is not null)
                    return Ok(infoUser);
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
                var admins = await _reponsitory.GetListAdminsByCustomerIDAsync(customData.JeeAccount.CustomerID);
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
                var appList = await _reponsitory.GetListAppByCustomerIDAsync(customData.JeeAccount.CustomerID);

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
                var appList = await _reponsitory.GetListAppByUserIDAsync(customData.JeeAccount.UserID, customData.JeeAccount.CustomerID);
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
                var accUser = await _reponsitory.GetListUsernameByAppcodeAsync(customData.JeeAccount.CustomerID, appcode);
                return JsonResultCommon.ThanhCong(accUser);
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
                    usernameQuanLy = await _reponsitory.GetDirectManagerByUserID(model.Userid);
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    usernameQuanLy = await _reponsitory.GetDirectManagerByUsername(model.Username);
                }
                var infoUser = await _service.GetInfoByUsernameAsync(usernameQuanLy, customData.JeeAccount.CustomerID);
                if (infoUser is not null) return Ok(infoUser);
                return BadRequest(MessageReturnHelper.Custom("quản lý trực tiếp không tồn tại hoặc userid và username không hợp lệ"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

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
                var update = await _service.UpdatePersonalInfoCustomData(token, personalInfoCustom, UserID, customData.JeeAccount.CustomerID);
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
                var update = await _service.UppdateCustomData(token, objCustomData, customData.JeeAccount.CustomerID);
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

                var lstUsername = await _reponsitory.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
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

                var lstUsername = await _reponsitory.GetListJustUsernameByCustormerID(customData.JeeAccount.CustomerID);
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

                var lstUsername = await _reponsitory.GetListJustUserIDByCustormerID(customData.JeeAccount.CustomerID);
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
                        var result = await _reponsitory.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
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
                    var result = _reponsitory.GetUserIdByUsername(model.Username, customData.JeeAccount.CustomerID);
                    if (result != 0)
                        return Ok(result.ToString());
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var result = GeneralService.GetUsernameByUserID(_connectionString, model.Userid);
                    if (!string.IsNullOrEmpty(result.ToString()))
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

                var lstUsername = await _reponsitory.GetListDirectManager(customData.JeeAccount.CustomerID);
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
                var username = "";
                if (!string.IsNullOrEmpty(model.Username))
                {
                    username = model.Username;
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    username = GeneralService.GetUsernameByUserID(_connectionString, model.Userid).ToString();
                }
                var lst = await _service.ListNhanVienCapDuoiDirectManagerByDirectManager(username, customData.JeeAccount.CustomerID);
                if (lst is not null) return Ok(lst);
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
                    var res = TranferDataHelper.ListNhanVienDuocQuanLyTrucTiep_FromNhanVienDuocQuanLyTrucTiep(data.data);
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

        [HttpPost("ChangeTinhTrang")]
        public object ChangeTinhTrang(AccChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }

                ReturnSqlModel update = _reponsitory.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.Username, acc.Note, customData.JeeAccount.UserID);
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
                var create = await _service.CreateAccount(token, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID, account, apiUrl);
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

        [HttpPost("UpdateDirectManager")]
        public object UpdateDirectManager(AccDirectManagerModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var update = _reponsitory.UpdateDirectManager(acc.Username, acc.DirectManager, customData.JeeAccount.CustomerID);
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

        #endregion api public

        #region api for customer

        [HttpGet("GetListCustomerAppByCustomerIDFromCustomer")]
        public async Task<IActionResult> GetListCustomerAppByCustomerIDFromCustomer(long CustomerID)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var create = await _reponsitory.GetListCustomerAppByCustomerIDFromAccountAsync(CustomerID);
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

                var response = await _service.ResetPasswordRootCustomer(model);
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

                var lstCutomer = await _reponsitory.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    var lstUsername = await _reponsitory.GetListJustUsernameByCustormerID(customerid);

                    foreach (string username in lstUsername)
                    {
                        var model = new InputApiModel();
                        model.Username = username;
                        model.StaffID = null;
                        model.Userid = null;
                        var reponse = await _service.UpdateOneStaffIDByInputApiModel(model);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("updateAllStaffIDAll/internal")]
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

                var lstUsername = await _reponsitory.GetListJustUsernameByCustormerID(customerid);
                if (lstUsername is null) return NotFound(MessageReturnHelper.KhongTonTai("customerid"));
                foreach (string username in lstUsername)
                {
                    var model = new InputApiModel();
                    model.Username = username;
                    model.StaffID = null;
                    model.Userid = null;
                    var reponse = await _service.UpdateOneStaffIDByInputApiModel(model);
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
                var reponse = await _service.UpdateOneStaffIDByInputApiModel(model);
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
                var reponse = await _service.UpdateOneStaffIDByInputApiModel(model);
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

                var lstCutomer = await _reponsitory.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    if (customerid == 25)
                    {
                        var lstUsername = await _reponsitory.GetListJustUsernameByCustormerID(customerid);

                        foreach (string username in lstUsername)
                        {
                            if (username.Equals("vanluc"))
                            {
                                var model = new InputApiModel();
                                model.Username = username;
                                model.Userid = null;
                                var reponse = await _service.UpdateOneBgColorCustomData(model);
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

                var lstCutomer = await _reponsitory.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    if (customerid == customData.JeeAccount.CustomerID)
                    {
                        var lstUsername = await _reponsitory.GetListJustUsernameByCustormerID(customerid);
                        var lstAppCode = await _reponsitory.GetListAppByCustomerIDAsync(customerid);
                        var lstAppID = lstAppCode.Select(x => x.AppID).ToList();
                        foreach (string username in lstUsername)
                        {
                            var model = new InputApiModel();
                            model.Username = username;
                            model.Userid = null;
                            _service.UpdateAllAppCodesCustomData(model, lstAppID);
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
                var listApp = await _reponsitory.GetListAppByCustomerIDAsync(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listfail = "";
                if (dataJeehr.status == 1)
                {
                    var identity = new IdentityServerController();
                    foreach (var data in dataJeehr.data)
                    {
                        if (data.username is null) continue;

                        listExist += $"{data.username} ";
                        var personalInfoCustom = new PersonalInfoCustomData();
                        personalInfoCustom.Fullname = data.HoTen;
                        personalInfoCustom.Name = GeneralService.GetFirstname(data.HoTen);
                        personalInfoCustom.Avatar = data.avatar;
                        personalInfoCustom.BgColor = GeneralService.GetColorNameUser(GeneralService.GetFirstname(data.HoTen).Substring(0, 1));
                        personalInfoCustom.Jobtitle = data.TenChucVu;
                        personalInfoCustom.Structure = data.Structure;
                        personalInfoCustom.Birthday = data.NgaySinh;
                        personalInfoCustom.Email = data.Email;

                        var userid = long.Parse(GeneralService.GetUserIDByUsername(_connectionString, data.username).ToString());

                        var res = await _service.UpdatePersonalInfoCustomData(access_token, personalInfoCustom, userid, 25);
                        if (res.statusCode != 0)
                        {
                            listfail += $"{data.username}, ";
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

        [HttpGet("SaveAgianPersonalInfoJeehr")]
        public async Task<IActionResult> SaveAgianPersonalInfoJeehr()
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
                var customerID = 25;
                var lstok = "";
                var lstfail = "";
                if (dataJeehr.status == 1)
                {
                    using (DpsConnection cnn = new DpsConnection(_connectionString))
                    {
                        var lst = GeneralService.GetListUserNameDTOByCustomeridCnn(cnn, 25);

                        foreach (var nv in dataJeehr.data)
                        {
                            Hashtable val = new Hashtable();
                            SqlConditions Conds = new SqlConditions();
                            Conds.Add("CustomerID", customerID);
                            Conds.Add("Username", nv.username);
                            val.Add("Jobtitle", nv.TenChucVu);
                            val.Add("JobtitleID", Convert.ToInt32(nv.jobtitleid));
                            val.Add("Department", nv.Structure);
                            val.Add("DepartmentID", nv.structureid);
                            val.Add("StaffID", nv.IDNV);
                            var item = lst.Find(data => data.StaffID == nv.managerid);
                            if (item is not null)
                            {
                                val.Add("DirectManager", item.Username);
                            }
                            int x = cnn.Update(val, Conds, "AccountList");
                            if (x <= 0)
                            {
                                lstfail += " ," + nv.username;
                            }
                            else
                            {
                                lstok += " ," + nv.username;
                            }
                        }
                    }
                }
                return Ok(new { lstfail = lstfail, lstok = lstok });
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
                var listApp = await _reponsitory.GetListAppByCustomerIDAsync(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listfail = "";
                var lstIdCustomer = await _reponsitory.GetListJustUserIDByCustormerID(customerID);
                var ListID = new List<long>();
                var ListIDNotExist = new List<long>();
                if (dataJeehr.status == 1)
                {
                    var identity = new IdentityServerController();
                    foreach (var data in dataJeehr.data)
                    {
                        if (data.username is null) continue;
                        var userid = long.Parse(GeneralService.GetUserIDByUsername(_connectionString, data.username).ToString());
                        _reponsitory.SaveStaffID(userid, Convert.ToInt64(data.IDNV));
                        listExist += ", " + data.username;
                    }
                }

                return Ok(new { listExist = listExist });
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
                var customerID = customData.JeeAccount.CustomerID;
                var listApp = await _reponsitory.GetListAppByCustomerIDAsync(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listfail = "";
                var lstIdCustomer = await _reponsitory.GetListJustUserIDByCustormerID(customerID);
                var ListID = new List<long>();
                var ListIDNotExist = new List<long>();
                var identity = new IdentityServerController();
                foreach (var userid in lstIdCustomer)
                {
                    InputApiModel model = new InputApiModel();
                    model.Userid = userid.ToString();
                    var value = await _service.GetJeeAccountCustomDataAsync(model);
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
                var customerID = customData.JeeAccount.CustomerID;
                var listApp = await _reponsitory.GetListAppByCustomerIDAsync(customerID);
                var listId = listApp.Select(item => item.AppID).ToList();
                var listAppCode = listApp.Select(item => item.AppCode).ToList();
                var listfail = "";
                var lstIdCustomer = await _reponsitory.GetListJustUserIDByCustormerID(customerID);
                var ListID = new List<long>();
                var ListIDNotExist = new List<long>();
                var identity = new IdentityServerController();
                var lstusername = new List<string>();
                var lstuserid = new List<long>();

                var lstStringApp = new List<string>();

                foreach (var userid in lstIdCustomer)
                {
                    InputApiModel model = new InputApiModel();
                    model.Userid = userid.ToString();
                    var value = await _service.GetJeeAccountCustomDataAsync(model);
                    var Obj = new ObjCustomData();
                    Obj.userId = userid;
                    Obj.updateField = "jee-account";
                    Obj.fieldValue = value;
                    var username = GeneralService.GetUsernameByUserID(_connectionString, userid.ToString()).ToString();

                    if (username == " dps.vanluc" || username == "dps.vanluc")
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