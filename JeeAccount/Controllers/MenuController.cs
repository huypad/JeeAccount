using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public MenuController(IConfiguration configuration)
        {
            _config = configuration;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
        }

        #region Load menu

        /// <summary>
        /// Load menu
        /// </summary>
        /// <returns></returns>

        [Route("LayMenuChucNang")]
        [HttpGet]
        public object LayMenuChucNang()
        {
            ErrorModel error = new ErrorModel();
            //string Token = lc.GetHeader(Request);
            DataSet ds = new DataSet();
            string sql = ""; ;
            try
            {
                var customData = Ulities.GetCustomDataByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return Unauthorized(MessageReturnHelper.DangNhap());
                }
                //select menu
                sql = $@"select title, Target, Summary, ALink, ISNULL(Icon, 'flaticon-interface-7') as Icon, '' as title_, position, Code
                from Mainmenu  order by position
                select title, AllowPermit, Target, Tbl_submenu.id_row, PhanLoai1, PhanLoai2, GroupName, ALink, Summary, AppLink, AppIcon, '' as title_ from Tbl_submenu  order by position";

                using (DpsConnection cnn = new DpsConnection(_connectionString))
                {
                    ds = cnn.CreateDataSet(sql);
                    if (ds.Tables.Count == 0) return JsonResultCommon.ThatBai("Không có dữ liệu", cnn.LastError);
                }
                var data = from r in ds.Tables[0].AsEnumerable()
                           select new
                           {
                               Code = r["Code"].ToString(),
                               Title = r["title"].ToString(),
                               Target = r["Target"],
                               Summary = r["Summary"].ToString(),
                               Icon = r["Icon"].ToString(),
                               ALink = r["ALink"].ToString(),
                               Child = from c in ds.Tables[1].AsEnumerable()
                                       where c["groupname"].ToString().Trim().ToLower().Equals(r["Code"].ToString().Trim().ToLower())
                                       select new
                                       {
                                           Title = c["title"].ToString(),
                                           Summary = c["Summary"].ToString(),
                                           AllowPermit = c["AllowPermit"].ToString(),
                                           Target = c["Target"].ToString(),
                                           GroupName = c["GroupName"].ToString(),
                                           ALink = c["ALink"].ToString(),
                                       },
                           };

                return JsonResultCommon.ThanhCong(data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        #endregion Load menu
    }
}