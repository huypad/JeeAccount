using DPSinfra.Kafka;
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

        public AccountManagementController(IConfiguration configuration, IAccountManagementService accountManagementService, IProducer producer)
        {
            _config = configuration;
            _service = accountManagementService;
            _JeeCustomerSecrectkey = configuration.GetValue<string>("AppConfig:Secrectkey:JeeCustomer");
            _internal_secret = configuration["Jwt:internal_secret"];
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _producer = producer;
        }

        #region giao diện JeeAccount  Management/AccountManagement

        [Route("GetListAccountManagement")]
        [HttpGet]
        public async Task<ActionResult> GetListAccountManagement([FromQuery] QueryParams query)
        {
            try
            {
                query = query == null ? new QueryParams() : query;

                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
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
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
            }
            catch (JeeHRException error)
            {
                return BadRequest(MessageReturnHelper.ExceptionJeeHR(error));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [Route("GetListAccountManagement/internal/{customerid}")]
        [HttpGet]
        public async Task<ActionResult> GetListAccountManagementInternal([FromQuery] QueryParams query, long customerid)
        {
            try
            {
                query = query == null ? new QueryParams() : query;
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                PageModel pageModel = new PageModel();
                var lst = await _service.GetListAccountManagement(query, customerid).ConfigureAwait(false);
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
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
            }
            catch (JeeHRException error)
            {
                return BadRequest(MessageReturnHelper.ExceptionJeeHR(error));
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, 0, img.Username);
                var userid = commonInfo.UserID;
                await _service.UpdateAvatarWithChangeUrlAvatar(Convert.ToInt64(userid), img.Username, customData.JeeAccount.CustomerID, img.imgFile).ConfigureAwait(false);
                return Ok("Cập nhật thành công");
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var usernames = await _service.GetListAccUsernameModel(customData.JeeAccount.CustomerID);
                if (usernames is null)
                    return JsonResultCommon.KhongTonTai("Danh sách thông tin");
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
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var usernames = await _service.GetListAccUsernameModel(customerID);
                if (usernames is null)
                    return BadRequest(MessageReturnHelper.KhongTonTai("Danh sách thông tin"));
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
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var list = await _service.GetListJustCustormerID();
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
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var list = await _service.GetListJustCustormerIDAppCode(appCode);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var customerid = await _service.GetCustormerIDByUsernameAsync(username);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var customerid = await _service.GetCustormerID(model);
                if (customerid == 0)
                    return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
                return Ok(customerid);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }

                var infoAdminDTOs = await _service.GetInfoAdminAccountByCustomerIDAsync(customData.JeeAccount.CustomerID);
                if (infoAdminDTOs is null)
                    return JsonResultCommon.KhongTonTai("Danh sách admin");
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var infoUser = await _service.GetInfoByCustomerIDAsync(customData.JeeAccount.CustomerID);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);
                var infoUser = await _service.GetInfoByUsernameAsync(commonInfo.Username, customData.JeeAccount.CustomerID);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var admins = await _service.GetListAdminsByCustomerIDAsync(customData.JeeAccount.CustomerID);
                if (admins.Count() == 0) return JsonResultCommon.KhongTonTai("Danh sách admin");
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var appList = await _service.GetListAppByCustomerIDAsync(customData.JeeAccount.CustomerID);

                return JsonResultCommon.ThanhCong(appList);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListAppByUserID")]
        public async Task<object> GetListAppByUserID(long userid = 0)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                if (userid == 0) userid = customData.JeeAccount.UserID;

                var appList = await GeneralReponsitory.GetListAppByUserIDAsync(_connectionString, userid, customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(appList);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetEditListAppByUserIDByListCustomerId")]
        public async Task<object> GetEditListAppByUserIDByListCustomerId(long userid = 0)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                if (userid == 0) userid = customData.JeeAccount.UserID;

                var appList = await _service.GetEditListAppByUserIDByListCustomerId(userid, customData.JeeAccount.CustomerID);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var accUser = await _service.GetListUsernameByAppcodeAsync(customData.JeeAccount.CustomerID, appcode);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var usernameQuanLy = "";
                if (!string.IsNullOrEmpty(model.Userid))
                {
                    usernameQuanLy = await _service.GetDirectManagerByUserID(model.Userid);
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    usernameQuanLy = await _service.GetDirectManagerByUsername(model.Username);
                }
                var infoUser = await _service.GetInfoByUsernameAsync(usernameQuanLy, customData.JeeAccount.CustomerID);
                if (infoUser.Name != null) return Ok(infoUser);
                return BadRequest(MessageReturnHelper.Custom("quản lý trực tiếp không tồn tại hoặc userid và username không hợp lệ"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("ListUsernameAndUserid")]
        public async Task<IActionResult> ListUsernameAndUserid()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _service.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
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

        [HttpPost("UppdatePersonalInfo")]
        public async Task<object> UppdatePersonalInfo(PersonalInfoCustomData personalInfoCustom, long UserID)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
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

        [HttpGet("ListUsername")]
        public async Task<IActionResult> ListUsername()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _service.GetListJustUsernameByCustormerID(customData.JeeAccount.CustomerID);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _service.GetListJustUserIDByCustormerID(customData.JeeAccount.CustomerID);
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
        public IActionResult UsernameOrUserID(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);

                if (!string.IsNullOrEmpty(model.Username))
                {
                    return Ok(commonInfo.UserID);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    return Ok(commonInfo.Username);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstUsername = await _service.GetListDirectManager(customData.JeeAccount.CustomerID);
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);
                var lst = await _service.ListNhanVienCapDuoiDirectManagerByDirectManager(commonInfo.Username, customData.JeeAccount.CustomerID);
                if (lst is not null) return Ok(lst);
                return BadRequest(MessageReturnHelper.KhongTonTai("UserId hoặc Username"));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #endregion api public

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
                var commonInfo = GeneralReponsitory.GetCommonInfoByInputApiModel(_connectionString, model);

                var jeehrController = new JeeHRController(HOST_JEEHR_API);
                var data = await jeehrController.GetDSNhanVienTheoQuanLyTrucTiep(access_token, commonInfo.StaffID);
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
        public async Task<IActionResult> ChangeTinhTrang(AccChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                await _service.ChangeTinhTrang(token, customData.JeeAccount.CustomerID, acc.Username, acc.Note, customData.JeeAccount.UserID);
                return Ok(MessageReturnHelper.ThanhCong());
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
            }
            catch (JeeHRException error)
            {
                return BadRequest(MessageReturnHelper.ExceptionJeeHR(error));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("createAccount")]
        public async Task<IActionResult> CreateAccount(AccountManagementModel account)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var username = Ulities.GetUsernameByHeader(HttpContext.Request.Headers);
                var isjeeHR = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (string.IsNullOrEmpty(username)) return Unauthorized(MessageReturnHelper.DangNhap());
                // for JeeOffice
                account.cocauid = 1;
                account.chucvuid = 33;
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                await _service.CreateAccount(isjeeHR, token, customData.JeeAccount.CustomerID, username, account);

                return Ok(MessageReturnHelper.ThanhCong("Tạo tài khoản"));
            }
            catch (TrungDuLieuExceoption ex)
            {
                return BadRequest(MessageReturnHelper.Trung(ex));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("UpdateAccount")]
        public async Task<IActionResult> UpdateAccount(AccountManagementModel account)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var username = Ulities.GetUsernameByHeader(HttpContext.Request.Headers);
                if (string.IsNullOrEmpty(username)) return Unauthorized(MessageReturnHelper.DangNhap());
                var isjeeHR = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (isjeeHR)
                {
                    if (account.StaffID > 0)
                    {
                        var common = GeneralReponsitory.GetCommonInfo(_connectionString, 0, account.Username);
                        var common2 = GeneralReponsitory.GetCommonInfo(_connectionString, 0, "", account.StaffID);

                        if (common.UserID != common2.UserID && common2.UserID != 0)
                        {
                            return BadRequest(MessageReturnHelper.Trung("ID Nhân viên này đã được tài khoản khác, vui lòng chọn đúng tài khoản"));
                        }
                    }
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                await _service.UpdateAccount(isjeeHR, token, customData.JeeAccount.CustomerID, username, account);

                return Ok(MessageReturnHelper.ThanhCong("Cập tài khoản"));
            }
            catch (TrungDuLieuExceoption ex)
            {
                return BadRequest(MessageReturnHelper.Trung(ex));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("UpdateDirectManager")]
        public object UpdateDirectManager(AccDirectManagerModel acc)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var update = _service.UpdateDirectManager(acc.Username, acc.DirectManager, customData.JeeAccount.CustomerID);
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

        #region api for customer

        [HttpGet("GetListCustomerAppByCustomerIDFromCustomer")]
        public async Task<IActionResult> GetListCustomerAppByCustomerIDFromCustomer(long CustomerID)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var create = await _service.GetListCustomerAppByCustomerIDFromAccountAsync(CustomerID);
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

        [HttpPost("GetInternalToken")]
        public IActionResult GetInternalToken()
        {
            try
            {
                return Ok(GeneralService.GetInternalToken(_config));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetDSJeeHR")]
        public async Task<IActionResult> GetDSJeeHR()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var jeeHRController = new JeeHRController(HOST_JEEHR_API);
                var lst = await jeeHRController.GetDSNhanVien(token);
                var listStaffId = GeneralReponsitory.GetLstStaffIDByCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (lst.status == 0) return BadRequest(MessageReturnHelper.ErrorJeeHR(lst.error));
                var lstData = lst.data.Where(item => !listStaffId.Contains(item.IDNV));
                return Ok(lstData);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetDSJeeHRToUpdate")]
        public async Task<IActionResult> GetDSJeeHRToUpdate()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var jeeHRController = new JeeHRController(HOST_JEEHR_API);
                var lst = await jeeHRController.GetDSNhanVien(token);
                var listStaffId = GeneralReponsitory.GetLstStaffIDByCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (lst.status == 0) return BadRequest(MessageReturnHelper.ErrorJeeHR(lst.error));
                var lstData = lst.data.Where(item => listStaffId.Contains(item.IDNV));
                return Ok(lstData);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("GetCommonAccount/{userid}")]
        public IActionResult GetCommonAccount(long userid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }

                var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid);
                return Ok(common);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpDelete("Delete/{userid}")]
        public async Task<IActionResult> Delete(long userid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var Username = Ulities.GetUsernameByHeader(HttpContext.Request.Headers);
                if (Username is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                await _service.DeleteAccountManagement(token, Username, customData.JeeAccount.UserID, customData.JeeAccount.CustomerID, userid);
                return Ok(MessageReturnHelper.ThanhCong());
            }
            catch (KhongDuocXoaException ex)
            {
                return BadRequest(MessageReturnHelper.KhongDuocXoaException(ex));
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("CheckAdminHeThong/{userid}")]
        public IActionResult CheckAdminHeThong(long userid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }

                var isAdmin = GeneralReponsitory.IsAdminHeThong(_connectionString, userid);

                return Ok(new { IsAdmin = isAdmin });
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("CheckAdminApp/{userid}/{appID}")]
        public IActionResult CheckAdminApp(long userid, int appID)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }

                var isAdmin = GeneralReponsitory.IsAdminApp(_connectionString, userid, appID);

                return Ok(new { IsAdmin = isAdmin });
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }
    }
}