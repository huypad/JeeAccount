using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class AccUsernameModel
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Jobtitle { get; set; }
        public string Department { get; set; }
        public string AvartarImgURL { get; set; }
        public string PhoneNumber { get; set; }
        public long CustomerID { get; set; }
        public string Email { get; set; }
        public string StructureID { get; set; } // sử dụng cho JeeOffice

    }
}
