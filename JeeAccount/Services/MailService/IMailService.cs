using JeeAccount.Models.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.MailService
{
    public interface IMailService
    {
        MailModel InitialData(string CustemerID);
    }
}