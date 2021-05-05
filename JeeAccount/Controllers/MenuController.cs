using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.Common;
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
        public MenuController(IConfiguration configuration)
        {
            _config = configuration;
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
            string id_menu = "0";
            try
            {
                var loginData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                //LoginData loginData = lc._GetInfoUser(Token);
                //if (loginData == null)
                //    return JsonResultCommon.DangNhap();
                //string id_menu_hr = CustomUserManager.GetRules(loginData.UserName.ToString());
                string id_menu_hr = ",1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30"; // test
                if (!string.IsNullOrEmpty(id_menu_hr))
                {
                    id_menu += id_menu_hr;
                }
                //select menu
                sql = $@"select title, Target, Summary, '#' as ALink, ISNULL(Icon, 'flaticon-interface-7') as Icon, '' as title_, position, Code
                from Mainmenu where code in (select distinct groupname from Tbl_submenu where  Id_row in ({id_menu})) order by position 
                select title, AllowPermit, Target, Tbl_submenu.id_row, PhanLoai1, PhanLoai2, GroupName, ALink, Summary, AppLink, AppIcon, '' as title_ from Tbl_submenu  where id_row in ({id_menu}) order by position";
                using (DpsConnection cnn = new DpsConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    ds = cnn.CreateDataSet(sql);
                    if (ds.Tables.Count == 0) return JsonResultCommon.ThatBai("Không có dữ liệu", cnn.LastError);
                }

                var data = from r in ds.Tables[0].AsEnumerable()
                           orderby r["position"]
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
                                           PhanLoai1 = c["PhanLoai1"].ToString(),//Dùng cho HR
                                           PhanLoai2 = c["PhanLoai2"].ToString(),//Dùng cho HR
                                       },
                           };

                return JsonResultCommon.ThanhCong(data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }
        #endregion
    }
}
