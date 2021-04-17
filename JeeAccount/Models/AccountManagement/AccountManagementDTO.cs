using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class AccountManagementDTO : InfoUserBase
    {
        public override string Fullname { get ; set ; }
        public override string Name { get ; set ; }
        public override string Avatar { get ; set ; }
        public override string Jobtitle { get ; set ; }
        public override string Departmemt { get ; set ; }
        public string Username { get; set; }

        public string DirectManager { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
    }
}
