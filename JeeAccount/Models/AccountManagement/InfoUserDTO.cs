using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class InfoUserDTO : InfoUserBase
    {
        public override string Fullname { get; set; }
        public override string Name { get; set; }
        public override string Avatar { get; set; }
        public override string Jobtitle { get; set; }
        public override string Departmemt { get; set; }
        public override string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public override string StructureID { get; set; }
        public string ChucVuID { get; set; }
    }
}