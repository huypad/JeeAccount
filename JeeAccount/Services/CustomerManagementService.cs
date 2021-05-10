using DPSinfra.Kafka;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.CustomerManagement;
using Newtonsoft.Json;
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

        public async Task<IdentityServerReturn> CreateCustomer(string Admin_accessToken, CustomerModel customerModel, IProducer producer, string TopicAddNewCustomer)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    cnn.BeginTransaction();
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
                        Username = customerModel.Username,
                        Password = customerModel.Password,
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
                    IdentityServeAddAdminNewUser identity = new IdentityServeAddAdminNewUser
                    {
                        username = customerModel.Username,
                        password = customerModel.Password,
                        customData = new CustomAdminData
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
                            },
                            identityServer = new IdentityServer
                            {
                                actions = new List<string>() { "create_new_user", "update_custom_data", "change_user_state" }
                            } 
                        }
                    };
                    var addNewUser = await identityServerController.addNewAdminUser(identity, Admin_accessToken);
                    if (addNewUser.statusCode != 0)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return addNewUser;
                    }
                    // kafak
                    producer.PublishAsync("helloitme", JsonConvert.SerializeObject(identity.customData.JeeAccount));
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
