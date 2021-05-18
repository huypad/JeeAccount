using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common = JeeAccount.Classes.Common;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/departmentmanagement")]
    [ApiController]
    public class DepartmentManagementController: ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly StructureManagementService structureManagementService;

        public DepartmentManagementController(IConfiguration configuration)
        {
            _config = configuration;
            structureManagementService = new StructureManagementService(_config.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("GetOrgStructure")]
        public async Task<BaseModel<object>> GetOrgStructure([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
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

    }
}
