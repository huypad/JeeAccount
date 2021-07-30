using DpsLibs.Data;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JeeAccount.Services.AccountManagementService
{
    public interface IAccountManagementService
    {
        Task<string> GetDirectManagerByUsername(string username);

        Task<string> GetDirectManagerByUserID(string userid);

        Task<IdentityServerReturn> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl);

        Task<IdentityServerReturn> UpdatePersonalInfoCustomData(string Admin_access_token, PersonalInfoCustomData personalInfoCustom, long userId, long customerId);

        Task<IdentityServerReturn> UppdateCustomData(string Admin_access_token, ObjCustomData objCustomData, long customerId);

        string GetUsernameByUserID(string UserID, long CustomerId);

        Task UpdateAvatarWithChangeUrlAvatar(long UserId, string Username, long CustomerID, string apiUrl);

        Task<HttpResponseMessage> ResetPasswordRootCustomer(CustomerResetPasswordModel model);

        Task<IEnumerable<UserNameDTO>> GetListJustUsernameAndUserIDByCustormerID(long custormerID);

        Task<IEnumerable<string>> GetListJustUsernameByCustormerID(long custormerID);

        Task<IEnumerable<long>> GetListJustUserIDByCustormerID(long custormerID);

        Task<IEnumerable<long>> GetListJustCustormerID();

        Task<IEnumerable<long>> GetListJustCustormerIDAppCode(string appCode);

        Task<IEnumerable<string>> GetListDirectManager(long custormerID);

        Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManager(string DirectManager);

        Task<HttpResponseMessage> UpdateOneStaffIDByInputApiModel(InputApiModel model);

        Task<HttpResponseMessage> UpdateOneBgColorCustomData(InputApiModel model);

        void UpdateAllAppCodesCustomData(InputApiModel model, List<int> lstAppCode);

        void SaveNewAccountInAccount_AppAndAccountList(long customerID, long AdminUserID, AccountManagementModel account, List<int> ListAppID);

        Task<JeeAccountCustomData> GetJeeAccountCustomDataAsync(InputApiModel model);

        string UpdateAvatarCdn(string username, string base64);
    }
}