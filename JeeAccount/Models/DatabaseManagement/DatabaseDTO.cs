using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.DatabaseManagement
{
    public class DatabaseDTO
    {
        public int RowID { get; set; }
        public string AppCode { get; set; }
        public int AppID { get; set; }
        public string Title { get; set; }
    }
}