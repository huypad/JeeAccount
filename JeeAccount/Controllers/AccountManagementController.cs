using DPSinfra.Kafka;
using DPSinfra.Utils;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Services;
using JeeAccount.Services.AccountManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/accountmanagement")]
    [ApiController]
    public class AccountManagementController : ControllerBase
    {
        private IConfiguration _config;
        private IAccountManagementService accountManagementService;
        private readonly string _JeeCustomerSecrectkey;
        private readonly string _internal_secret;

        public AccountManagementController(IConfiguration configuration, IAccountManagementService accountManagementService)
        {
            _config = configuration;
            this.accountManagementService = accountManagementService;
            _JeeCustomerSecrectkey = configuration.GetValue<string>("AppConfig:Secrectkey:JeeCustomer");
            _internal_secret = configuration["Jwt:internal_secret"];
        }

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

        [HttpGet("usernamesByCustermerID/internal/{customerID}")]
        public async Task<IActionResult> GetListUsernameByCustormerIDInternal(long customerID)
        {
            try
            {
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return NotFound(MessageReturnHelper.Unauthorized());
                }
                var usernames = await accountManagementService.GetListUsernameByCustormerID(customerID);
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
                    return NotFound(MessageReturnHelper.Unauthorized());
                }
                var list = await accountManagementService.GetListJustCustormerID();
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
                    return NotFound(MessageReturnHelper.Unauthorized());
                }
                var list = await accountManagementService.GetListJustCustormerIDAppCode(appCode);
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

        [HttpPost("GetCustormerID")]
        public async Task<ActionResult<long>> GetCustormerID(InputApiModel model)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var customerid = await accountManagementService.GetCustormerIDByUsername(model.Username);
                    if (customerid != 0)
                        return Ok(customerid);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var customerid = await accountManagementService.GetCustormerIDByUserID(long.Parse(model.Userid));
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
                var infoAdminDTOs = await accountManagementService.GetInfoAdminAccountByCustomerID(customData.JeeAccount.CustomerID);
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
                var infoUser = await accountManagementService.GetInfoByCustomerID(customData.JeeAccount.CustomerID);
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
                var infoUser = await accountManagementService.GetInfoByUsername(username);
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
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var infoUser = await accountManagementService.GetInfoByUsername(model.Username);
                    if (infoUser != null)
                        return Ok(infoUser);
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var infoUser = await accountManagementService.GetInfoByUserID(model.Userid);
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
                var admins = await accountManagementService.GetListAdminsByCustomerID(customData.JeeAccount.CustomerID);
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
                var appList = await accountManagementService.GetListAppByCustomerID(customData.JeeAccount.CustomerID);

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
                var appList = await accountManagementService.GetListAppByUserID(customData.JeeAccount.UserID);
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
        public async Task<object> GetListAccountManagement([FromQuery] QueryParams query)
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
        public async Task<object> changeTinhTrang(AccChangeTinhTrangModel acc)
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
                    usernameQuanLy = await accountManagementService.GetDirectManagerByUserID(model.Userid);
                    var result = await accountManagementService.GetInfoByUsername(usernameQuanLy);
                    if (result.Fullname != null)
                    {
                        return Ok(result);
                    }
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    usernameQuanLy = await accountManagementService.GetDirectManagerByUsername(model.Username);
                    var result = await accountManagementService.GetInfoByUsername(usernameQuanLy);
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

        #endregion api public bên ngoài

        [HttpPost("UpdateAvatar")]
        public object UpdateAvatar(PostImgModel img)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var UserID = accountManagementService.GetUserIdByUsername(img.Username, customData.JeeAccount.CustomerID);
                GeneralService.saveImgNhanVien(img.imgFile, UserID.ToString(), customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong();
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("UpdateAvatarWithChangeUrlAvatar")]
        public async Task<object> UpdateAvatarWithChangeUrlAvatar(PostImgModel img)
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

                var lstUsername = await accountManagementService.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
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

                var lstUsername = await accountManagementService.GetListJustUsernameByCustormerID(customData.JeeAccount.CustomerID);
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

                var lstUsername = await accountManagementService.GetListJustUserIDByCustormerID(customData.JeeAccount.CustomerID);
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
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Userid))
                {
                    {
                        var result = await accountManagementService.GetListJustUsernameAndUserIDByCustormerID(customData.JeeAccount.CustomerID);
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
                    var result = accountManagementService.GetUserIdByUsername(model.Username, customData.JeeAccount.CustomerID);
                    if (result != 0)
                        return Ok(result.ToString());
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var result = accountManagementService.GetUsernameByUserID(model.Userid, customData.JeeAccount.CustomerID);
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

                var lstUsername = await accountManagementService.GetListDirectManager(customData.JeeAccount.CustomerID);
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
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }

                if (!string.IsNullOrEmpty(model.Username))
                {
                    var result = await accountManagementService.ListNhanVienCapDuoiDirectManagerByDirectManager(model.Username);
                    if (result.Count() > 0)
                        return Ok(result);
                    else
                    {
                        BadRequest(MessageReturnHelper.KhongTonTai("Nhân viên cấp dưới"));
                    }
                }

                if (!string.IsNullOrEmpty(model.Userid))
                {
                    var username = accountManagementService.GetUsernameByUserID(model.Userid, customData.JeeAccount.CustomerID);
                    var result = await accountManagementService.ListNhanVienCapDuoiDirectManagerByDirectManager(username);

                    if (result.Count() > 0)
                        return Ok(result);
                    else
                    {
                        BadRequest(MessageReturnHelper.KhongTonTai("Nhân viên cấp dưới"));
                    }
                }

                return BadRequest(MessageReturnHelper.KhongTonTai("UserId hoặc Username"));
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

                var create = await accountManagementService.GetListCustomerAppByCustomerIDFromAccount(CustomerID);
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

                var response = await accountManagementService.ResetPasswordRootCustomer(model);
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
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var lstCutomer = await accountManagementService.GetListJustCustormerID();
                foreach (long customerid in lstCutomer)
                {
                    var lstUsername = await accountManagementService.GetListJustUsernameByCustormerID(customerid);

                    foreach (string username in lstUsername)
                    {
                        var model = new InputApiModel();
                        model.Username = username;
                        model.StaffID = null;
                        model.Userid = null;
                        var reponse = await accountManagementService.UpdateOneStaffIDByInputApiModel(model);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpGet("updateAllStaffID/{customerid}")]
        public async Task<IActionResult> UpdateAllStaffID(long customerid)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var messageError = "";
                var lstUsername = await accountManagementService.GetListJustUsernameByCustormerID(customerid);
                if (lstUsername is null) return NotFound(MessageReturnHelper.KhongTonTai("customerid"));
                foreach (string username in lstUsername)
                {
                    var model = new InputApiModel();
                    model.Username = username;
                    model.StaffID = null;
                    model.Userid = null;
                    var reponse = await accountManagementService.UpdateOneStaffIDByInputApiModel(model);
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

        [HttpGet("updateAllStaffID/internal/{customerid}")]
        public async Task<IActionResult> UpdateAllStaffIDInternal(long customerid)
        {
            try
            {
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return NotFound(MessageReturnHelper.Unauthorized());
                }

                var messageError = "";
                var lstUsername = await accountManagementService.GetListJustUsernameByCustormerID(customerid);
                if (lstUsername is null) return NotFound(MessageReturnHelper.KhongTonTai("customerid"));
                foreach (string username in lstUsername)
                {
                    var model = new InputApiModel();
                    model.Username = username;
                    model.StaffID = null;
                    model.Userid = null;
                    var reponse = await accountManagementService.UpdateOneStaffIDByInputApiModel(model);
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
                    return NotFound(MessageReturnHelper.CustomDataKhongTonTai());
                }
                if (model == null)
                {
                    return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
                }
                var reponse = await accountManagementService.UpdateOneStaffIDByInputApiModel(model);
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
                    return NotFound(MessageReturnHelper.Unauthorized());
                }
                if (model == null)
                {
                    return BadRequest(MessageReturnHelper.KhongTonTai("UserID hoặc username"));
                }
                var reponse = await accountManagementService.UpdateOneStaffIDByInputApiModel(model);
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

        [HttpGet("testgetvalue")]
        public async Task<IActionResult> testgetvalue()
        {
            try
            {
                var valueObj = new ObjCustomData
                {
                    userId = 123,
                    updateField = "jee-hr",
                    fieldValue = new
                    {
                        username = "windsora"
                    },
                };
                var value = Newtonsoft.Json.JsonConvert.SerializeObject(valueObj);
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjCustomData>(value);
                if (obj.updateField.ToString().Equals("jee-hr", StringComparison.OrdinalIgnoreCase))
                {
                    var x_json = Newtonsoft.Json.JsonConvert.SerializeObject(obj.fieldValue);

                    var x = Newtonsoft.Json.JsonConvert.DeserializeObject<FindStaffID>(x_json);

                    return Ok(x);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        public class FindStaffID
        {
            public long staffID { get; set; } = 0;
        }

        #endregion api for jeehr
    }
}