using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.Common
{
    public class CommonInfo
    {
        public long UserID { get; set; }
        public string Username { get; set; }
        public long StaffID { get; set; }
        public long CustomerID { get; set; }
    }
}