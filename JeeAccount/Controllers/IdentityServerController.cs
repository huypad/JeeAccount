using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Services.AccountManagementService;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    public class IdentityServerController
    {
        private const string LINK_IDENTITY_SERVER = "https://identityserver.jee.vn";
        private const string LINK_LOGIN = LINK_IDENTITY_SERVER + "/user/login";
        private const string LINK_INFO_USER_LOGGED = LINK_IDENTITY_SERVER + "/user/me";
        private const string LINK_REFRESH_TOKEN = LINK_IDENTITY_SERVER + "/user/refresh";
        private const string LINK_LOGOUT = LINK_IDENTITY_SERVER + "/user/logout";
        private const string LINK_CHANGE_PASSWORD = LINK_IDENTITY_SERVER + "/user/changePassword";
        private const string LINK_ADD_NEWUSER = LINK_IDENTITY_SERVER + "/user/addnew";
        private const string LINK_ADD_NEWUSER_INTERNAL = LINK_IDENTITY_SERVER + "/user/addnew/internal";
        private const string LINK_UPDATE_CUSTOMDATA = LINK_IDENTITY_SERVER + "/user/updateCustomData";
        private const string LINK_UPDATE_CUSTOMDATASELF = LINK_IDENTITY_SERVER + "/user/updateCustomData/self";
        private const string LINK_CHANGE_USERSTATE = LINK_IDENTITY_SERVER + "/user/changeUserState";
        private const string LINK_CHANGEPASSWORD_INTERNAL = LINK_IDENTITY_SERVER + "/user/changePassword/internal";
        private const string LINK_RESETPASSWORD = LINK_IDENTITY_SERVER + "/user/resetPassword";

        public async Task<HttpResponseMessage> addNewUser(IdentityServerAddNewUser identityServerUserModel, string Admin_access_token)
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
                return reponse;
            }
        }

        public async Task<IdentityServerReturn> addNewAdminUserInternal(IdentityServeAddAdminNewUser identityServerUserModel, string internal_token)
        {
            string url = LINK_ADD_NEWUSER_INTERNAL;
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internal_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    return new IdentityServerReturn();
                }
                else
                {
                    string returnValue = await reponse.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }
        }

        public async Task<object> ChangePasswordAsyncInternal(string internal_token, ChangePasswordModel model)
        {
            try
            {
                string url = LINK_CHANGEPASSWORD_INTERNAL;
                var content = new
                {
                    username = model.Username,
                    password_old = model.PasswordOld,
                    password_new = model.PaswordNew,
                };

                var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
                var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internal_token);
                    var reponse = await client.PostAsync(url, httpContent);
                    string returnValue = await reponse.Content.ReadAsStringAsync();
                    if (reponse.IsSuccessStatusCode)
                    {
                        return returnValue;
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                        throw new Exception(res.message);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<HttpResponseMessage> resetPasswordInternal(string internalToken, string username)
        {
            string url = LINK_RESETPASSWORD;
            var content = new
            {
                username = username
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internalToken);

                var reponse = await client.PostAsync(url, httpContent);

                return reponse;
            }
        }

        public async Task<IdentityServerReturn> changeUserState(IdentityServerChangeUserStateModel identityServerChangeUserStateModel)
        {
            string url = LINK_CHANGE_USERSTATE;
            var content = new IdentityServerChangeUserState
            {
                username = identityServerChangeUserStateModel.username,
                disabled = identityServerChangeUserStateModel.disabled,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(identityServerChangeUserStateModel.Admin_access_token_or_internal_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    string returnValue = await reponse.Content.ReadAsStringAsync();

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
                    string returnValue = await reponse.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }
        }

        public async Task<HttpResponseMessage> changeUserStateAsync(string Admin_access_token, string username, bool disable)
        {
            string url = LINK_CHANGE_USERSTATE;
            var content = new
            {
                username = username,
                disabled = disable,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Admin_access_token);

                var reponse = await client.PostAsync(url, httpContent);
                return reponse;
            }
        }

        public async Task<HttpResponseMessage> changeUserStateInternalAsync(IdentityServerChangeUserStateModel identityServerChangeUserStateModel)
        {
            string url = LINK_CHANGE_USERSTATE + "/internal";
            var content = new IdentityServerChangeUserState
            {
                username = identityServerChangeUserStateModel.username,
                disabled = identityServerChangeUserStateModel.disabled,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(identityServerChangeUserStateModel.Admin_access_token_or_internal_token);

                return await client.PostAsync(url, httpContent);
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
                    string returnValue = await reponse.Content.ReadAsStringAsync();

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
                    string returnValue = await reponse.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }
        }

        public async Task<HttpResponseMessage> updateCustomDataPersonalInfoInternal(string internal_token, PersonalInfoCustomData personalInfoCustom, string Username)
        {
            string url = LINK_UPDATE_CUSTOMDATA + "/internal";
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internal_token);

                var reponse = await client.PostAsync(url, httpContent);
                return reponse;
            }
        }

        public async Task<HttpResponseMessage> UppdateCustomDataHttpResponse(string Admin_access_token, string Username, ObjCustomData objCustomData)
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
                return reponse;
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
                    string returnValue = await reponse.Content.ReadAsStringAsync();

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
                    string returnValue = await reponse.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }
        }

        public async Task<IdentityServerReturn> UppdateCustomDataInternal(string internal_token, string Username, ObjCustomData objCustomData)
        {
            string url = LINK_UPDATE_CUSTOMDATA + "/internal";
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internal_token);

                var reponse = await client.PostAsync(url, httpContent);
                if (reponse.IsSuccessStatusCode)
                {
                    string returnValue = await reponse.Content.ReadAsStringAsync();

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
                    string returnValue = await reponse.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<IdentityServerReturn>(returnValue);
                    return res;
                }
            }
        }

        public async Task<HttpResponseMessage> UpdateCustomDataInternal(string internal_token, string Username, ObjCustomData objCustomData)
        {
            string url = LINK_UPDATE_CUSTOMDATA + "/internal";
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internal_token);

                var reponse = await client.PostAsync(url, httpContent);
                return reponse;
            }
        }

        public async Task<HttpResponseMessage> ResetPasswordRootCustomer(string internal_token, string username, CustomerResetPasswordModel model)
        {
            string url = LINK_CHANGEPASSWORD_INTERNAL;
            var content = new
            {
                username = username,
                password_old = model.OldPassword,
                password_new = model.NewPassword,
            };

            var stringContent = await Task.Run(() => JsonConvert.SerializeObject(content));
            var httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(internal_token);

                var reponse = await client.PostAsync(url, httpContent);
                return reponse;
            }
        }

        public ObjectLogin ObjectLogin(string username, string password)
        {
            string url = LINK_LOGIN;
            var content = new
            {
                username = username,
                password = password
            };

            var stringContent = JsonConvert.SerializeObject(content);

            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);

            request.AddJsonBody(stringContent);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<ObjectLogin>(response.Content);
        }

        public ObjectUserme ObjectUserme(string access_token)
        {
            var url = LINK_INFO_USER_LOGGED;
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", access_token);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<ObjectUserme>(response.Content);
        }

        public IRestResponse AddNewUserInternalNotAysnc(IdentityServerAddNewUser identityServerUserModel, string internal_token)
        {
            string url = LINK_ADD_NEWUSER_INTERNAL;
            var content = new
            {
                username = identityServerUserModel.username,
                password = identityServerUserModel.password,
                customData = identityServerUserModel.customData,
            };

            var stringContent = JsonConvert.SerializeObject(content);

            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);

            request.AddJsonBody(stringContent);

            request.AddHeader("Authorization", internal_token);

            return client.Execute(request);
        }

        public ObjCustomData JeeAccountCustomData(List<string> appCodes, long UserID, long CustomerID, long StaffID)
        {
            try
            {
                if (appCodes.Count == 0) throw new Exception("List AppCode không có dữ liệu");
                ObjCustomData obj = new ObjCustomData();
                obj.fieldValue = new
                {
                    customerID = CustomerID,
                    appCode = appCodes,
                    userID = UserID,
                    staffID = StaffID
                };
                obj.updateField = "jee-account";
                obj.userId = UserID;
                return obj;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}