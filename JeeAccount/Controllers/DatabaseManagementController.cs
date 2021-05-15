
using DPSinfra.Utils;
using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DatabaseManagement;
using JeeAccount.Services;
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
        private readonly IConfiguration _config;
        private readonly DatabaseManagementService _service;
        public DatabaseManagementController(IConfiguration configuration)
        {
            _config = configuration;
            _service = new DatabaseManagementService(_config.GetConnectionString("DefaultConnection"));
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

        [HttpGet("testtoken")]
        public async Task<object> createTesttokenAsync()
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                var secret = _config.GetValue<string>("Jwt:access_secret");
                var projectName = _config.GetValue<string>("KafkaConfig:ProjectName");
                var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
                var obj = new DBTokenModel
                {
                    customerID = 1,
                    appCode = "HR"
                };

                var stringContent = await Task.Run(() => Newtonsoft.Json.JsonConvert.SerializeObject(obj));
                var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                using (var client = new HttpClient(clientHandler))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                    var reponse = await client.PostAsync("https://localhost:5001/api/databasemanagement/GetDSDB", httpContent);
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = returnValue;
                    return res;
                }
            }

            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
