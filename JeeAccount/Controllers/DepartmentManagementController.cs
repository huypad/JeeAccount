using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/accountdepartmentmanagement")]
    [ApiController]
    public class DepartmentManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IDepartmentManagementReponsitory _reponsitory;
        private readonly string _connectionString;
        private readonly string HOST_JEEHR_API;

        public DepartmentManagementController(IConfiguration configuration, IDepartmentManagementReponsitory reponsitory)
        {
            _config = configuration;
            _reponsitory = reponsitory;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
        }

        [HttpGet("GetListDepartment")]
        public async Task<object> GetListDepartment([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Thông tin customData");
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return JsonResultCommon.DangNhap();
                }
                var checkUsedJeeHr = GeneralService.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (!checkUsedJeeHr)
                {
                    var depart = await _reponsitory.GetListDepartment(customData.JeeAccount.CustomerID);
                    return JsonResultCommon.ThanhCong(depart);
                }
                else
                {
                    var jeehrController = new JeeHRController(HOST_JEEHR_API);
                    var list = await jeehrController.GetDSCoCauToChuc(token);
                    return list;
                }
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("GetDSPhongban")]
        public async Task<IActionResult> GetDSPhongban([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Ok(MessageReturnHelper.CustomDataKhongTonTai());
                }
                var token = Ulities.GetAccessTokenByHeader(HttpContext.Request.Headers);
                if (token is null)
                {
                    return Ok(MessageReturnHelper.DangNhap());
                }
                var checkUsedJeeHr = GeneralService.IsUsedJeeHRCustomerid(_connectionString, customData.JeeAccount.CustomerID);
                if (!checkUsedJeeHr)
                {
                    var depart = await _reponsitory.GetListDepartment(customData.JeeAccount.CustomerID);
                    var obj = new
                    {
                        tree = DBNull.Value,
                        flat = depart,
                        isTree = false
                    };
                    return Ok(obj);
                }
                else
                {
                    var jeehrController = new JeeHRController(HOST_JEEHR_API);
                    var list = await jeehrController.GetDSCoCauToChuc(token);
                    if (list.status == 1)
                    {
                        var obj = new
                        {
                            tree = list.data,
                            flat = TranferDataHelper.FlatListJeeHRCoCauToChuc(list.data),
                            isTree = true
                        };

                        return Ok(obj);
                    }
                    else
                    {
                        return BadRequest(MessageReturnHelper.ErrorJeeHR(list.error));
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(MessageReturnHelper.Exception(ex));
            }
        }

        [HttpPost("CreateDepartment")]
        public object CreateDepartment(DepartmentModel depart)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var username = GeneralService.GetUsernameByUserID(_connectionString, customData.JeeAccount.UserID.ToString());
                _reponsitory.CreateDepartment(depart, customData.JeeAccount.CustomerID, username.ToString());

                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("ChangeTinhTrang")]
        public async Task<object> changeTinhTrang(DepChangeTinhTrangModel acc)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return await Task.FromResult(JsonResultCommon.BatBuoc("Thông tin đăng nhập CustomData"));
                }

                ReturnSqlModel update = _reponsitory.ChangeTinhTrang(customData.JeeAccount.CustomerID, acc.RowID, acc.Note, customData.JeeAccount.UserID);
                if (!update.Susscess)
                {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST))
                    {
                        return await Task.FromResult(JsonResultCommon.KhongTonTai("tài khoản"));
                    }
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION))
                    {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return Task.FromResult(JsonResultCommon.ThatBai(update.ErrorMessgage));
                    }
                }
                return await Task.FromResult(JsonResultCommon.ThanhCong(update));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(JsonResultCommon.Exception(ex));
            }
        }

        [HttpGet("ApiTaoDefaultDepartment")]
        public object ApiTaoDefaultDepartment()
        {
            try
            {
                var depart = new DepartmentModel();
                depart.DepartmentName = "Phòng kỹ thuật";
                var lstCustomerid = GeneralService.GetLstCustomerid(_connectionString);
                foreach (var customerid in lstCustomerid)
                {
                    if (!_reponsitory.CheckDepartmentExist(customerid, _connectionString))
                    {
                        var lstuserid = GeneralService.GetLstUserIDByCustomerid(_connectionString, customerid);
                        if (lstuserid is not null)
                        {
                            var userid = lstuserid[0];
                            var username = GeneralService.GetUsernameByUserID(_connectionString, userid.ToString());
                            _reponsitory.CreateDepartment(depart, customerid, username.ToString());
                        }
                    }
                }

                return JsonResultCommon.ThanhCong();
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("ApiGoi1lanSaveDepartmentID")]
        public object ApiGoi1lanSaveDepartmentID()
        {
            try
            {
                var depart = new DepartmentModel();
                var lstCustomerid = GeneralService.GetLstCustomerid(_connectionString);
                foreach (var customerid in lstCustomerid)
                {
                    if (_reponsitory.CheckDepartmentExist(customerid, _connectionString))
                    {
                        var lst = GeneralService.GetLstUsernameByCustomerid(_connectionString, customerid);

                        _reponsitory.ApiGoi1lanSaveDepartmentID(customerid, lst);
                    }
                }

                return JsonResultCommon.ThanhCong();
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
    }
}