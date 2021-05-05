namespace JeeAccount.Models.Mail
{
    public class MailModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string SmptClient { get; set; }
        public string Password { get; set; }
        public bool EnableSSL { get; set; }
        public int Port { get; set; }
    }
}
