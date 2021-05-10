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

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/customermanagement")]
    [ApiController]

    public class CustomerManagementController: ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IProducer _producer;
        private readonly CustomerManagementService _service;
        public readonly string TopicAddNewCustomer;
        public CustomerManagementController(IConfiguration configuration, IProducer producer)
        {
            _config = configuration;
            _service = new CustomerManagementService(_config.GetConnectionString("DefaultConnection"));
            _producer = producer;
            TopicAddNewCustomer = _config.GetValue<string>("KafkaTopic:TopicAddNewCustomer");
        }

        [HttpPost("Get_DSCustomer")]
        public object GetListUsernameByCustormerID([FromBody] QueryRequestParams query)
        {
            try
            {
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (string.IsNullOrEmpty(access_token))
                {
                    return JsonResultCommon.DangNhap();
                }
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
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (string.IsNullOrEmpty(access_token))
                {
                    return JsonResultCommon.DangNhap();
                }

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
        public async Task<BaseModel<object>> CreateCustomer([FromBody] CustomerModel customerModel)
        {
            try
            {
                var access_token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (string.IsNullOrEmpty(access_token))
                {
                    return JsonResultCommon.DangNhap();
                }

                var checkTrungCode = _service.checkTrungCode(customerModel.Code);
                if (checkTrungCode) return JsonResultCommon.Trung("Code");
                var create = await _service.CreateCustomer(access_token, customerModel, _producer, TopicAddNewCustomer);
                if (create.statusCode != 0)
                {
                    return JsonResultCommon.ThatBai(create.message);
                }
                return JsonResultCommon.ThanhCong();
            } catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
    }
}
