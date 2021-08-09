using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class CheckEditAppListByDTO
    {
        public int AppID { get; set; }
        public string AppCode { get; set; }
        public string AppName { get; set; }
        public bool IsUsed { get; set; }
    }
}