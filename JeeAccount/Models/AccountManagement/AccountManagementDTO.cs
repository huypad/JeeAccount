using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class AccountManagementDTO : AccUsernameModel
    {
        public int JobtitleID { get; set; }
        public int DepartmentID { get; set; }
        public bool IsActive { get; set; }
        public string DirectManagerUsername { get; set; }
        public long DirectManagerUserID { get; set; }
        public string DirectManager { get; set; }
        public bool IsAdmin { get; set; }
        public string Note { get; set; }
    }
}