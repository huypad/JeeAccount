using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class InsertAppListAccountModel
    {
        public List<string> AppCode { get; set; }
        public List<int> AppId { get; set; }
        public long CustomerId { get; set; }
        public long UserId { get; set; }
    }
}