using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DatabaseManagement;
using JeeAccount.Services.DatabaseManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/databasemanagement")]
    [ApiController]
    public class DatabaseManagementController : ControllerBase
    {
        private IConfiguration _config;
        private IDatabaseManagementService _service;
        private readonly string _JeeCustomerSecrectkey;

        public DatabaseManagementController(IConfiguration configuration, IDatabaseManagementService databaseManagementService)
        {
            _config = configuration;
            _service = databaseManagementService;
            _JeeCustomerSecrectkey = configuration.GetValue<string>("AppConfig:Secrectkey:JeeCustomer");
        }

        [HttpPost("GetDSDB")]
        public object GetListDSByCustomerIDAppCode([FromBody] DBTokenModel dBTokenModel)
        {
            try
            {
                var prjectname = Ulities.GetProjectnameByHeader(HttpContext.Request.Headers, _config["Jwt:internal_secret"]);
                if (prjectname is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin Internal Token");
                }

                if (dBTokenModel is null)
                {
                    JsonResultCommon.BatBuoc("customerID hoặc appCode");
                }
                var data = _service.GetDBByCustomerIDAppCode(dBTokenModel.customerID, dBTokenModel.appCode);
                return JsonResultCommon.ThanhCong(data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        #region for customer call

        [HttpGet("GetDSDataList")]
        public object GetDSDataList()
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");
                var lst = _service.GetDBDatabaseDTO();
                return Ok(lst);
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

        #endregion for customer call
    }
}