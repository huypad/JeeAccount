using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class PersonalInfoCustomData : InfoUserBase
    {
        public string Birthday { get; set; }
        public string Phonenumber { get; set; }
        public override string Fullname { get; set; }
        public override string Name { get; set; }
        public override string Avatar { get; set; }
        public int JobtitleID { get; set; }
        public override string Jobtitle { get; set; }
        public override string Departmemt { get; set; }
        public int DepartmemtID { get; set; }
        public override string Email { get; set; }
        public override string StructureID { get; set; }
        public string Structure { get; set; }
        public string BgColor { get; set; } = "";
    }
}