using DPSinfra.Kafka;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.CustomerManagement;
using JeeAccount.Services.AccountManagementService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JeeAccount.Services.CustomerManagementService
{
    public class CustomerManagementService : ICustomerManagementService
    {
        private ICustomerManagementReponsitory _customerManagementReponsitory;
        private IdentityServerController identityServerController;
        private string ConnectionString;
        private IProducer producer;
        private IAccountManagementReponsitory _accountManagementReponsitory;
        private IConfiguration configuration;
        private IAccountManagementService _accountManagementService;

        public CustomerManagementService(IProducer producer, IConfiguration configuration, ICustomerManagementReponsitory customerManagementReponsitory, IAccountManagementReponsitory accountManagementReponsitory, IAccountManagementService accountManagementService)
        {
            this._customerManagementReponsitory = customerManagementReponsitory;
            this.identityServerController = new IdentityServerController();
            ConnectionString = configuration.GetValue<string>("AppConfig:Connection");
            this._accountManagementReponsitory = accountManagementReponsitory;
            this.producer = producer;
            this.configuration = configuration;
            _accountManagementService = accountManagementService;
        }

        public IEnumerable<CustomerModelDTO> GetListCustomer()
        {
            var list = _customerManagementReponsitory.GetListCustomer();
            return list;
        }

        public IEnumerable<CustomerModelDTO> GetListCustomer(string whereSrt, string orderByStr)
        {
            var list = _customerManagementReponsitory.GetListCustomer(whereSrt, orderByStr);
            return list;
        }

        public IEnumerable<AppListDTO> GetListApp()
        {
            var list = _customerManagementReponsitory.GetListApp();
            return list;
        }

        public bool checkTrungCode(string Code)
        {
            return _customerManagementReponsitory.checkTrungCode(Code);
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
                string TopicAddNewCustomer = configuration.GetValue<string>("KafkaConfig:TopicProduce:JeeplatformInitialization");
                IdentityServerReturn identityServerReturn = new IdentityServerReturn();
                try
                {
                    if (customerModel.RowID <= 0)
                    {
                        identityServerReturn.statusCode = -1;
                        identityServerReturn.message = "RowID không tồn tại";
                        return identityServerReturn;
                    }
                    cnn.BeginTransaction();
                    var create = _customerManagementReponsitory.CreateCustomer(cnn, customerModel);
                    if (!create.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(create);
                        return identityServerReturn;
                    }

                    var createAppcodes = _customerManagementReponsitory.CreateAppCode(cnn, customerModel, customerModel.RowID);
                    if (!createAppcodes.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        identityServerReturn = GeneralService.TranformIdentityServerReturnSqlModel(createAppcodes);
                        return identityServerReturn;
                    }

                    var appcodes = _customerManagementReponsitory.AppCodes(cnn, customerModel.RowID);
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
                    // kafak

                    var index = appcodes.FindIndex(code => code.Equals("jeehr", StringComparison.OrdinalIgnoreCase));
                    if (index > -1)
                    {
                        var objHR = new
                        {
                            CustomerID = identity.customData.JeeAccount.CustomerID,
                            AppCode = identity.customData.JeeAccount.AppCode,
                            UserID = identity.customData.JeeAccount.UserID,
                            Username = customerModel.Username,
                            IsInitial = true,
                            IsAdmin = true,
                            customerModel = new
                            {
                                Code = customerModel.Code,
                                RegisterDate = string.IsNullOrEmpty(customerModel.RegisterDate) ? customerModel.RegisterDate : "",
                                DeadlineDate = customerModel.DeadlineDate,
                                PakageID = customerModel.GoiSuDung[index],
                                CompanyName = customerModel.CompanyName,
                                Address = customerModel.Address,
                                Phone = customerModel.Phone,
                                Note = customerModel.Note,
                                Nguoidaidien = customerModel.RegisterName,
                                CustomerID = identity.customData.JeeAccount.CustomerID,
                                UsernameAdmin = customerModel.Username,
                                PasswordAdmin = customerModel.Password,
                                Email = customerModel.Email
                            }
                        };

                        await producer.PublishAsync(TopicAddNewCustomer, JsonConvert.SerializeObject(objHR));
                    }
                    var obj = new
                    {
                        CustomerID = identity.customData.JeeAccount.CustomerID,
                        AppCode = identity.customData.JeeAccount.AppCode,
                        UserID = identity.customData.JeeAccount.UserID,
                        Username = customerModel.Username,
                        IsInitial = true,
                        IsAdmin = true
                    };


                    await producer.PublishAsync(TopicAddNewCustomer, JsonConvert.SerializeObject(obj));
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

        public async Task<ReturnSqlModel> UpdateCustomerAppGiaHanModel(CustomerAppGiaHanModel model)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                var update = await _customerManagementReponsitory.UpdateCustomerAppGiaHanModelCnn(model, cnn);
                if (!update.Susscess)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                }
                return await Task.FromResult(update);
            }
        }

        public async Task<ReturnSqlModel> UpdateCustomerAppAddNumberStaff(CustomerAppAddNumberStaffModel model)
        {
            return await _customerManagementReponsitory.UpdateCustomerAppAddNumberStaff(model);
        }

        public async Task<string> LockUnLockCustomer(long customerid, bool state)
        {
            using (DpsConnection cnn = new DpsConnection(ConnectionString))
            {
                var messageError = "";
                var internal_token = getInternalJWToken();
                var lstUsername = await _accountManagementService.GetListJustUsernameByCustormerID(customerid);
                cnn.BeginTransaction();
                var valueState = 1;
                if (!state) valueState = 0;
                UpdateLockUnLockCustomerCnn(cnn, customerid, valueState);
                foreach (var username in lstUsername)
                {
                    var changeModel = new IdentityServerChangeUserStateModel();
                    changeModel.Admin_access_token_or_internal_token = internal_token;
                    changeModel.username = username;
                    changeModel.disabled = state;

                    var response = await identityServerController.changeUserStateInternalAsync(changeModel);
                    if (!response.IsSuccessStatusCode)
                    {
                        var returnValue = response.Content.ReadAsStringAsync().Result;
                        var obj = JsonConvert.DeserializeObject<IdentityServerReturnParse>(returnValue);
                        if (string.IsNullOrEmpty(messageError))
                        {
                            messageError = $"{username} ({obj.message})";
                        }
                        else
                        {
                            messageError += $" , {username} ({obj.message})";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(messageError))
                {
                    cnn.RollbackTransaction();
                }
                cnn.EndTransaction();
                return messageError;
            }
        }

        private void UpdateLockUnLockCustomerCnn(DpsConnection cnn, long customerid, int Status)
        {
            Hashtable val = new Hashtable();
            val.Add("Status", Status);
            SqlConditions conds = new SqlConditions();
            conds.Add("RowID", customerid);
            int x = cnn.Update(val, conds, "CustomerList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }

        private string getInternalJWToken()
        {
            var secret = configuration.GetValue<string>("Jwt:internal_secret");
            var projectName = configuration.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }
    }
}