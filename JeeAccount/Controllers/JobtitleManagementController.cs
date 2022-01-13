using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.JobtitleManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Services.JobtitleManagementService;
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
    [Route("api/jobtitlemanagement")]
    [ApiController]
    public class JobtitleManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IJobtitleManagementService _service;
        private readonly string _connectionString;
        private readonly string HOST_JEEHR_API;

        public JobtitleManagementController(IConfiguration configuration, IJobtitleManagementService service)
        {
            _config = configuration;
            _service = service;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
        }

        [HttpGet("GetListJobtitleManagement")]
        public async Task<IActionResult> GetListDepartmentManagement([FromQuery] QueryParams query)
        {
            try
            {
                query = query == null ? new QueryParams() : query;

                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return BadRequest(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }

                var obj = await _service.GetDSChucvu(query, customData.JeeAccount.CustomerID, token);

                return Ok(obj);
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

        [HttpPost("CreateJobtitle")]
        public object CreateJobtitle(JobtitleModel depart)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }
                var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, customData.JeeAccount.UserID);
                var username = commonInfo.Username;
                _service.CreateJobtitle(depart, customData.JeeAccount.CustomerID, username);

                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetJobtitle/{rowid}")]
        public IActionResult GetJobtitle(int rowid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                if (GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID)) return BadRequest(MessageReturnHelper.Custom("Api này không dùng cho khách hàng có sử dụng JeeHR"));
                return Ok(_service.GetJobtitle(rowid, customData.JeeAccount.CustomerID));
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

        [HttpPost("UpdateJobtitle")]
        public IActionResult UpdateJobtitle(JobtitleModel jobtitle)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, customData.JeeAccount.UserID);
                var isJeeHR = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                _service.UpdateJobtitle(jobtitle, customData.JeeAccount.CustomerID, commonInfo.Username, isJeeHR);
                return Ok(MessageReturnHelper.ThanhCong());
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

        [HttpPost("ChangeTinhTrang")]
        public async Task<object> changeTinhTrang(JobChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
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

        [HttpDelete("Delete/{rowid}")]
        public IActionResult Delete(int rowid)
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
                _service.DeleteJobtile(Username, customData.JeeAccount.CustomerID, rowid);
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
    }
}