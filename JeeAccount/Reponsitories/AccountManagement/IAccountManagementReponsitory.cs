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

        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagementDefaultAsync(long customerID, string where = "", string orderby = "");

        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagementIsJeeHRAsync(long customerID, string where = "", string orderBy = "");

        #endregion giao diện JeeAccount  Management/AccountManagement

        #region api

        Task<IEnumerable<AccUsernameModel>> GetListAccUsernameModelDefaultAsync(long custormerID);

        Task<IEnumerable<AccUsernameModel>> GetListAccUsernameModelIsJeeHRAsync(long custormerID);

        Task<IEnumerable<long>> GetListJustCustormerID();

        Task<IEnumerable<long>> GetListJustCustormerIDAppCode(string appCode);

        Task<long> GetCustormerIDByUsernameAsync(string username);

        Task<long> GetCustormerIDByUserIDAsync(long UserID);

        Task<IEnumerable<AdminModel>> GetListAdminsByCustomerIDAsync(long customerID);

        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerIDDefaultAsync(long customerID);

        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerIDIsJeeHRAsync(long customerID);

        Task<InfoCustomerDTO> GetInfoByCustomerIDAsync(long customerID);

        Task<InfoUserDTO> GetInfoByUsernameIsJeeHRAsync(string username);

        Task<InfoUserDTO> GetInfoByUsernameDefaultAsync(string username);

        Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcodeAsync(long custormerID, string appcode);

        Task<IEnumerable<AppListDTO>> GetListAppByCustomerIDAsync(long customerID);

        Task<string> GetDirectManagerByUserID(string userid);

        Task<string> GetDirectManagerByUsername(string username);

        Task<IEnumerable<UserNameDTO>> GetListJustUsernameAndUserIDByCustormerID(long custormerID);

        Task<IEnumerable<string>> GetListJustUsernameByCustormerID(long custormerID);

        Task<IEnumerable<long>> GetListJustUserIDByCustormerID(long custormerID);

        Task<IEnumerable<string>> GetListDirectManager(long custormerID);

        Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManagerDefaultAsync(string DirectManager);

        Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManagerJeeHRAsync(string DirectManager);

        Task<IEnumerable<AppListDTO>> GetListInfoAppByCustomerIDAsync(long customerID);

        long GetCurrentIdentity(DpsConnection cnn);

        long GetLastUserID(DpsConnection cnn);

        List<int> GetAppIdByAppCode(DpsConnection cnn, List<string> AppCode);

        Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccountAsync(long customerID);

        #endregion api

        bool ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin);

        void CreateAccount(bool isJeeHR, DpsConnection cnn, AccountManagementModel account, string usernameCreatedBy, long CustomerID, bool isAdmin = false);

        void UpdateAccount(bool isJeeHR, DpsConnection cnn, AccountManagementModel account, long CustomerID);

        void UpdateAvatar(string AvatarUrl, long userID, long CustomerID);

        ReturnSqlModel UpdatePersonalInfoCustomData(DpsConnection cnn, PersonalInfoCustomData personalInfoCustom, long userId, long customerId);

        ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID);

        void InsertAppCodeAccount(DpsConnection cnn, long UserID, List<int> AppID, string createdBy, bool IsAdmin = false);

        void RemoveAppCodeAccount(DpsConnection cnn, long UserID, List<int> AppID, string editBy);

        void DeleteAccountManagement(string DeletedBy, long customerID, long userid);
    }
}