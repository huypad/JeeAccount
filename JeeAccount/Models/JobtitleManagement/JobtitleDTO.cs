using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.JobtitleManagement
{
    public class JobtitleDTO
    {
        public long RowID { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
    }
}