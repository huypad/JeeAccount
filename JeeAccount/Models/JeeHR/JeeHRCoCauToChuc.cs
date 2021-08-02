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

    public class FlatJeeHRCoCauToChucModel
    {
        public int RowID { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public int Position { get; set; }

        public FlatJeeHRCoCauToChucModel(JeeHRCoCauToChuc item)
        {
            RowID = item.RowID;
            Title = item.Title;
            Level = item.Level;
            ParentID = item.ParentID;
            Position = item.Position;
        }
    }
}