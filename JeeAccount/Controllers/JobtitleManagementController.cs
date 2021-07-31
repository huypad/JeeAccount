using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.JobtitleManagement;
using JeeAccount.Reponsitories.JobtitleManagement;
using JeeAccount.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/jobtitlemanagement")]
    [ApiController]
    public class JobtitleManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IJobtitleManagementReponsitory _reponsitory;
        private readonly string _connectionString;

        public JobtitleManagementController(IConfiguration configuration, IJobtitleManagementReponsitory reponsitory)
        {
            _config = configuration;
            _reponsitory = reponsitory;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
        }

        [HttpGet("GetListJobtitle")]
        public async Task<object> GetListJobtitle([FromQuery] QueryParams query)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var depart = await _reponsitory.GetListJobtitleAsync(customData.JeeAccount.CustomerID);
                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("CreateJobtitle")]
        public object CreateJobtitle(JobtitleModel depart)
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                var username = GeneralService.GetUsernameByUserID(_connectionString, customData.JeeAccount.UserID.ToString());
                _reponsitory.CreateJobtitle(depart, customData.JeeAccount.CustomerID, username.ToString());

                return JsonResultCommon.ThanhCong(depart);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("ChangeTinhTrang")]
        public async Task<object> changeTinhTrang(JobChangeTinhTrangModel acc)
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

        [HttpGet("ApiTaoDefaultJobtitle")]
        public object ApiTaoDefaultJobtitle()
        {
            try
            {
                var lstCustomerid = GeneralService.GetLstCustomerid(_connectionString);
                foreach (var customerid in lstCustomerid)
                {
                    if (!_reponsitory.CheckJobtitleExist(customerid, _connectionString))
                    {
                        if (customerid == 25) continue;
                        var job1 = new JobtitleModel();
                        job1.JobtitleName = "Developer";
                        var lstuserid = GeneralService.GetLstUserIDByCustomerid(_connectionString, customerid);
                        if (lstuserid is not null)
                        {
                            var userid = lstuserid[0];
                            var username = GeneralService.GetUsernameByUserID(_connectionString, userid.ToString());
                            _reponsitory.CreateJobtitle(job1, customerid, username.ToString());
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

        [HttpGet("ApiGoi1lanSaveJobtitleID")]
        public object ApiGoi1lanSaveJobtitleID()
        {
            try
            {
                var lstCustomerid = GeneralService.GetLstCustomerid(_connectionString);
                foreach (var customerid in lstCustomerid)
                {
                    if (customerid == 25) continue;

                    if (_reponsitory.CheckJobtitleExist(customerid, _connectionString))
                    {
                        var lst = GeneralService.GetLstUsernameByCustomerid(_connectionString, customerid);

                        _reponsitory.ApiGoi1lanSaveJobtitleID(customerid, lst);
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