using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.JobtitleManagement
{
    public class JobtitleModel
    {
        public long RowID { get; set; }
        public string JobtitleName { get; set; }
        public List<string> ThanhVien { get; set; }
        public List<string> ThanhVienDelete { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
    }
}