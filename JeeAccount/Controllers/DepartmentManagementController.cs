using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using JeeAccount.Reponsitories;
using JeeAccount.Services.DepartmentManagement;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/accountdepartmentmanagement")]
    [ApiController]
    public class DepartmentManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IDepartmentManagementService _service;
        private readonly string _connectionString;
        private readonly string HOST_JEEHR_API;

        public DepartmentManagementController(IConfiguration configuration, IDepartmentManagementService service)
        {
            _config = configuration;
            _service = service;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
        }

        [HttpGet("GetListDepartment")]
        public async Task<object> GetListDepartment()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin tài khoản");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return JsonResultCommon.DangNhap();
                }
                var checkUsedJeeHr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (!checkUsedJeeHr)
                {
                    var depart = await _service.GetListDepartmentDefaultAsync(customData.JeeAccount.CustomerID);
                    return JsonResultCommon.ThanhCong(depart);
                }
                else
                {
                    var jeehrController = new JeeHRController(HOST_JEEHR_API);
                    var list = await jeehrController.GetDSCoCauToChuc(token);
                    return JsonResultCommon.ThanhCong(list);
                }
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetDSPhongban")]
        public async Task<IActionResult> GetDSPhongban(bool donotcallapijeehr = false)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Ok(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Ok(MessageReturnHelper.DangNhap());
                }
                var query = new QueryParams();
                if (donotcallapijeehr)
                {
                    query.donotcallapijeehr = donotcallapijeehr;
                }
                query.more = true;
                var lst = await _service.GetDSPhongBan(query, customData.JeeAccount.CustomerID, token).ConfigureAwait(false);
                return Ok(lst);
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieu(ex.Message));
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

        [HttpGet("Get_DSPhongban")]
        public async Task<IActionResult> Get_DSPhongban(bool donotcallapijeehr = false)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Ok(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Ok(MessageReturnHelper.DangNhap());
                }
                var checkUsedJeeHr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (!checkUsedJeeHr)
                {
                    var depart = await _service.GetListDepartmentDefaultAsync(customData.JeeAccount.CustomerID).ConfigureAwait(false);
                    var lst = TranferDataHelper.LstJeeHRCoCauToChucModelFromDBFromLstDepartmentDTO(depart.ToList());
                    return Ok(lst);
                }
                else
                {
                    if (!donotcallapijeehr)
                    {
                        var jeehrController = new JeeHRController(HOST_JEEHR_API);
                        var list = await jeehrController.GetDSCoCauToChuc(token);
                        if (list.status == 1)
                        {
                            var lst = TranferDataHelper.LstJeeHRCoCauToChucModelFromDBFromLstJeeHRCoCauToChuc(list.data);
                            return Ok(lst);
                        }
                        else
                        {
                            var listjeehr = await _service.GetListDepartmentIsJeeHRAsync(customData.JeeAccount.CustomerID).ConfigureAwait(false);
                            return Ok(listjeehr);
                        }
                    }
                    else
                    {
                        var listjeehr = await _service.GetListDepartmentIsJeeHRAsync(customData.JeeAccount.CustomerID).ConfigureAwait(false);
                        return Ok(listjeehr);
                    }
                }
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieu(ex.Message));
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

        [HttpGet("GetListDepartmentManagement")]
        public async Task<IActionResult> GetListDepartmentManagement([FromQuery] QueryParams query)
        {
            try
            {
                query = query == null ? new QueryParams() : query;

                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Ok(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Ok(MessageReturnHelper.DangNhap());
                }

                var obj = await _service.GetDSPhongBan(query, customData.JeeAccount.CustomerID, token, true).ConfigureAwait(false);

                return Ok(obj);
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieu(ex.Message));
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

        [HttpPost("CreateDepartment")]
        public object CreateDepartment(DepartmentModel depart)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, customData.JeeAccount.UserID);
                _service.CreateDepartment(depart, customData.JeeAccount.CustomerID, commonInfo.Username);

                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("ChangeTinhTrang")]
        public async Task<object> changeTinhTrang(DepChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return await Task.FromResult(JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData"));
                }

                ReturnSqlModel update = _service.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.RowID, acc.Note, customData.JeeAccount.UserID);
                if (!update.Susscess)
                {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST))
                    {
                        return await Task.FromResult(JsonResultCommon.KhongTonTai("tài khoản"));
                    }
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION))
                    {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return Task.FromResult(JsonResultCommon.ThatBai(update.ErrorMessgage));
                    }
                }
                return await Task.FromResult(JsonResultCommon.ThanhCong(update));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(JsonResultCommon.Exception(ex));
            }
        }
    }
}