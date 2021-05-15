using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.ModelKafka
{
    public class InitalAccountModel
    {
        public long CustomerID { get; set; }
        public List<string> AppCode { get; set; }
        public long UserID { get; set; }
        public string Username { get; set; }
        public bool IsInitial { get; set; }
        public bool IsAdmin { get; set; }
    }
}
