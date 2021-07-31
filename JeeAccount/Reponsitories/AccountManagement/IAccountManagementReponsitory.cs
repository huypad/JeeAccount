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
        #region giao diện JeeAccount  Management/AccountManagement

        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagementAsync(long customerID, string where, string orderby);

        #endregion giao diện JeeAccount  Management/AccountManagement

        Task<IEnumerable<string>> GetListJustUsernameByCustormerID(long custormerID);

        Task<string> GetDirectManagerByUsername(string username);

        Task<string> GetDirectManagerByUserID(string userid);

        Task<IEnumerable<UserNameDTO>> GetListJustUsernameAndUserIDByCustormerID(long custormerID);

        Task<IEnumerable<long>> GetListJustUserIDByCustormerID(long custormerID);

        Task<IEnumerable<long>> GetListJustCustormerID();

        Task<IEnumerable<long>> GetListJustCustormerIDAppCode(string appCode);

        Task<IEnumerable<string>> GetListDirectManager(long custormerID);

        Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManager(string DirectManager);

        Task<IEnumerable<AccUsernameModel>> GetListUsernameByCustormerIDAsync(long custormerID);

        Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcodeAsync(long custormerID, string appcode);

        Task<IEnumerable<AdminModel>> GetListAdminsByCustomerIDAsync(long customerID);

        Task<long> GetCustormerIDByUsernameAsync(string username);

        Task<long> GetCustormerIDByUserIDAsync(long UserID);

        void SaveStaffID(long UserID, long staffID);

        Task<InfoUserDTO> GetInfoByUsernameAsync(string username);

        Task<InfoCustomerDTO> GetInfoByCustomerIDAsync(long customerID);

        Task<IEnumerable<AppListDTO>> GetListAppByCustomerIDAsync(long customerID);

        Task<IEnumerable<AppListDTO>> GetListAppByUserIDAsync(long UserID, long CustomerID = 0);

        Task<IEnumerable<AppListDTO>> GetListInfoAppByCustomerIDAsync(long customerID);

        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerIDAsync(long customerID);

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

        Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccountAsync(long customerID);
    }
}