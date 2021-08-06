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
        public string Level { get; set; }
        public string ParentID { get; set; }
        public string Position { get; set; }
        public JeeHRCoCauToChuc[] Children { get; set; }
    }

    public class FlatJeeHRCoCauToChucModel
    {
        public int RowID { get; set; }
        public string Title { get; set; }
        public string Level { get; set; }
        public string ParentID { get; set; }
        public string Position { get; set; }

        public FlatJeeHRCoCauToChucModel(JeeHRCoCauToChuc item)
        {
            RowID = item.RowID;
            Title = item.Title;
            Level = item.Level;
            ParentID = item.ParentID;
            Position = item.Position;
        }
    }

    public class JeeHRCoCauToChucModelFromDB
    {
        public int RowID { get; set; }
        public string Title { get; set; }
    }

    public class JeeHRChucVuToJeeHRFromDB
    {
        public int RowID { get; set; }
        public string Title { get; set; }
    }

    public class JeeHRChucVu
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

    public class JeeHRChucVuFromDB
    {
        public int RowID { get; set; }
        public string Title { get; set; }
    }
}