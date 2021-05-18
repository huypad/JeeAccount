using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.StructureManagement
{
    public class StructureModel
    {
        public long Id { get; set; }
        public long LoaiDonVi { get; set; }
        public string DonVi { get; set; }
        public string MaDonvi { get; set; }
        public string MaDinhDanh { get; set; }
        public long Parent { get; set; }
        public long SDT { get; set; }
        public long Email { get; set; }
        public long DiaChi { get; set; }
        public long Logo { get; set; }
        public string Note { get; set; }
        public bool Locked { get; set; }
        public long Priority { get; set; }
        public bool DangKyLichLanhDao { get; set; }
        public bool KhongCoVanThu { get; set; }
    }
}
