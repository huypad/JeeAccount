using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.PermissionManagementService
{
    public interface IPermissionManagementService
    {
        Task<IEnumerable<AccountManagementDTO>> GetListAccountAdminAppNotAdminHeThong(QueryParams query, long Customerid, int AppID);

        Task CreateAdminHeThong(long userid, string username, long customerid, long UpdateBy);

        Task CreateAdminApp(long userid, string username, long customerid, long UpdateBy, List<int> AppID);

        Task RemoveAdminHeThong(long userid, string username, long customerid, long UpdateBy);

        Task RemoveAdminApp(long userid, string username, long customerid, long UpdateBy, List<int> AppID);
    }
}