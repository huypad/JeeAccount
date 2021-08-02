using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class JeeHRPersonalInfo : UserNameDTO
    {
        public string Birthday { get; set; }
        public string Phonenumber { get; set; }
        public string Fullname { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
        public string BgColor { get; set; } = "";
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public long ManagerID { get; set; }
        public string ManagerUsername { get; set; }
        public string Note { get; set; }
    }
}