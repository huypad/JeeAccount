using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class EmployeeModel
    {
        public string ID_NV { get; set; }
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string Phai { get; set; }
        public string Email { get; set; }
        public string Structure { get; set; }
        public string TenChucVu { get; set; }
    }
}