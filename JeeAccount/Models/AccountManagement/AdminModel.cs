using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class AdminModel
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}