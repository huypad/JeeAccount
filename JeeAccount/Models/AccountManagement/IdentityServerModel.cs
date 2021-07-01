using JeeAccount.Models.AccountManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models
{
    public class IdentityServerModel
    {
    }

    public class IdentityServerUserModel
    {
        public string Admin_access_token { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public object customData { get; set; }
    }

    public class IdentityServerChangePasswordModel
    {
        public string USer_access_token { get; set; }
        public string password_old { get; set; }
        public string password_new { get; set; }
    }

    public class IdentityServerChangeUserStateModel
    {
        public string Admin_access_token { get; set; }
        public string userId { get; set; }
        public bool disabled { get; set; }
    }

    public class IdentityServerAddNewUser
    {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("customData")]
        public CustomData customData { get; set; }
    }

    public class IdentityServeAddAdminNewUser
    {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("customData")]
        public CustomAdminData customData { get; set; }
    }

    public class IdentityServerLogin
    {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }
    }

    public class IdentityServerChangePassword
    {
        [JsonProperty("password_old")]
        public string password_old { get; set; }

        [JsonProperty("password_new")]
        public string password_new { get; set; }
    }

    public class IdentityServerChangeUserState
    {
        [JsonProperty("userId")]
        public string userId { get; set; }

        [JsonProperty("disabled")]
        public bool disabled { get; set; }
    }

    public class IdentityServerReturn
    {
        public int statusCode { get; set; } = 0;
        public string message { get; set; }
        public AccessRefreshToken data { get; set; }

        public IdentityServerReturn(string Access, string Refresh)
        {
            AccessRefreshToken item = new AccessRefreshToken();
            item.access_token = Access;
            item.refresh_token = Refresh;

            this.data = item;
            this.statusCode = 0;
            this.message = "success";
        }

        public IdentityServerReturn()
        {
            this.statusCode = 0;
            this.message = "success";
            this.data = null;
        }
    }

    public class UpdateCustomDataPersonInfoModel
    {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("updateField")]
        public string updateField { get; set; }

        [JsonProperty("fieldValue")]
        public PersonalInfoCustomData fieldValue { get; set; }
    }

    public class UpdateCustomDataModel
    {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("updateField")]
        public string updateField { get; set; }

        [JsonProperty("fieldValue")]
        public object fieldValue { get; set; }
    }

    public class AccessRefreshToken
    {
        public string access_token { get; set; }

        public string refresh_token { get; set; }
    }

    public class ObjCustomData
    {
        [JsonProperty("userId")]
        public long userId { get; set; }

        [JsonProperty("updateField")]
        public string updateField { get; set; }

        [JsonProperty("fieldValue")]
        public object fieldValue { get; set; }
    }
}