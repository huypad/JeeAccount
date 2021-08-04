﻿using JeeAccount.Models.Common;
using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Classes
{
    public static class MessageReturnHelper
    {
        public const int ERRORCODE = 101;                                              //lỗi token
        public const int ERRORDATA = 106;                                              //lỗi Data
        public const int ERRORCODETIME = 102;                                          //lỗi về time
        public const int ERRORCODE_SQL = 103;                                          //lỗi sql
        public const int ERRORCODE_FORM = 104;                                         //lỗi về dữ liệu khi post thiếu dl
        public const int ERRORCODE_ROLE = 105;                                         //lỗi về quyền truy cập chức năng
        public const int ERRORCODE_EXCEPTION = 0;                                      //lỗi exception
        public const int ERRORCODE_BADREQEST = 400;                                    //lỗi cú pháp không hợp lệ
        public const int ERRORCODE_UNAUTHORIZED = 401;                                 //lỗi không có quyền

        public static object Ok(object data, PageModel pageModel)
        {
            return new
            {
                data = data,
                panigator = pageModel
            };
        }

        public static object Ok(object data, PageModel pageModel, bool Visible)
        {
            return new
            {
                data = data,
                Visible = Visible,
                panigator = pageModel
            };
        }

        public static object Unauthorized()
        {
            return new
            {
                statusCode = ERRORCODE_UNAUTHORIZED,
                message = "Unauthorized",
            };
        }

        public static object CustomDataKhongTonTai()
        {
            return new
            {
                statusCode = ERRORCODE,
                message = "CustomData không tồn tại",
            };
        }

        public static object KhongTonTai(string message = "")
        {
            return new
            {
                statusCode = ERRORDATA,
                message = string.IsNullOrEmpty(message) ? "Không tồn tại" : message + " ;hông tồn tại",
            };
        }

        public static object KhongCoDuLieu(string message = "")
        {
            return new
            {
                statusCode = ERRORDATA,
                message = string.IsNullOrEmpty(message) ? "không có dữ liệu" : message + " không có dữ liệu",
            };
        }

        public static object Trung(string message)
        {
            return new
            {
                statusCode = ERRORDATA,
                message = message.ToLower() + "đã tồn tại",
            };
        }

        public static object BatBuoc(string str_required)
        {
            return new
            {
                statusCode = ERRORDATA,
                message = str_required.ToLower() + "là bắt buộc",
            };
        }

        public static object Custom(string str_custom)
        {
            return new
            {
                statusCode = ERRORDATA,
                message = str_custom.ToLower()
            };
        }

        public static object PhanQuyen(string quyen = "")
        {
            return new
            {
                statusCode = ERRORCODE_ROLE,
                message = "Không có quyền thực hiện chức năng " + (quyen.ToLower()?.Length == 0 ? "này" : quyen.ToLower()),
            };
        }

        public static object DangNhap()
        {
            return new
            {
                statusCode = ERRORCODE,
                message = "Phiên đăng nhập hết hiệu lực. Vui lòng đăng nhập lại!"
            };
        }

        public static object Exception(Exception last_error)
        {
            return new
            {
                message = "Lỗi truy xuất dữ liệu",
                statusCode = ERRORCODE_EXCEPTION,
                error = last_error != null ? last_error.Message : "",
            };
        }

        public static object ErrorJeeHR(ErrorJeeHR errorJeehr)
        {
            return new
            {
                statusCode = errorJeehr.code,
                message = errorJeehr.message,
                error = errorJeehr.LastError,
                allowForce = errorJeehr.allowForce
            };
        }

        public static object ExceptionJeeHR(JeeHRException jeeHRException)
        {
            return new
            {
                statusCode = jeeHRException.JeeHRError.code,
                message = jeeHRException.JeeHRError.message,
                error = jeeHRException.JeeHRError.LastError,
                allowForce = jeeHRException.JeeHRError.allowForce
            };
        }

        public static object PhanTrang()
        {
            return new
            {
                statusCode = ERRORCODE,
                message = "Dữ liệu phân trang không đúng",
            };
        }
    }
}