using DPSinfra.Utils;
using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Services.MailService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/mail")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private IConfiguration _config;
        private IMailService mailService;

        public MailController(IConfiguration configuration, IMailService mailService)
        {
            _config = configuration;
            this.mailService = mailService;
        }

        [HttpGet("GetMailByCustomerID")]
        public object GetMailByCustomerID()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData");
                }
                var mailModel = mailService.InitialData(customData.JeeAccount.CustomerID.ToString());
                if (mailModel is null)
                    return JsonResultCommon.KhongTonTai("CustomerID");
                return JsonResultCommon.ThanhCong(mailModel);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("GetMailByCustomerID/Internal")]
        public object GetMailByCustomerIDInternal(long customerID)
        {
            try
            {
                var prjectname = Ulities.GetProjectnameByHeader(HttpContext.Request.Headers, _config["Jwt:internal_secret"]);
                if (prjectname is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin Internal Token");
                }

                var mailModel = mailService.InitialData(customerID.ToString());
                if (mailModel is null)
                    return JsonResultCommon.KhongTonTai("CustomerID");
                return JsonResultCommon.ThanhCong(mailModel);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("gettoken")]
        public string GetSecretToken()
        {
            var secret = _config.GetValue<string>("Jwt:internal_secret");
            var projectName = _config.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }
    }
}