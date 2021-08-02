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
        Task<InfoUserDTO> GetInfoByUsernameAsync(string username, long customerid);

        Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManager(string username, long customerid);

        Task<IdentityServerReturn> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl);

        Task<IdentityServerReturn> UpdatePersonalInfoCustomData(string Admin_access_token, PersonalInfoCustomData personalInfoCustom, long userId, long customerId);

        Task<IdentityServerReturn> UppdateCustomData(string Admin_access_token, ObjCustomData objCustomData, long customerId);

        Task UpdateAvatarWithChangeUrlAvatar(long UserId, string Username, long CustomerID, string apiUrl);

        Task<HttpResponseMessage> ResetPasswordRootCustomer(CustomerResetPasswordModel model);

        Task<HttpResponseMessage> UpdateOneStaffIDByInputApiModel(InputApiModel model);

        Task<HttpResponseMessage> UpdateOneBgColorCustomData(InputApiModel model);

        void UpdateAllAppCodesCustomData(InputApiModel model, List<int> lstAppCode);

        void SaveNewAccountInAccount_AppAndAccountList(long customerID, long AdminUserID, AccountManagementModel account, List<int> ListAppID);

        Task<JeeAccountCustomData> GetJeeAccountCustomDataAsync(InputApiModel model);

        string UpdateAvatarCdn(string username, string base64);
    }
}