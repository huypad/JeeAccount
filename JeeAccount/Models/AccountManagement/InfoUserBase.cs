namespace JeeAccount.Models.AccountManagement
{
    public abstract class InfoUserBase
    {
        public abstract string Fullname { get; set; }
        public abstract string Name { get; set; }
        public abstract string Avatar { get; set; }
        public abstract string Jobtitle { get; set; }
        public abstract string Departmemt { get; set; }
    }
}