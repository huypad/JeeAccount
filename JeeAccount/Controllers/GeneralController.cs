using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/general")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GeneralController(IConfiguration configuration)
        {
            _config = configuration;
        }

        [HttpGet("GetMatchipNhanvien")]
        public object GetMatchipNhanVien()
        {
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                DataTable dt = new DataTable();
                SqlConditions Conds = new SqlConditions();
                Conds.Add("CustomerID", customData.JeeAccount.CustomerID);
                string sql = "select UserID, Username, LastName + '' + FirstName as Fullname, LastName  + ' ' + FirstName + ' (' + Username + ')' as Display from AccountList " +
                    "where CustomerID =@CustomerID and (Disable != 1 or Disable is null)";
                IEnumerable<NhanVienMatchip> data;
                using (DpsConnection cnn = new DpsConnection(_config.GetValue<string>("AppConfig:Connection")))
                {
                    dt = cnn.CreateDataTable(sql, Conds);
                    data = dt.AsEnumerable().Select(row => new NhanVienMatchip
                    {
                        UserId = long.Parse(row["UserId"].ToString()),
                        Username = row["Username"].ToString(),
                        Display = row["Display"].ToString(),
                        Fullname = row["Fullname"].ToString(),
                    });
                }
                return JsonResultCommon.ThanhCong(data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
    }
}