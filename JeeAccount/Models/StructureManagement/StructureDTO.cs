using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.StructureManagement
{
    public class StructureDTO
    {
        public long Id { get; set; }
        public long LoaiDonVi { get; set; }
        public string DonVi { get; set; }
        public string MaDonvi { get; set; }
        public string MaDinhDanh { get; set; }
        public long Parent { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string Logo { get; set; }
        public string Note { get; set; }
        public bool Locked { get; set; }
        public long Priority { get; set; }
        public bool DangKyLichLanhDao { get; set; }
        public bool KhongCoVanThu { get; set; }
        public string ParentName { get; set; }
        public string CreatedDate { get; set; }

    }
}
