using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Classes
{
    [Serializable]
    public class JeeHRException : Exception
    {
        public ErrorJeeHR JeeHRError;

        public JeeHRException() : base()
        {
        }

        public JeeHRException(string message) : base(message)
        {
        }

        public JeeHRException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public JeeHRException(ErrorJeeHR error)
        {
            JeeHRError = error;
        }
    }
}