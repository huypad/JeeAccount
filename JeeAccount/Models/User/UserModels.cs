using JeeAccount.Models.AccountManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models
{
    public class UserModels
    {
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }

    public class LoginObject
    {
        [JsonProperty("access_token")]
        public string Access_token { get; set; }

        [JsonProperty("refresh_token")]
        public string Refresh_token { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    public class User
    {
        [JsonProperty("customData")]
        public CustomData CustomData { get; set; }

        [JsonProperty("disabled")]
        public bool Disable { get; set; }

        [JsonProperty("createdDate")]
        public string CreatedDate { get; set; }

        [JsonProperty("_id")]
        public string _Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("__v")]
        public string __V { get; set; }
    }

    public class CustomData
    {
        [JsonProperty("personalInfo")]
        public PersonalInfoCustomData PersonalInfo { get; set; }

        [JsonProperty("jee-account")]
        public JeeAccountModel JeeAccount { get; set; }
    }

    public class CustomAdminData
    {
        [JsonProperty("personalInfo")]
        public PersonalInfoCustomData PersonalInfo { get; set; }

        [JsonProperty("jee-account")]
        public JeeAccountModel JeeAccount { get; set; }

        [JsonProperty("identityServer")]
        public IdentityServer identityServer { get; set; }
    }

    public class IdentityServer
    {
        [JsonProperty("actions")]
        public List<string> actions { get; set; }
    }

    public class JeeAccountModel
    {
        [JsonProperty("customerID")]
        public long CustomerID { get; set; }

        [JsonProperty("appCode")]
        public List<string> AppCode { get; set; }

        [JsonProperty("userID")]
        public long UserID { get; set; }

        [JsonProperty("staffID")]
        public long StaffID { get; set; } = 0;
    }
}