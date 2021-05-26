using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.Mail;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace JeeAccount.Reponsitories.Mail
{
    public class MailReponsitory : IMailReponsitory
    {
        private readonly string _connectionString;

        public MailReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public MailModel InitialData(string CustemerID)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                return InitialData(CustemerID, cnn);
            }
        }

        private MailModel InitialData(string CustemerID, DpsConnection cnn)
        {
            var mailModel = new MailModel();
            DataTable dt = new DataTable();
            SqlConditions cond = new SqlConditions();
            cond.Add("RowID", CustemerID);
            dt = cnn.CreateDataTable("select SmtpClient, Port, EmailAddress, Password, EnableSSL, Username from CustomerList where (where)", "(where)", cond);
            if (dt.Rows.Count <= 0)
            {
                mailModel.Email = "";
                return mailModel;
            }
            else
            {
                int port = 0;
                if (!int.TryParse(dt.Rows[0]["Port"].ToString(), out port))
                {
                    port = 0;
                }
                mailModel.Email = dt.Rows[0]["EmailAddress"].ToString().Trim();
                mailModel.UserName = dt.Rows[0]["username"].ToString().Trim();
                mailModel.SmptClient = dt.Rows[0]["SmtpClient"].ToString().Trim();
                if (bool.TrueString.Equals(dt.Rows[0]["EnableSSL"].ToString()))
                    mailModel.EnableSSL = true;
                else mailModel.EnableSSL = false;
                mailModel.Port = port;
                try
                {
                    mailModel.Password = DpsLibs.Common.EncDec.Decrypt(dt.Rows[0]["Password"].ToString(), Constant.PASSWORD_ED);
                }
                catch { }
                if (("".Equals(mailModel.Email)) || ("".Equals(mailModel.UserName)) || ("".Equals(mailModel.SmptClient)) || (port <= 0))
                {
                    return InitDefaultData(cnn);
                }
                return mailModel;
            }
        }

        private MailModel InitDefaultData(DpsConnection cnn)
        {
            var mailModel = new MailModel();
            DataTable dt = new DataTable();
            SqlConditions cond = new SqlConditions();
            cond.Add("RowID", 0);
            dt = cnn.CreateDataTable("select SmtpClient, Port, EmailAddress, Password, EnableSSL, Username from CustomerList where (where)", "(where)", cond);
            if (dt.Rows.Count <= 0)
            {
                mailModel.Email = "";
                return mailModel;
            }
            else
            {
                int port = 0;
                if (!int.TryParse(dt.Rows[0]["Port"].ToString(), out port))
                {
                    port = 0;
                }
                mailModel.Email = dt.Rows[0]["EmailAddress"].ToString().Trim();
                mailModel.UserName = dt.Rows[0]["username"].ToString().Trim();
                mailModel.SmptClient = dt.Rows[0]["SmtpClient"].ToString().Trim();
                if (bool.TrueString.Equals(dt.Rows[0]["EnableSSL"].ToString()))
                    mailModel.EnableSSL = true;
                else mailModel.EnableSSL = false;
                mailModel.Port = port;
                try
                {
                    mailModel.Password = DpsLibs.Common.EncDec.Decrypt(dt.Rows[0]["Password"].ToString(), Constant.PASSWORD_ED);
                }
                catch { }
                return mailModel;
            }
        }
    }
}