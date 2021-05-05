using JeeAccount.Models.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.Mail
{
    public interface IMailReponsitory
    {
        MailModel InitialData(string CustemerID);
    }
}
