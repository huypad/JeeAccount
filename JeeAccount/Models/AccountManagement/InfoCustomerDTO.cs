using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Models.AccountManagement
{
    public class InfoCustomerDTO
    {
        public string Code { get; set; }
        public string CompanyName { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public string TaxCode { get; set; }
        public string LogoImgURL { get; set; }

        public InfoCustomerDTO(string code, string companyName, string adress, string phone, string taxCode, string logoImgURL)
        {
            Code = code;
            CompanyName = companyName;
            Adress = adress;
            Phone = phone;
            TaxCode = taxCode;
            LogoImgURL = logoImgURL;
        }
    }
}
