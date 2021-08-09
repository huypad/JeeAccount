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
        Task<IEnumerable<AccountManagementDTO>> GetListAccountManagement(QueryParams query, long customerid);

        Task UpdateAvatarWithChangeUrlAvatar(long UserId, string Username, long CustomerID, string apiUrl);

        Task<IEnumerable<AccUsernameModel>> GetListAccUsernameModel(long CustomerID);

        Task<IEnumerable<long>> GetListJustCustormerID();

        Task<IEnumerable<long>> GetListJustCustormerIDAppCode(string appCode);

        Task<long> GetCustormerIDByUsernameAsync(string username);

        Task<long> GetCustormerIDByUserIDAsync(long UserId);

        Task<long> GetCustormerID(InputApiModel model);

        Task<IEnumerable<InfoAdminDTO>> GetInfoAdminAccountByCustomerIDAsync(long customerID);

        Task<InfoCustomerDTO> GetInfoByCustomerIDAsync(long customerID);

        Task<IEnumerable<AdminModel>> GetListAdminsByCustomerIDAsync(long customerID);

        Task<IEnumerable<AppListDTO>> GetListAppByCustomerIDAsync(long customerID);

        Task<IEnumerable<UserNameDTO>> GetListUsernameByAppcodeAsync(long custormerID, string appcode);

        Task<string> GetDirectManagerByUserID(string userid);

        Task<string> GetDirectManagerByUsername(string username);

        Task<IEnumerable<UserNameDTO>> GetListJustUsernameAndUserIDByCustormerID(long custormerID);

        Task<IEnumerable<string>> GetListJustUsernameByCustormerID(long custormerID);

        Task<IEnumerable<long>> GetListJustUserIDByCustormerID(long custormerID);

        Task<IEnumerable<string>> GetListDirectManager(long custormerID);

        Task<InfoUserDTO> GetInfoByUsernameAsync(string username, long customerid);

        Task<IEnumerable<AccUsernameModel>> ListNhanVienCapDuoiDirectManagerByDirectManager(string username, long customerid);

        Task<IEnumerable<CustomerAppDTO>> GetListCustomerAppByCustomerIDFromAccountAsync(long customerID);

        Task<JeeAccountCustomData> GetJeeAccountCustomDataAsync(InputApiModel model);

        ReturnSqlModel ChangeTinhTrang(long customerID, string Username, string Note, long UserIdLogin);

        ReturnSqlModel UpdateDirectManager(string Username, string DirectManager, long customerID);

        Task CreateAccount(bool isJeeHR, string Admin_accessToken, long customerID, string usernameCreatedBy, AccountManagementModel account);

        Task<IdentityServerReturn> UpdatePersonalInfoCustomData(string Admin_access_token, PersonalInfoCustomData personalInfoCustom, long userId, long customerId);

        Task<IdentityServerReturn> UppdateCustomData(string Admin_access_token, ObjCustomData objCustomData, long customerId);

        Task<HttpResponseMessage> ResetPasswordRootCustomer(CustomerResetPasswordModel model);

        string UpdateAvatar(string username, string base64);

        Task<IEnumerable<CheckEditAppListByDTO>> GetEditListAppByUserIDByListCustomerId(long userid, long customerid);
    }
}