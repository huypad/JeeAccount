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
                var pHeader = HttpContext.Request.Headers;

                if (pHeader == null) return null;
                if (!pHeader.ContainsKey(HeaderNames.Authorization)) return null;

                IHeaderDictionary _d = pHeader;
                string _bearer_token, _user;
                _bearer_token = _d[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(_bearer_token) as JwtSecurityToken;
                string projectName = tokenS.Claims.Where(x => x.Type == "projectName").FirstOrDefault().Value;
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