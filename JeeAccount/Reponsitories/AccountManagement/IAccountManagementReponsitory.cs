using DpsLibs.Data;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public interface IAccountManagementReponsitory
    {
        Task<DataTable> GetListUsernameByCustormerIDAsync(long custormerID);

        Task<DataTable> GetAllAccountListAsync(long custormerID);

        Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcode(long custormerID, string appcode);

        Task<IEnumerable<AdminModel>> GetListAdminsByCustomerID(long customerID);

        Task<long> GetCustormerIDByUsername(string username);

        Task<long> GetCustormerIDByUserID(long UserID);

        Task<InfoUserDTO> GetInfoByUsername(string username);

        Task<InfoCustomerDTO> GetInfoByCustomerID(long customerID);

        Task<IEnumerable<AppListDTO>> GetListAppByCustomerID(long customerID);

        Task<IEnumerable<AppListDTO>> GetListAppByUserID(long UserID, long CustomerID = 0);

        Task<IEnumerable<AppListDTO>> GetListInfoAppByCustomerID(long customerID);

        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerID(long customerID);

        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID, string where, string orderby);

        ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin);

        ReturnSqlModel CreateAccount(DpsConnection cnn, AccountManagementModel account, long AdminUserID, long CustomerID, bool isAdmin = false);

        ReturnSqlModel UpdateAvatar(DpsConnection cnn, string AvatarUrl, long userID, long CustomerID);

        void UpdateAvatar(string AvatarUrl, long userID, long CustomerID);

        ReturnSqlModel UpdateAvatarFirstTime(DpsConnection cnn, string AvatarUrl, long userID, long CustomerID);

        ReturnSqlModel UpdatePersonalInfoCustomData(DpsConnection cnn, PersonalInfoCustomData personalInfoCustom, long userId, long customerId);

        long GetCurrentIdentity(DpsConnection cnn);

        string GetUsername(DpsConnection cnn, long userId, long customerId);

        long GetUserIdByUsername(string Username, long customerId);

        PersonalInfoCustomData GetPersonalInfoCustomData(long UserID, long CustomerID);

        ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID);

        long GetLastUserID(DpsConnection cnn);

        long GetCustomerIDByUserID(long UserID);

        ReturnSqlModel InsertAppCodeAccount(DpsConnection cnn, long UserID, List<int> AppID);

        List<int> GetAppIdByAppCode(DpsConnection cnn, List<string> AppCode);

        List<LoginAccountModel> GetListLogin(DpsConnection cnn);

        Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccount(long customerID);
    }
}