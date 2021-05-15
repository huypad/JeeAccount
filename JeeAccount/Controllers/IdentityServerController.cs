using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    public class IdentityServerController
    {
        private readonly static string LINK_IDENTITY_SERVER = "https://identityserver.jee.vn";
        private readonly static string LINK_LOGIN = LINK_IDENTITY_SERVER + "/user/login";
        private readonly static string LINK_INFO_USER_LOGGED = LINK_IDENTITY_SERVER + "/user/me";
        private readonly static string LINK_REFRESH_TOKEN = LINK_IDENTITY_SERVER + "/user/refresh";
        private readonly static string LINK_LOGOUT = LINK_IDENTITY_SERVER + "/user/logout";
        private readonly static string LINK_CHANGE_PASSWORD = LINK_IDENTITY_SERVER + "/user/changePassword";
        private readonly static string LINK_ADD_NEWUSER = LINK_IDENTITY_SERVER + "/user/addnew";
        private readonly static string LINK_UPDATE_CUSTOMDATA = LINK_IDENTITY_SERVER + "/user/updateCustomData";
        private readonly static string LINK_UPDATE_CUSTOMDATASELF = LINK_IDENTITY_SERVER + "/user/updateCustomData/self";
        private readonly static string LINK_CHANGE_USERSTATE = LINK_IDENTITY_SERVER + "/user/changeUserState";

        public async Task<AppAccount> loginUser(string username, string password)
        {
            string url = LINK_LOGIN;
            var content = new IdentityServerLogin
            {
                username = username,
                password = password,
            };
            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;

                    var logindata = JsonConvert.DeserializeObject<LoginObject>(returnValue);

                    var appcodes = logindata.User.CustomData.JeeAccount.AppCode;
                    var userid = logindata.User.CustomData.JeeAccount.UserID;
                    AccessRefreshToken accessRefreshToken = new AccessRefreshToken();
                    accessRefreshToken.access_token = logindata.Access_token;
                    accessRefreshToken.refresh_token = logindata.Refresh_token;

                    IdentityServerReturn identity = new IdentityServerReturn();
                    identity.data = accessRefreshToken;
                    var obj = new AppAccount
                    {
                        AppCode = appcodes,
                        UserId = userid
                    };
                    return obj;
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return new AppAccount();
                }
            }
        }

        public async Task<IdentityServerReturn> addNewUser(IdentityServerAddNewUser identityServerUserModel, string Admin_access_token)
        {
            string url = LINK_ADD_NEWUSER;
            var content = new IdentityServerAddNewUser
            {
                username = identityServerUserModel.username,
                password = identityServerUserModel.password,
                customData = identityServerUserModel.customData,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Admin_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return new IdentityServerReturn();
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }

        }

        public async Task<IdentityServerReturn> addNewAdminUser(IdentityServeAddAdminNewUser identityServerUserModel, string Admin_access_token)
        {
            string url = LINK_ADD_NEWUSER;
            var content = new IdentityServeAddAdminNewUser
            {
                username = identityServerUserModel.username,
                password = identityServerUserModel.password,
                customData = identityServerUserModel.customData,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Admin_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return new IdentityServerReturn();
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }

        }

        public async Task<IdentityServerReturn> changePassword(IdentityServerChangePasswordModel identityServerChangePasswordModel)
        {
            string url = LINK_CHANGE_PASSWORD;
            var content = new IdentityServerChangePassword
            {
                password_old = identityServerChangePasswordModel.password_old,
                password_new = identityServerChangePasswordModel.password_new,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(identityServerChangePasswordModel.USer_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return new IdentityServerReturn();
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }

        }

        public async Task<IdentityServerReturn> changeUserState(IdentityServerChangeUserStateModel identityServerChangeUserStateModel)
        {
            string url = LINK_CHANGE_USERSTATE;
            var content = new IdentityServerChangeUserState
            {
                userId = identityServerChangeUserStateModel.userId,
                disabled = identityServerChangeUserStateModel.disabled,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(identityServerChangeUserStateModel.Admin_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;

                    var logindata = JsonConvert.DeserializeObject<LoginObject>(returnValue);

                    AccessRefreshToken accessRefreshToken = new AccessRefreshToken();
                    accessRefreshToken.access_token = logindata.Access_token;
                    accessRefreshToken.refresh_token = logindata.Refresh_token;

                    IdentityServerReturn identity = new IdentityServerReturn();
                    identity.data = accessRefreshToken;
                    return identity;
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }

        }

        public async Task<IdentityServerReturn> updateCustomDataPersonalInfo(string Admin_access_token, PersonalInfoCustomData personalInfoCustom, string Username)
        {
            string url = LINK_UPDATE_CUSTOMDATA;
            var content = new UpdateCustomDataPersonInfoModel
            {
                username = Username,
                updateField = "personalInfo",
                fieldValue = personalInfoCustom
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Admin_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;

                    var logindata = JsonConvert.DeserializeObject<LoginObject>(returnValue);

                    AccessRefreshToken accessRefreshToken = new AccessRefreshToken();
                    accessRefreshToken.access_token = logindata.Access_token;
                    accessRefreshToken.refresh_token = logindata.Refresh_token;

                    IdentityServerReturn identity = new IdentityServerReturn();
                    identity.data = accessRefreshToken;
                    return identity;
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }

        }

        public async Task<IdentityServerReturn> UppdateCustomData(string Admin_access_token, string Username, ObjCustomData objCustomData)
        {
            string url = LINK_UPDATE_CUSTOMDATA;
            var content = new UpdateCustomDataModel
            {
                username = Username,
                updateField = objCustomData.updateField,
                fieldValue = objCustomData.fieldValue,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Admin_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;

                    var logindata = JsonConvert.DeserializeObject<LoginObject>(returnValue);

                    AccessRefreshToken accessRefreshToken = new AccessRefreshToken();
                    accessRefreshToken.access_token = logindata.Access_token;
                    accessRefreshToken.refresh_token = logindata.Refresh_token;

                    IdentityServerReturn identity = new IdentityServerReturn();
                    identity.data = accessRefreshToken;
                    return identity;
                }
                else
                {
                    string returnValue = reponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }

        }
    }
}
