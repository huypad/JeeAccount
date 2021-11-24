using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class AccountManagementModel
    {
        public string Fullname { get; set; }
        public string ImageAvatar { get; set; } = "";
        public string Email { get; set; }
        public string Departmemt { get; set; }
        public int DepartmemtID { get; set; } = 0;
        public string Jobtitle { get; set; }
        public int JobtitleID { get; set; } = 0;
        public string Username { get; set; }
        public string Phonemumber { get; set; }
        public string Password { get; set; }
        public string Birthday { get; set; }
        public List<string> AppCode { get; set; }
        public List<int> AppID { get; set; }

        public int cocauid { get; set; } = 0;
        public int chucvuid { get; set; } = 0;
        public long StaffID { get; set; } = 0;
        public long Userid { get; set; }
        public string DirectManager { get; set; } = "";
    }
}