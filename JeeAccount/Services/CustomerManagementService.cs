﻿using DPSinfra.Kafka;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.CustomerManagement;
using Microsoft.Extensions.Configuration;
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
        private readonly IProducer producer;
        private IAccountManagementReponsitory _accountManagementReponsitory;
        private readonly IConfiguration configuration;
        public CustomerManagementService(string connectionString, IProducer producer, IConfiguration configuration)
        {
            this.customerManagementReponsitory = new CustomerManagementReponsitory(connectionString);
            this.identityServerController = new IdentityServerController();
            this.ConnectionString = connectionString;
            this._accountManagementReponsitory = new AccountManagementReponsitory(connectionString);
            this.producer = producer;
            this.configuration = configuration;
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

        public string getSecretToken()
        {
            var secret = configuration.GetValue<string>("Jwt:internal_secret");
            var projectName = configuration.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }

        public async Task<IdentityServerReturn> CreateCustomer(CustomerModel customerModel)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                string TopicAddNewCustomer = configuration.GetValue<string>("KafkaTopic:TopicAddNewCustomer");
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    if (customerModel.RowID <= 0)
                    {
                        identityServerReturn.statusCode = -1;
                        identityServerReturn.message = "RowID không tồn tại";
                    }
                    cnn.BeginTransaction();
                    var create = customerManagementReponsitory.CreateCustomer(cnn, customerModel);
                    if (!create.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(create);
                        return identityServerReturn;
                    }


                    var createAppcodes = customerManagementReponsitory.CreateAppCode(cnn, customerModel, customerModel.RowID);
                    if (!createAppcodes.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(createAppcodes);
                        return identityServerReturn;
                    }

                    var appcodes = customerManagementReponsitory.AppCodes(cnn, customerModel.RowID);
                    AccountManagementModel accountManagementModel = new AccountManagementModel
                    {
                        AppCode = appcodes,
                        Email = customerModel.Email,
                        Fullname = customerModel.RegisterName,
                        Phonemumber = customerModel.Phone,
                        Username = customerModel.Username,
                        Password = customerModel.Password,
                    };

                    var createAccount = _accountManagementReponsitory.CreateAccount(cnn, accountManagementModel, 0, customerModel.RowID, true);
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
                                CustomerID = customerModel.RowID,
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
                    string token = getSecretToken();
                    var addNewUser = await identityServerController.addNewAdminUserInternal(identity, token);
                    if (addNewUser.statusCode != 0)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return addNewUser;
                    }
                    cnn.EndTransaction();
                    // kafak
                    var obj = new
                    {
                        CustomerID = identity.customData.JeeAccount.CustomerID,
                        AppCode = identity.customData.JeeAccount.AppCode,
                        UserID = identity.customData.JeeAccount.UserID,
                        Username = customerModel.Username,
                        IsInitial = true,
                        IsAdmin = true,
                    };
                    producer.PublishAsync(TopicAddNewCustomer, JsonConvert.SerializeObject(obj));
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