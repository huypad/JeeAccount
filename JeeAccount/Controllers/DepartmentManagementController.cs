using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Services.DepartmentManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/accountdepartmentmanagement")]
    [ApiController]
    public class DepartmentManagementController : ControllerBase
    {
        private IConfiguration _config;
        private IDepartmentManagementService departmentManagementService;

        public DepartmentManagementController(IConfiguration configuration, IDepartmentManagementService departmentManagementService)
        {
            _config = configuration;
            this.departmentManagementService = departmentManagementService;
        }

        [HttpGet("GetListDepartment")]
        public async Task<BaseModel<object>> GetListDepartment([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var depart = await departmentManagementService.GetListDepartment(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("CreateDepartment")]
        public BaseModel<object> CreateDepartment(DepartmentModel depart)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var create = departmentManagementService.CreateDepartment(depart, customData.JeeAccount.CustomerID, customData.JeeAccount.UserID);
                if (!create.Susscess)
                {
                    if (create.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION))
                    {
                        // TODO: bổ sung ghi log sau
                        string logMessage = create.ErrorMessgage;

                        return JsonResultCommon.ThatBai(create.ErrorMessgage);
                    }
                }
                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("ChangeTinhTrang")]
        public Task<BaseModel<object>> changeTinhTrang(DepChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Task.FromResult(JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData"));
                }

                ReturnSqlModel update = departmentManagementService.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.RowID, acc.Note, customData.JeeAccount.UserID);
                if (!update.Susscess)
                {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST))
                    {
                        return Task.FromResult(JsonResultCommon.KhongTonTai("tài khoản"));
                    }
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION))
                    {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return Task.FromResult(JsonResultCommon.ThatBai(update.ErrorMessgage));
                    }
                }
                return Task.FromResult(JsonResultCommon.ThanhCong(update));
            }
            catch (Exception ex)
            {
                return Task.FromResult(JsonResultCommon.Exception(ex));
            }
        }
    }
}