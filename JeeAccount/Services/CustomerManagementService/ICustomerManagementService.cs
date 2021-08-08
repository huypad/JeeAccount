using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JeeAccount.Services.CustomerManagementService
{
    public interface ICustomerManagementService
    {
        IEnumerable<CustomerModelDTO> GetListCustomer();

        IEnumerable<CustomerModelDTO> GetListCustomer(string whereSrt, string orderByStr);

        IEnumerable<AppListDTO> GetListApp();

        bool checkTrungCode(string Code);

        Task<IdentityServerReturn> CreateCustomer(CustomerModel customerModel, string usernameAdmin);

        Task<ReturnSqlModel> UpdateCustomerAppGiaHanModel(CustomerAppGiaHanModel model);

        Task<ReturnSqlModel> UpdateCustomerAppAddNumberStaff(CustomerAppAddNumberStaffModel model);

        Task<string> LockUnLockCustomer(long customerid, bool state);

        string CompanyCode(long customerid);
    }
}