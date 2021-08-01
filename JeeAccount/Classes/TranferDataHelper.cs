using DpsLibs.Data;
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

        public static List<AccountManagementDTO> ListNhanVienJeeHRToAccountManagementDTO(List<NhanVienJeeHR> nhanviens, long customerid, List<UsernameUserIDStaffID> lstUsers, string connectString)
        {
            using (DpsConnection cnn = new DpsConnection(connectString))
            {
                var lst = new List<AccountManagementDTO>();
                foreach (var nv in nhanviens)
                {
                    foreach (var item in lstUsers)
                    {
                        if (item.StaffID == nv.IDNV)
                        {
                            var acc = NhanVienJeeHRToAccountManagementDTO(nv, customerid, item.UserId, item.Username, cnn);
                            lst.Add(acc);
                            lstUsers.Remove(item);
                            break;
                        }
                    }
                }
                return lst;
            }
        }

        public static AccountManagementDTO NhanVienJeeHRToAccountManagementDTO(NhanVienJeeHR nhanvien, long customerid, long userid, string username, DpsConnection cnn)
        {
            var acc = new AccountManagementDTO();
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
            acc.DepartmentID = (int)nhanvien.structureid;
            acc.DirectManager = nhanvien.manager;
            acc.DirectManagerUserID = long.Parse(GeneralService.GetUserIDByStaffIDCnn(cnn, nhanvien.managerid.ToString()).ToString());
            acc.DirectManagerUsername = GeneralService.GetUsernameByUserIDCnn(cnn, acc.DirectManagerUserID.ToString()).ToString();
            acc.Note = null;
            acc.PhoneNumber = "";
            acc.IsActive = GeneralService.CheckIsActiveByUserIDCnn(cnn, userid.ToString());
            acc.IsAdmin = GeneralService.CheckIsAdminByUserIDCnn(cnn, userid.ToString());
            acc.UserId = userid;
            acc.Username = username;
            acc.NgaySinh = nhanvien.NgaySinh;
            acc.JobtitleID = Convert.ToInt32(nhanvien.jobtitleid);
            return acc;
        }
    }
}