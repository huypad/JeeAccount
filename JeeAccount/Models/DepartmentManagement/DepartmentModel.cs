﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.DepartmentManagement
{
    public class DepartmentModel
    {
        public long RowID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentManager { get; set; }
        public List<string> ThanhVien { get; set; }
        public string Note { get; set; }
    }
}