using DPSinfra.Utils;
using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DatabaseManagement;
using JeeAccount.Services;
using JeeAccount.Services.DatabaseManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/databasemanagement")]
    [ApiController]
    public class DatabaseManagementController : ControllerBase
    {
        private IConfiguration _config;
        private IDatabaseManagementService _service;

        public DatabaseManagementController(IConfiguration configuration, IDatabaseManagementService databaseManagementService)
        {
            _config = configuration;
            _service = databaseManagementService;
        }

        [HttpPost("GetDSDB")]
        public BaseModel<object> GetListDSByCustomerIDAppCode([FromBody] DBTokenModel dBTokenModel)
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
    }
}