using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.DatabaseManagement
{
    public class DBTokenModel
    {
        public long customerID { get; set; }
        public string appCode { get; set; }
    }
}
