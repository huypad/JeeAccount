using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class UserNameDTO
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public long StaffID { get; set; }
    }
}