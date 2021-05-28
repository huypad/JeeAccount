using JeeAccount.Models.Mail;
using JeeAccount.Reponsitories.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class MailService
    {
        private IMailReponsitory mailReponsitory;

        public MailService(string connectionString)
        {
            this.mailReponsitory = new MailReponsitory(connectionString);
        }

        public MailModel InitialData (string CustemerID)
        {
            return mailReponsitory.InitialData(CustemerID);
        }

    }
}
