using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.JeeHR
{
    public class NhanVienJeeHR
    {
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string Structure { get; set; }
        public string Phai { get; set; }
        public string PhoneNumber { get; set; }

        public string TenChucVu { get; set; }
        public string MaNV { get; set; }
        public string Email { get; set; }
        public string TuNgay { get; set; }
        public DateTime NgayBatDauLamViec { get; set; }
        public long IDNV { get; set; }
        public string Title { get; set; }
        public string cmnd { get; set; }
        public string avatar { get; set; }
        public string username { get; set; }
        public long structureid { get; set; }
        public long jobtitleid { get; set; }
        public long managerid { get; set; }
        public string manager { get; set; }
    }
}