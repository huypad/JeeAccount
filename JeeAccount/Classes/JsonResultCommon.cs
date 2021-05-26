using JeeAccount.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Classes
{
    public class JsonResultCommon
    {
        public static BaseModel<object> KhongTonTai(string name = "")
        {
            return new BaseModel<object>
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = string.IsNullOrEmpty(name) ? "Không tồn tại" : name + " không tồn tại",
                    code = Constant.ERRORDATA
                }
            };
        }

        public static BaseModel<object> Trung(string name)
        {
            return new BaseModel<object>
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = name + " đã tồn tại",
                    code = Constant.ERRORDATA
                }
            };
        }

        public static BaseModel<object> BatBuoc(string str_required)
        {
            if (!string.IsNullOrEmpty(str_required))
                str_required = str_required.ToLower();
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = "Thông tin " + str_required + " là bắt buộc",
                    code = Constant.ERRORDATA
                }
            };
        }

        public static BaseModel<object> Custom(string str_custom)
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = str_custom,
                    code = Constant.ERRORDATA
                }
            };
        }

        public static BaseModel<object> PhanQuyen(string quyen = "")
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = "Không có quyền thực hiện chức năng " + (quyen == "" ? "này" : quyen),
                    code = Constant.ERRORCODE_ROLE
                }
            };
        }

        public static BaseModel<object> DangNhap()
        {
            return new BaseModel<object>
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = "Phiên đăng nhập hết hiệu lực. Vui lòng đăng nhập lại!",
                    code = Constant.ERRORCODE
                },
            };
        }

        public static BaseModel<object> Exception(Exception last_error)
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = "Lỗi truy xuất dữ liệu",
                    LastError = last_error != null ? last_error.Message : "",
                    code = Constant.ERRORCODE_EXCEPTION
                }
            };
        }

        public static BaseModel<object> PhanTrang()
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = "Dữ liệu phân trang không đúng",
                    code = Constant.ERRORCODE
                },
            };
        }

        public static BaseModel<object> ThanhCong()
        {
            return new BaseModel<object>
            {
                status = 1,
            };
        }

        public static BaseModel<object> ThanhCong(object data)
        {
            return new BaseModel<object>
            {
                status = 1,
                data = data
            };
        }

        public static BaseModel<object> ThanhCong(object data, PageModel pageModel)
        {
            return new BaseModel<object>
            {
                status = 1,
                data = data,
                panigator = pageModel
            };
        }

        public static BaseModel<object> ThanhCong(object data, PageModel pageModel, bool Visible)
        {
            return new BaseModel<object>
            {
                status = 1,
                data = data,
                Visible = Visible,
                panigator = pageModel
            };
        }

        public static BaseModel<object> ThatBai(string message, Exception last_error)
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = message,
                    LastError = last_error != null ? last_error.Message : "",
                    code = Constant.ERRORCODE_EXCEPTION
                }
            };
        }

        public static BaseModel<object> ThatBai(string message)
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = message,
                    code = Constant.ERRORDATA
                }
            };
        }

        public static BaseModel<object> ThatBai(string message, bool Visible)
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = message,
                    code = Constant.ERRORDATA
                },
                Visible = Visible,
            };
        }

        public static BaseModel<object> KhongHopLe(string name = "")
        {
            return new BaseModel<object>
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = string.IsNullOrEmpty(name) ? "Không hợp lệ" : name + " không hợp lệ",
                    code = Constant.ERRORDATA
                }
            };
        }

        public static BaseModel<object> SQL(string last_error)
        {
            return new BaseModel<object>()
            {
                status = 0,
                error = new ErrorModel()
                {
                    message = "Có gì đó không đúng, vui lòng thử lại sau",
                    //error = last_error,
                    code = Constant.ERRORCODE_SQL
                }
            };
        }
    }
}