using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class AppAccount
    {
        public List<string> AppCode { get; set; }
        public long UserId { get; set; }
    }
}