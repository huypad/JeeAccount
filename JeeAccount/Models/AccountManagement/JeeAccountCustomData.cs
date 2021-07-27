using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class JeeAccountCustomData
    {
        public long staffID { get; set; }
        public long userID { get; set; }
        public long customerID { get; set; }
        public List<string> appCode { get; set; } = new List<string>();
    }
}