using JeeAccount.Models.AccountManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.PermissionManagement
{
    public interface IPermissionManagementRepository
    {
        Task<IEnumerable<AccountManagementDTO>> GetListAccountAdminAppNotAdminHeThongDefaultAsync(long customerID, int AppID, string where = "", string orderBy = "");

        Task<IEnumerable<AccountManagementDTO>> GetListAccountAdminAppNotAdminHeThongJeeHRAsync(long customerID, int AppID, string where = "", string orderBy = "");

        Task CreateAdminApp(long userid, long customerid, long UpdateBy, List<int> lstAppID);

        Task CreateAdminHeThong(long userid, long customerid, long updateBy);

        Task RemoveAdminHeThong(long userid, long customerid, long updateBy);

        Task RemoveAdminApp(long userid, long customerid, long UpdateBy, List<int> lstAppID);
    }
}