using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Services.StructureManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/departmentmanagement")]
    [ApiController]
    public class StructureManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IStructureManagementService structureManagementService;

        public StructureManagementController(IConfiguration configuration, IStructureManagementService structureManagementService)
        {
            _config = configuration;
            this.structureManagementService = structureManagementService;
        }

        [HttpGet("GetOrgStructure")]
        public async Task<object> GetOrgStructure([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var depart = await structureManagementService.GetOrgStructure(query);
                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("Sysn_Structure")]
        public async Task<object> Sysn_Structure(long CustomerID)
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var depart = await structureManagementService.Sysn_Structure(CustomerID);
                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
    }
}