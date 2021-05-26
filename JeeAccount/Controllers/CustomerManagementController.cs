using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common = JeeAccount.Classes.Common;
using DPSinfra.Kafka;
using static JeeAccount.Models.Common.Panigator;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Services.CustomerManagementService;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/customermanagement")]
    [ApiController]
    public class CustomerManagementController : ControllerBase
    {
        private IConfiguration _config;
        private IProducer _producer;
        private ICustomerManagementService _service;
        public string TopicAddNewCustomer;

        public CustomerManagementController(IConfiguration configuration, IProducer producer, ICustomerManagementService customerManagementService)
        {
            _config = configuration;
            _service = customerManagementService;
            _producer = producer;
            TopicAddNewCustomer = _config.GetValue<string>("KafkaTopic:TopicAddNewCustomer");
        }

        [HttpPost("Get_DSCustomer")]
        public object GetListUsernameByCustormerID([FromBody] QueryRequestParams query)
        {
            try
            {
                query = query == null ? new QueryRequestParams() : query;
                BaseModel<object> model = new BaseModel<object>();
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
                        orderByStr = filter[query.Sort.ColumnName] + " " + (query.Sort.Direction.ToLower().Equals("asc") ? "asc" : "desc");
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
        public BaseModel<object> GetListApp()
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

        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerModel model)
        {
            try
            {
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null) return NotFound("Secrectkey");

                var create = await _service.CreateCustomer(model);
                if (create.statusCode == 0)
                {
                    return Ok(token);
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
    }
}