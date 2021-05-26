using JeeAccount.Models.Mail;
using JeeAccount.Reponsitories.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.MailService
{
    public class MailService : IMailService
    {
        private IMailReponsitory mailReponsitory;

        public MailService(IMailReponsitory mailReponsitory)
        {
            this.mailReponsitory = mailReponsitory;
        }

        public MailModel InitialData(string CustemerID)
        {
            return mailReponsitory.InitialData(CustemerID);
        }
    }
}