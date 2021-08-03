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