using JeeAccount.Classes;
using JeeAccount.Services.CustomerManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/appmanagement")]
    [ApiController]
    public class AppManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _internal_secret;
        private readonly string _connectionString;
        private ICustomerManagementService _customerService;


        public AppManagementController(IConfiguration configuration, ICustomerManagementService customerManagementService)
        {
            _config = configuration;
            _internal_secret = configuration["Jwt:internal_secret"];
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _customerService = customerManagementService;
        }

        [HttpGet("GetListApp/internal")]
        public IActionResult GetListAppInternal()
        {
            try
            {
                var isToken = Ulities.IsInternaltoken(HttpContext.Request.Headers, _config.GetValue<string>("Jwt:internal_secret"));
                if (isToken == false)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                var list = _customerService.GetListApp();
                if (list == null) return NotFound(MessageReturnHelper.KhongCoDuLieu("Danh sach"));
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }
    }
}
