using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.JeeHR
{
    public class NhanVienDuocQuanLyTrucTiep_
    {
        public string MaNV { get; set; }
        public long ID_NV { get; set; }
        public string Username { get; set; }
        public string HoTen { get; set; }
        public long ChucVuID { get; set; }

        public NhanVienDuocQuanLyTrucTiep_(NhanVienDuocQuanLyTrucTiep nhanvien)
        {
            MaNV = nhanvien.MaNV;
            ID_NV = Convert.ToInt32(nhanvien.ID_NV);
            HoTen = nhanvien.HoTen;
            Username = nhanvien.Username;
            ChucVuID = Convert.ToInt32(nhanvien.ChucVuID);
        }
    }
}