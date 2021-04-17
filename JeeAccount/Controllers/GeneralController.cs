using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using static JeeAccount.Models.Common.Common;

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

        public static string calPercentage(object tong, object v)
        {
            try
            {
                if (tong == null || v == null)
                    return "";
                double re = 0;
                double sum = double.Parse(tong.ToString());
                if (sum == 0)
                    return "0.00";
                double val = double.Parse(v.ToString());
                re = (val * 100) / sum;
                return string.Format("{0:N2}", re);
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static string genLinkImage(string domain, long idKH, string id_nv, string contentRootPath)
        {
            //string Image = domain + "dulieu/Images/Noimage.jpg";
            string Image = "";
            string str = "images/nhanvien/" + idKH + "/" + id_nv + ".jpg";
            string path = Path.Combine(contentRootPath, str);
            if (System.IO.File.Exists(path))
            {
                Image = domain + str;
            }
            return Image;
        }

        public static string genLinkAttachment(string domain, object path)
        {
            if (path == null)
                return "";
            return domain + Constant.RootUpload + path;
        }

        public static bool log(DpsConnection cnn, int id_action, long object_id, long id_user, string log_content = "", object _old = null, object _new = null)
        {
            Hashtable val = new Hashtable();
            val["id_action"] = id_action;
            val["object_id"] = object_id;
            val["CreatedBy"] = id_user;
            if (!string.IsNullOrEmpty(log_content))
                val["log_content"] = log_content;
            if (_old == null)
                val["oldvalue"] = DBNull.Value;
            else
                val["oldvalue"] = _old;
            if (_new == null)
                val["newvalue"] = DBNull.Value;
            else
                val["newvalue"] = _new;
            return cnn.Insert(val, "we_log") == 1;
        }

        [HttpGet("GetMatchipNhanvien")]
        public BaseModel<object> GetMatchipNhanVien()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
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
                using (DpsConnection cnn = new DpsConnection(_config.GetConnectionString("DefaultConnection")))
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

        [HttpGet("GetSelectionDepartMent")]
        public BaseModel<object> GetSelectionDepartMent()
        {
            try
            {
                var customData = Ulities.GetUserByHeader(HttpContext.Request.Headers);
                if (customData is null)
                {
                    return JsonResultCommon.BatBuoc("Đăng nhập");
                }

                DataTable dt = new DataTable();
                SqlConditions Conds = new SqlConditions();
                Conds.Add("CustomerID", customData.JeeAccount.CustomerID);
                string sql = "select RowID, DepartmentName from DepartmentList " +
                    "where CustomerID =@CustomerID and (Disable != 1 or Disable is null)";
                IEnumerable<DepartmentSelection> data;
                using (DpsConnection cnn = new DpsConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    dt = cnn.CreateDataTable(sql, Conds);
                    data = dt.AsEnumerable().Select(row => new DepartmentSelection
                    {
                        RowID = row["RowID"].ToString(),
                        DeparmentName = row["DepartmentName"].ToString(),
                    });
                }
                return JsonResultCommon.ThanhCong(data);
            }
            catch (Exception ex)
            {
                return JsonResultCommon.Exception(ex);
            }
        }

        public static PersonalInfoCustomData GetPersonalInfoCustomData(DpsConnection cnn, string Username, long CustomerId)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConditions Conds = new SqlConditions();
                Conds.Add("CustomerID", CustomerId);
                Conds.Add("Username", Username);
                string sql = "select AvartarImgURL as 'Avatar', Birthday, Department, LastName + ' ' + FirstName as 'Fullname', Jobtitle, FirstName as 'Name', PhoneNumber from AccountList" +
                    "where CustomerID =@CustomerID and (Disable != 1 or Disable is null) and Username=@Username";

                dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                    return new PersonalInfoCustomData();

                return new PersonalInfoCustomData {
                    Avatar = dt.Rows[0]["Avatar"].ToString(),
                    Birthday = dt.Rows[0]["Birthday"].ToString(),
                    Departmemt = dt.Rows[0]["Department"].ToString(),
                    Fullname = dt.Rows[0]["Fullname"].ToString(),
                    Jobtitle = dt.Rows[0]["Jobtitle"].ToString(),
                    Name = dt.Rows[0]["Name"].ToString(),
                    Phonenumber = dt.Rows[0]["PhoneNumber"].ToString()
                };
                
            }
            catch (Exception ex)
            {
                return new PersonalInfoCustomData();
            }
        }
    }
}
