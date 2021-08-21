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
using JeeAccount.Services.PermissionManagementService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static JeeAccount.Models.Common.Panigator;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/permissionmanagement")]
    [ApiController]
    public class PermissionManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPermissionManagementService _service;
        private readonly string _connectionString;
        private readonly IAccountManagementService _accountService;

        public PermissionManagementController(IConfiguration configuration, IPermissionManagementService Service, IProducer producer, IAccountManagementService accountManagementService)
        {
            _config = configuration;
            _service = Service;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _accountService = accountManagementService;
        }

        [Route("GetListAccountAdminApp/{appid}")]
        [HttpGet]
        public async Task<ActionResult> GetListAccountAdminApp([FromQuery] QueryParams query, int appid)
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
                var lst = await _service.GetListAccountAdminAppNotAdminHeThong(query, customData.JeeAccount.CustomerID, appid).ConfigureAwait(false);

                int total = lst.Count();
                if (total == 0) return Ok(MessageReturnHelper.Ok(lst, pageModel));
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

        [Route("GetListAccountAdminHeThong")]
        [HttpGet]
        public async Task<ActionResult> GetListAccountAdminHeThong([FromQuery] QueryParams query)
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
                var lst = await _accountService.GetListAccountManagement(query, customData.JeeAccount.CustomerID, true).ConfigureAwait(false);
                int total = lst.Count();
                if (total == 0) return Ok(MessageReturnHelper.Ok(lst, pageModel));
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

        [HttpPost("CreateAdminHeThong")]
        public async Task<IActionResult> CreateAdminHeThong(NhanVienMatchip nv)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                await _service.CreateAdminHeThong(nv.UserId, nv.Username, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID);
                return Ok(MessageReturnHelper.ThanhCong("Tạo Admin hệ thống"));
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
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

        [HttpPost("CreateAdminApp/{appid}")]
        public async Task<IActionResult> CreateAdminApp(NhanVienMatchip nv, int appid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var lstAppid = new List<int>();
                lstAppid.Add(appid);
                await _service.CreateAdminApp(nv.UserId, nv.Username, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID, lstAppid);
                return Ok(MessageReturnHelper.ThanhCong("Tạo Admin hệ thống"));
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
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

        [HttpDelete("RemoveAdminHeThong/{userid}")]
        public async Task<IActionResult> RemoveAdminHeThong(long userid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid);
                await _service.RemoveAdminHeThong(userid, common.Username, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID);
                return Ok(MessageReturnHelper.ThanhCong());
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
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

        [HttpDelete("RemoveAdminApp/{appid}/{userid}")]
        public async Task<IActionResult> RemoveAdminApp(int appid, long userid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid);
                var Lst = new List<int>();
                Lst.Add(appid);
                await _service.RemoveAdminApp(userid, common.Username, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID, Lst);
                return Ok(MessageReturnHelper.ThanhCong());
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
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
    }
}