using DpsLibs.Data;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
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

        public static JeeHRCoCauToChucModelFromDB JeeHRCoCauToChucModelFromDBFromDepartmentDTO(DepartmentDTO dto)
        {
            var jeehr = new JeeHRCoCauToChucModelFromDB();
            jeehr.RowID = int.Parse(dto.RowID.ToString());
            jeehr.Title = dto.DepartmentName;
            return jeehr;
        }

        public static JeeHRCoCauToChucModelFromDB JeeHRCoCauToChucModelFromDBFromFlatJeeHRCoCauToChucModel(FlatJeeHRCoCauToChucModel dto)
        {
            var jeehr = new JeeHRCoCauToChucModelFromDB();
            jeehr.RowID = int.Parse(dto.RowID.ToString());
            jeehr.Title = dto.Title;
            return jeehr;
        }

        public static List<JeeHRChucVuToJeeHRFromDB> LstJeeHRChucVuToJeeHRFromDBFromLstJeeHRChucvu(List<JeeHRChucVu> LstJeeHRChucVu)
        {
            var list = new List<JeeHRChucVuToJeeHRFromDB>();
            foreach (var item in LstJeeHRChucVu)
            {
                list.Add(JeeHRChucVuToJeeHRFromDBFromJeeHRChucvu(item));
            }
            return list;
        }

        public static JeeHRChucVuToJeeHRFromDB JeeHRChucVuToJeeHRFromDBFromJeeHRChucvu(JeeHRChucVu jeeHRChucVu)
        {
            var data = new JeeHRChucVuToJeeHRFromDB();
            data.RowID = jeeHRChucVu.ID;
            data.Title = jeeHRChucVu.Title;
            return data;
        }
    }
}