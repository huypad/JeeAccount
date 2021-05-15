using DpsLibs.Data;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public interface IAccountManagementReponsitory
    {
        Task<IEnumerable<AccUsernameModel>> GetListUsernameByCustormerID(long custormerID);
        Task<IEnumerable<AccUsernameModel>> GetListUsernameByAppcode(long custormerID, string appcode);
        Task<IEnumerable<AccUsernameModel>> GetListAdminsByCustomerID(long customerID);
        Task<long> GetCustormerIDByUsername(string username);
        Task<InfoUserDTO> GetInfoByUsername(string username);
        Task<InfoUserDTO> GetInfoByCustomerID(long customerID);
        Task<IEnumerable<AppListDTO>> GetListAppByCustomerID(long customerID);
        Task<IEnumerable<AppListDTO>> GetListAppByUserID(long UserID);
        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerID(long customerID);
        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID);
        ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin);
        ReturnSqlModel CreateAccount(DpsConnection cnn, AccountManagementModel account, long AdminUserID, long CustomerID, bool isAdmin = false);
        ReturnSqlModel UpdateAvatar(DpsConnection cnn, string AvatarUrl, long userID, long CustomerID);
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
    }
}

