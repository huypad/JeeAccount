using DPSinfra.Kafka;
using DPSinfra.Utils;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Services.AccountManagementService;
using JeeAccount.Services.CustomerManagementService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static JeeAccount.Models.Common.Panigator;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/customermanagement")]
    [ApiController]
    public class CustomerManagementController : ControllerBase
    {
        private IConfiguration _config;
        private ICustomerManagementService _service;
        private readonly string _JeeCustomerSecrectkey;

        public CustomerManagementController(IConfiguration configuration, ICustomerManagementService customerManagementService)
        {
            _config = configuration;
            _service = customerManagementService;
            _JeeCustomerSecrectkey = configuration.GetValue<string>("AppConfig:Secrectkey:JeeCustomer");
        }

        [HttpPost("Get_DSCustomer")]
        public object GetListUsernameByCustormerID([FromBody] QueryRequestParams query)
        {
            try
            {
                query = query == null ? new QueryRequestParams() : query;
                object model = new object();
                PageModel pageModel = new PageModel();
                ErrorModel error = new ErrorModel();
                string orderByStr = "RowID asc";
                string whereStr = "";
                Dictionary<string, string> filter = new Dictionary<string, string>
                        {
                            { "thongtinkhachhang", "CompanyName"},
                            { "dienthoainguoidaidien", "Phone"},
                        };
                if (query.Sort != null)
                {
                    if (!string.IsNullOrEmpty(query.Sort.ColumnName) && filter.ContainsKey(query.Sort.ColumnName))
                    {
                        orderByStr = filter[query.Sort.ColumnName] + " " + (query.Sort.Direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc");
                    }
                }
                var customerlist = _service.GetListCustomer(whereStr, orderByStr);
                if (customerlist.Count() == 0)
                    return JsonResultCommon.ThatBai("");
                if (customerlist is null)
                    return JsonResultCommon.KhongTonTai("CustomerID");
                pageModel.TotalCount = customerlist.Count();
                pageModel.AllPage = (int)Math.Ceiling(customerlist.Count() / (decimal)query.Panigator.PageSize);
                pageModel.Size = query.Panigator.PageSize;
                pageModel.Page = query.Panigator.PageIndex;
                customerlist = customerlist.AsEnumerable().Skip((query.Panigator.PageIndex - 1) * query.Panigator.PageSize).Take(query.Panigator.PageSize);
                return JsonResultCommon.ThanhCong(customerlist, pageModel);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetListApp")]
        public object GetListApp()
        {
            try
            {
                var list = _service.GetListApp();
                if (list is null) return JsonResultCommon.KhongTonTai();
                return JsonResultCommon.ThanhCong(list);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetCompanyCode")]
        public IActionResult GetCompanyCode()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.CustomDataKhongTonTai());
                }

                var code = _service.CompanyCode(customData.JeeAccount.CustomerID);
                if (code is null) throw new KhongCoDuLieuException();
                return Ok(new { CompanyCode = code });
            }
            catch (KhongCoDuLieuException ex)
            {
                return BadRequest(MessageReturnHelper.KhongCoDuLieuException(ex));
            }
            catch (JeeHRException error)
            {
                return BadRequest(MessageReturnHelper.ExceptionJeeHR(error));
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        #region api for customer

        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerModel model)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var create = await _service.CreateCustomer(model, "jeecustomer");
                if (create.statusCode == 0)
                {
                    return Ok(create);
                }
                else
                {
                    return BadRequest(create.message);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateCustomerAppGiaHanModel")]
        public async Task<IActionResult> UpdateCustomerAppGiaHanModel([FromBody] CustomerAppGiaHanModel model)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var create = await _service.UpdateCustomerAppGiaHanModel(model);
                if (create.Susscess)
                {
                    return Ok(token);
                }
                else
                {
                    return BadRequest(create.ErrorMessgage);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateCustomerAppAddNumberStaff")]
        public async Task<IActionResult> UpdateCustomerAppAddNumberStaff(CustomerAppAddNumberStaffModel model)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");

                var create = await _service.UpdateCustomerAppAddNumberStaff(model);
                return Ok(create);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("LockUnLockCustomer/{customerid}/{state}")]
        public async Task<IActionResult> LockUnLockCustomer(long customerid, bool state)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");
                if (!token.Equals(_JeeCustomerSecrectkey)) return NotFound("Secrectkey Không hợp lệ");
                var messageError = await _service.LockUnLockCustomer(customerid, state);
                if (string.IsNullOrEmpty(messageError)) return Ok();
                return BadRequest(MessageReturnHelper.Custom(messageError));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion api for customer
    }
}