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
        Task<IEnumerable<AccUsernameModel>> GetListUsernameByCustormerID(long customerID);

        Task<long> GetCustormerIDByUsername(string username);

        Task<long> GetCustormerIDByUserID(long userid);

        Task<string> GetDirectManagerByUsername(string username);

        Task<string> GetDirectManagerByUserID(string userid);

        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerID(long customerID);

        Task<InfoCustomerDTO> GetInfoByCustomerID(long customerID);

        Task<InfoUserDTO> GetInfoByUsername(string username);

        Task<InfoUserDTO> GetInfoByUserID(string userid);

        Task<IEnumerable<AdminModel>> GetListAdminsByCustomerID(long customerID);

        Task<IEnumerable<AppListDTO>> GetListAppByCustomerID(long customerID);

        Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcode(long customerID, string appcode);

        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(long customerID);

        ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin);

        ReturnSqlModel UpdateAvatar(string AvatarUrl, long userID, long CustomerID);

        Task<IdentityServerReturn> CreateAccount(string Admin_accessToken, long customerID, long AdminUserID, AccountManagementModel account, string apiUrl);

        Task<IdentityServerReturn> UpdatePersonalInfoCustomData(string Admin_access_token, PersonalInfoCustomData personalInfoCustom, long userId, long customerId);

        Task<IdentityServerReturn> UppdateCustomData(string Admin_access_token, ObjCustomData objCustomData, long customerId);

        Task<object> login(string username, string password);

        long GetUserIdByUsername(string Username, long CustomerId);

        PersonalInfoCustomData GetPersonalInfoCustomData(long UserID, long CustomerID);

        Task<IdentityServerReturn> UpdateAvatarWithChangeUrlAvatar(string Admin_access_token, long UserId, string Username, long CustomerID, string apiUrl);

        ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID);

        bool checkUserIDInCustomerID(long UserID, long CustomerID);

        Task<IEnumerable<AppListDTO>> GetListAppByUserID(long UserID);

        Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccount(long CustomerID);

        List<int> GetAppIdByAppCode(DpsConnection cnn, List<string> AppCode);

        List<LoginAccountModel> GetListLogin();

        Task<ReturnSqlModel> UpdateTool();

        Task<HttpResponseMessage> ResetPasswordRootCustomer(CustomerResetPasswordModel model);
    }
}