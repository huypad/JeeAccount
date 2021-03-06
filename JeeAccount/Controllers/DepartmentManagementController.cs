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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DPSinfra.Logger;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using RestSharp;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [ApiController]
    [Route("api/accountdepartmentmanagement")]
    public class DepartmentManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IDepartmentManagementService _service;
        private readonly string _connectionString;
        private readonly string HOST_JEEHR_API;
        private readonly ILogger<DepartmentManagementController> _logger;

        public DepartmentManagementController(IConfiguration configuration, IDepartmentManagementService service, ILogger<DepartmentManagementController> logger)
        {
            _config = configuration;
            _service = service;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
            _logger = logger;
        }

        [HttpGet("GetListDepartment")]
        public async Task<object> GetListDepartment()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
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
                    var list = jeehrController.GetDSCoCauToChuc(token);
                    return list;
                }
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListDepartmentManagement")]
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
                    return BadRequest(MessageReturnHelper.DangNhap());
                }

                var obj = await _service.GetDSPhongBan(query, customData.JeeAccount.CustomerID, token);

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

        [HttpGet("GetListDepartmentManagement2")]
        public IActionResult GetListDepartmentManagement2()
        {
            try
            {

                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return BadRequest(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return BadRequest(MessageReturnHelper.DangNhap());
                }

                string host_api_jeehr = _config.GetValue<string>("Host:JeeHR_API");
                string url = $"{host_api_jeehr}/api/interaction/getCoCauToChuc";

                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);

                request.AddHeader("Authorization", token);
                var traceLog = new GeneralLog()
                {
                    name = "department",
                    data = "",
                    message = "send response"
                };
                _logger.LogTrace(JsonConvert.SerializeObject(traceLog));
                IRestResponse response = client.Execute(request);
                var traceLog2 = new GeneralLog()
                {
                    name = "department",
                    data = "",
                    message = "send ok"
                };


                var res = response.Content;

                return Ok(res);
                
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
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
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

        [HttpGet("GetDepartment/{rowid}")]
        public IActionResult GetDepartment(int rowid)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                if (GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID)) return BadRequest(MessageReturnHelper.Custom("Api này không dùng cho khách hàng có sử dụng JeeHR"));
                return Ok(_service.GetDepartment(rowid, customData.JeeAccount.CustomerID));
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

        [HttpPost("UpdateDepartment")]
        public IActionResult UpdateDepartment(DepartmentModel depart)
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
                _service.UpdateDepartment(depart, customData.JeeAccount.CustomerID, commonInfo.Username, isJeeHR);
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

        [HttpPost("UpdateDepartmentManager")]
        public IActionResult UpdateDepartmentManager(UpdateDepartmentManagerModel depart)
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
                _service.UpdateDepartmentManager(Username, customData.JeeAccount.CustomerID, depart.DepartmentManager, depart.RowID);
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
        public IActionResult changeTinhTrang(DepChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                _service.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.RowID, acc.Note, customData.JeeAccount.UserID);
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
                _service.DeleteDepartmentManager(Username, customData.JeeAccount.CustomerID, rowid);
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