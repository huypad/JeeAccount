using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class CustomerManagementService
    {
        private ICustomerManagementReponsitory customerManagementReponsitory;
        private IdentityServerController identityServerController;
        private readonly string ConnectionString;
        private IAccountManagementReponsitory _accountManagementReponsitory;
        public CustomerManagementService(string connectionString)
        {
            this.customerManagementReponsitory = new CustomerManagementReponsitory(connectionString);
            this.identityServerController = new IdentityServerController();
            this.ConnectionString = connectionString;
            this._accountManagementReponsitory = new AccountManagementReponsitory(connectionString);
        }
        public IEnumerable<CustomerModelDTO> GetListCustomer()
        {
            var list = customerManagementReponsitory.GetListCustomer();
            return list;
        }

        public IEnumerable<CustomerModelDTO> GetListCustomer(string whereSrt, string orderByStr)
        {
            var list = customerManagementReponsitory.GetListCustomer(whereSrt, orderByStr);
            return list;
        }

        public IEnumerable<AppListDTO> GetListApp()
        {
            var list = customerManagementReponsitory.GetListApp();
            return list;
        }

        public bool checkTrungCode(string Code)
        {
            return customerManagementReponsitory.checkTrungCode(Code);
        }

        public async Task<IdentityServerReturn> CreateCustomer(string Admin_accessToken, CustomerModel customerModel)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
                    string username = customerModel.Code + ".admin";
                    string password = GeneralService.RandomString(8);
                    var create = customerManagementReponsitory.CreateCustomer(cnn, customerModel);
                    if (!create.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(create);
                        return identityServerReturn;
                    }

                    long customerId = customerManagementReponsitory.GetlastCustomerID(cnn);

                    var createAppcodes = customerManagementReponsitory.CreateAppCode(cnn, customerModel, customerId);
                    if (!createAppcodes.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(createAppcodes);
                        return identityServerReturn;
                    }

                    var appcodes = customerManagementReponsitory.AppCodes(cnn, customerId);
                    AccountManagementModel accountManagementModel = new AccountManagementModel
                    {
                        AppCode = appcodes,
                        Email = customerModel.Email,
                        Fullname = customerModel.RegisterName,
                        Phonemumber = customerModel.Phone,
                        Username = username,
                        Password = password,
                    };

                    var createAccount = _accountManagementReponsitory.CreateAccount(cnn, accountManagementModel, 0, customerId);
                    if (!createAccount.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(createAccount);
                        return identityServerReturn;
                    }

                    long userId = _accountManagementReponsitory.GetLastUserID(cnn);
                    IdentityServerAddNewUser identity = new IdentityServerAddNewUser
                    {
                        username = username,
                        password = password,
                        customData = new CustomData
                        {
                            JeeAccount = new JeeAccountModel
                            {
                                AppCode = appcodes,
                                CustomerID = customerId,
                                UserID = userId,
                            },
                            PersonalInfo = new PersonalInfoCustomData
                            {
                                Email = customerModel.Email,
                                Fullname = customerModel.RegisterName,
                                Name = GeneralService.getFirstname(customerModel.RegisterName),
                                Avatar = "",
                                Birthday = "",
                                Departmemt = "",
                                Jobtitle = "",
                                Phonenumber = customerModel.Phone
                            }
                        }
                    };
                    var addNewUser = await identityServerController.addNewUser(identity, Admin_accessToken);
                    if (addNewUser.statusCode != 0)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return addNewUser;
                    }
                    cnn.EndTransaction();
                    return addNewUser;
                }
                catch (Exception ex)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    identityServerReturn = GeneralService.TranformIdentityServerException(ex);
                    return identityServerReturn;
                }
            }
        }

    }
}
