using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.JeeHR
{
    public class JeeHRCoCauToChuc
    {
        public int RowID { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public int Position { get; set; }
        public JeeHRCoCauToChuc[] Children { get; set; }
    }
}