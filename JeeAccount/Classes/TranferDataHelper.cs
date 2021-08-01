using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.JeeHR;
using JeeAccount.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Classes
{
    public static class TranferDataHelper
    {
        public static List<AccUsernameModel> ListNhanVienJeeHRToListAccUsernameModel(List<NhanVienJeeHR> nhanviens, long customerid, List<UsernameUserIDStaffID> lstUsers)
        {
            var lst = new List<AccUsernameModel>();
            foreach (var nv in nhanviens)
            {
                foreach (var item in lstUsers)
                {
                    if (item.StaffID == nv.IDNV)
                    {
                        var acc = NhanVienJeeHRToAccUsernameModel(nv, customerid, item.UserId, item.Username);
                        lst.Add(acc);
                        lstUsers.Remove(item);
                        break;
                    }
                }
            }
            return lst;
        }

        public static AccUsernameModel NhanVienJeeHRToAccUsernameModel(NhanVienJeeHR nhanvien, long customerid, long userid, string username)
        {
            var acc = new AccUsernameModel();
            acc.AvartarImgURL = nhanvien.avatar;
            acc.BgColor = GeneralService.GetColorFullNameUser(nhanvien.HoTen);
            acc.ChucVuID = Convert.ToInt32(nhanvien.jobtitleid).ToString();
            acc.CustomerID = customerid;
            acc.Department = nhanvien.Structure;
            acc.Email = nhanvien.Email;
            acc.FirstName = GeneralService.getFirstname(nhanvien.HoTen);
            acc.LastName = GeneralService.getlastname(nhanvien.HoTen);
            acc.FullName = nhanvien.HoTen;
            acc.Jobtitle = nhanvien.TenChucVu;
            acc.NgaySinh = nhanvien.NgaySinh;
            acc.PhoneNumber = nhanvien.PhoneNumber;
            acc.StructureID = nhanvien.structureid.ToString();
            acc.UserId = userid;
            acc.Username = username;
            return acc;
        }
    }
}