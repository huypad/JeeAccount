using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.JeeHR
{
    public class ReturnJeeHR<T>
    {
        public int status { get; set; }
        public List<T> data { get; set; }
        public PageJeeHR page { get; set; }
        public ErrorJeeHR error { get; set; }
        public bool Visible { get; set; }
    }

    public class PageJeeHR
    {
        public int Page { get; set; }
        public int AllPage { get; set; }
        public int Size { get; set; }
        public int TotalCount { get; set; }
    }

    public class ErrorJeeHR
    {
        public string message { get; set; }
        public string code { get; set; }
        public string LastError { get; set; }
        public bool allowForce { get; set; }
    }
}