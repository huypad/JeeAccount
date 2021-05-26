using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Services;
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
        private readonly IConfiguration _config;
        private readonly IMailService mailService;

        public MailController(IConfiguration configuration, IMailService mailService)
        {
            _config = configuration;
            this.mailService = mailService;
        }

        [HttpGet("GetMailByCustomerID")]
        public BaseModel<object> GetMailByCustomerID()
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
    }
}