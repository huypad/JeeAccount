﻿using DpsLibs.Data;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using System.Collections.Generic;


namespace JeeAccount.Reponsitories.CustomerManagement
{
    public interface ICustomerManagementReponsitory
    {
        IEnumerable<CustomerModelDTO> GetListCustomer();
        IEnumerable<CustomerModelDTO> GetListCustomer(string whereSrt, string orderByStr);
        IEnumerable<AppListDTO> GetListApp();
        bool checkTrungCode(string Code);
        ReturnSqlModel CreateCustomer(DpsConnection cnn, CustomerModel customerModel);
        long GetlastCustomerID(DpsConnection cnn);
        ReturnSqlModel CreateAppCode(DpsConnection cnn, CustomerModel customerModel, long CustomerID);
        List<string> AppCodes(DpsConnection cnn, long CustomerID);
    }
}