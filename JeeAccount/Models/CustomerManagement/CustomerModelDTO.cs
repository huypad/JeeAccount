using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.CustomerManagement
{
    public class CustomerModelDTO
    {
        public int RowID { get; set; }
        public string Code { get; set; }
        public string CompanyName { get; set; }
        public string RegisterName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string RegisterDate { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
    }
}
