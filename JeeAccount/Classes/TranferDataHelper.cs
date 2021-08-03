using DpsLibs.Data;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
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
        public static List<AccUsernameModel> ListNhanVienJeeHRToListAccUsernameModel(List<NhanVienJeeHR> nhanviens, long customerid, List<JeeHRPersonalInfo> lstUsers)
        {
            var lst = new List<AccUsernameModel>();
            foreach (var nv in nhanviens)
            {
                foreach (var item in lstUsers)
                {
                    if (item.StaffID == nv.IDNV)
                    {
                        var acc = NhanVienJeeHRToAccUsernameModel(nv, customerid, item);
                        lst.Add(acc);
                        lstUsers.Remove(item);

                        break;
                    }
                }
            }
            return lst;
        }

        public static AccUsernameModel NhanVienJeeHRToAccUsernameModel(NhanVienJeeHR nhanvien, long customerid, JeeHRPersonalInfo jeeHRPersonalInfo)
        {
            var acc = new AccUsernameModel();
            acc.AvartarImgURL = nhanvien.avatar;
            acc.BgColor = GeneralService.GetColorFullNameUser(nhanvien.HoTen);
            acc.ChucVuID = Convert.ToInt32(nhanvien.jobtitleid).ToString();
            acc.CustomerID = customerid;
            acc.Department = nhanvien.Structure;
            acc.Email = nhanvien.Email;
            acc.FirstName = GeneralService.GetFirstname(nhanvien.HoTen);
            acc.LastName = GeneralService.Getlastname(nhanvien.HoTen);
            acc.FullName = nhanvien.HoTen;
            acc.Jobtitle = nhanvien.TenChucVu;
            acc.NgaySinh = nhanvien.NgaySinh;
            acc.PhoneNumber = nhanvien.PhoneNumber;
            acc.StructureID = nhanvien.structureid.ToString();
            acc.UserId = jeeHRPersonalInfo.UserId;
            acc.Username = jeeHRPersonalInfo.Username;
            return acc;
        }

        public static List<AccountManagementDTO> ListNhanVienJeeHRToAccountManagementDTO(List<NhanVienJeeHR> nhanviens, long customerid, List<JeeHRPersonalInfo> lstUsers, string connectString)
        {
            using (DpsConnection cnn = new DpsConnection(connectString))
            {
                var lst = new List<AccountManagementDTO>();
                var lstUserscopy = lstUsers.ToList();
                foreach (var nv in nhanviens)
                {
                    foreach (var item in lstUsers)
                    {
                        if (item.StaffID == nv.IDNV)
                        {
                            var usernameDTOManager = lstUserscopy.Find(dto => dto.StaffID == nv.managerid);
                            var acc = NhanVienJeeHRToAccountManagementDTO(nv, customerid, item, usernameDTOManager, cnn);
                            lst.Add(acc);
                            lstUsers.Remove(item);
                            break;
                        }
                    }
                }
                return lst;
            }
        }

        public static AccountManagementDTO NhanVienJeeHRToAccountManagementDTO(NhanVienJeeHR nhanvien, long customerid, JeeHRPersonalInfo jeeHRPersonalInfo, UserNameDTO userNameDTOManager, DpsConnection cnn)
        {
            var acc = new AccountManagementDTO();
            acc.AvartarImgURL = nhanvien.avatar;
            acc.BgColor = jeeHRPersonalInfo.BgColor;
            acc.ChucVuID = Convert.ToInt32(nhanvien.jobtitleid).ToString();
            acc.CustomerID = customerid;
            acc.Department = nhanvien.Structure;
            acc.Email = nhanvien.Email;
            acc.FirstName = GeneralService.GetFirstname(nhanvien.HoTen);
            acc.LastName = GeneralService.Getlastname(nhanvien.HoTen);
            acc.FullName = nhanvien.HoTen;
            acc.Jobtitle = nhanvien.TenChucVu;
            acc.NgaySinh = nhanvien.NgaySinh;
            acc.PhoneNumber = nhanvien.PhoneNumber;
            acc.StructureID = nhanvien.structureid.ToString();
            acc.UserId = jeeHRPersonalInfo.UserId;
            acc.Username = jeeHRPersonalInfo.Username;
            acc.DepartmentID = (int)nhanvien.structureid;
            acc.DirectManager = nhanvien.manager;
            acc.DirectManagerUserID = userNameDTOManager.UserId;
            acc.DirectManagerUsername = userNameDTOManager.Username;
            acc.Note = jeeHRPersonalInfo.Note;
            acc.PhoneNumber = jeeHRPersonalInfo.Phonenumber;
            acc.IsActive = jeeHRPersonalInfo.IsActive;
            acc.IsAdmin = jeeHRPersonalInfo.IsAdmin;
            acc.UserId = jeeHRPersonalInfo.UserId;
            acc.Username = jeeHRPersonalInfo.Username;
            acc.NgaySinh = nhanvien.NgaySinh;
            acc.JobtitleID = Convert.ToInt32(nhanvien.jobtitleid);
            return acc;
        }

        public static List<NhanVienDuocQuanLyTrucTiep_> ListNhanVienDuocQuanLyTrucTiep_FromNhanVienDuocQuanLyTrucTiep(List<NhanVienDuocQuanLyTrucTiep> lst)
        {
            var lstNhanVienDuocQuanLyTrucTiep_ = new List<NhanVienDuocQuanLyTrucTiep_>();

            foreach (var nv in lst)
            {
                var obj = new NhanVienDuocQuanLyTrucTiep_(nv);
                lstNhanVienDuocQuanLyTrucTiep_.Add(obj);
            }
            return lstNhanVienDuocQuanLyTrucTiep_;
        }

        public static List<FlatJeeHRCoCauToChucModel> FlatListJeeHRCoCauToChuc(List<JeeHRCoCauToChuc> lst)
        {
            var flatLst = new List<FlatJeeHRCoCauToChucModel>();

            foreach (var item in lst)
            {
                flatLst = JoinJeeHRCoCauToChuc(item, flatLst);
            }
            return flatLst;
        }

        public static List<FlatJeeHRCoCauToChucModel> JoinJeeHRCoCauToChuc(JeeHRCoCauToChuc jeecocau, List<FlatJeeHRCoCauToChucModel> lst)
        {
            var copyJeeCocau = new FlatJeeHRCoCauToChucModel(jeecocau);
            lst.Add(copyJeeCocau);

            foreach (var cocau in jeecocau.Children)
            {
                lst = JoinJeeHRCoCauToChuc(cocau, lst);
            }
            return lst;
        }

        public static IdentityServerReturn TranformIdentityServerReturnSqlModel(ReturnSqlModel returnSql)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(returnSql.ErrorCode);
            identity.message = returnSql.ErrorMessgage;
            return identity;
        }

        public static IdentityServerReturn TranformIdentityServerException(Exception ex)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
            return identity;
        }
    }
}