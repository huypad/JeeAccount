using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using JeeAccount.Services.AccountManagementService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JeeCustomer.ConsumerKafka
{
    public class AccountConsumerController : IHostedService
    {
        private IConfiguration _config;
        private Consumer accountConsumer;
        private IProducer _producer;
        private readonly ILogger<AccountConsumerController> _logger;
        private readonly string _connectionString;
        private readonly string HOST_JEEHR_API;
        private readonly string HOST_MINIOSERVER;
        private readonly IAccountManagementReponsitory _accountReponsitory;
        private readonly string TopicAddNewCustomerUser;

        public AccountConsumerController(IConfiguration config, IProducer producer, ILogger<AccountConsumerController> logger, IAccountManagementReponsitory reponsitory)
        {
            _config = config;
            accountConsumer = new Consumer(_config, _config.GetValue<string>("KafkaConfig:Consumer:JeeAccountGroup"));
            _producer = producer;
            _logger = logger;
            _connectionString = _config.GetValue<string>("AppConfig:Connection");
            HOST_JEEHR_API = _config.GetValue<string>("Host:JeeHR_API");
            HOST_MINIOSERVER = _config.GetValue<string>("MinioConfig:MinioServer");
            _accountReponsitory = reponsitory;
            TopicAddNewCustomerUser = _config.GetValue<string>("KafkaConfig:TopicProduce:JeeplatformInitialization");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
            {
                accountConsumer.SubscribeAsync(_config.GetValue<string>("KafkaConfig:TopicConsume:JeeplatformInitializationAppupdate"), GetValue);
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await accountConsumer.closeAsync();
        }

        public void GetValue(string value)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjCustomData>(value);
                var traceLog = new GeneralLog()
                {
                    name = obj.updateField,
                    data = value,
                    message = _config.GetValue<string>("KafkaConfig:TopicConsume:JeeplatformInitializationAppupdate")
                };
                _logger.LogTrace(JsonConvert.SerializeObject(traceLog));

                string username = GeneralReponsitory.GetObjectDB($"select Username from AccountList where UserID = {obj.userId}", _connectionString);
                if (!string.IsNullOrEmpty(username))
                {
                    var identity = new IdentityServerController();
                    _ = identity.UppdateCustomDataInternal(GeneralService.GetInternalToken(_config), username, obj);
                }
                if (obj.updateField.Equals("jee-hr", StringComparison.OrdinalIgnoreCase))
                {
                    var objJeeHR = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjCustomJeeHRDataRoles>(value);

                    if (objJeeHR.fieldValue != null)
                    {
                        if (objJeeHR.fieldValue.staffID > 0)
                        {
                            GeneralReponsitory.SaveStaffID(obj.userId, objJeeHR.fieldValue.staffID, _connectionString);
                        }

                        if (string.IsNullOrEmpty(objJeeHR.fieldValue.roles))
                        {
                            GeneralReponsitory.RemoveAppCodeJeeHRKafka(_connectionString, obj.userId);
                        }
                        else
                        {
                            GeneralReponsitory.InsertAppCodeJeeHRKafka(_connectionString, obj.userId);
                        }

                        _ = GeneralReponsitory.UpdateJeeAccountCustomDataByInputApiModel(obj.userId, _connectionString, GeneralService.GetInternalToken(_config));
                    }

                    string _password = GeneralReponsitory.GetObjectDB($"select Password from AccountList where UserID = {obj.userId}", _connectionString).Trim();
                    if (!string.IsNullOrEmpty(_password))
                    {
                        var d5 = new GeneralLog()
                        {
                            name = "vô kafka và đang import jeehr",
                            data = $"userid = {obj.userId}",
                            message = $"vô kafka và đang import jeehr"
                        };
                        _logger.LogTrace(JsonConvert.SerializeObject(d5));

                        var commonInfo = GeneralReponsitory.GetCommonInfo(_connectionString, obj.userId);
                        string _username = commonInfo.Username;

                        GeneralReponsitory.RemovePassword(_connectionString, obj.userId);

                        var identity = new IdentityServerController();
                        var jeeHRController = new JeeHRController(_config.GetValue<string>("Host:JeeHR_API"));
                        var data = identity.ObjectLogin(_username, _password);
                        var data_userme = identity.ObjectUserme(data.access_token);
                        var jeehr = new JeeHRController(HOST_JEEHR_API);
                        var ds = jeehr.GetDSNhanVienNotAysnc(data_userme.access_token);

                        var lstApp = _accountReponsitory.GetListAppByCustomerID(commonInfo.CustomerID);
                        var appCodes = lstApp.Select(item => item.AppCode).ToList();
                        var appInts = lstApp.Select(item => item.AppID).ToList();

                        var internal_token = GeneralService.GetInternalToken(_config);
                        if (ds != null)
                        {
                            foreach (var item in ds)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(item.username))
                                    {
                                        var d6 = new GeneralLog()
                                        {
                                            name = "import jeehr thất bại",
                                            data = $"{item.username} ({commonInfo.CustomerID})",
                                            message = $"username bị null"
                                        };
                                        _logger.LogError(JsonConvert.SerializeObject(d6));
                                    }
                                    var account = new AccountManagementModel();
                                    account.Username = item.username;
                                    account.StaffID = item.IDNV;
                                    account.AppCode = appCodes;
                                    account.AppID = appInts;
                                    account.Birthday = item.NgaySinh;
                                    account.chucvuid = 33;
                                    account.cocauid = 1;
                                    account.JobtitleID = Int32.Parse(item.jobtitleid.ToString());
                                    account.Jobtitle = item.TenChucVu;
                                    account.Departmemt = item.Structure;
                                    account.DepartmemtID = Int32.Parse(item.structureid.ToString());
                                    account.Email = item.Email;
                                    account.Fullname = item.HoTen;
                                    account.ImageAvatar = item.avatar;
                                    account.Phonemumber = item.PhoneNumber;
                                    account.Password = item.cmnd;
                                    CreateAccountJeeHRNormalKafka(identity, internal_token, commonInfo.CustomerID, "importjeehr", account);
                                }
                                catch (Exception ex)
                                {
                                    var d3 = new GeneralLog()
                                    {
                                        name = "import jeehr",
                                        data = item.username + "(" + commonInfo.CustomerID + ")",
                                        message = $"error {ex.Message}"
                                    };
                                    _logger.LogError(JsonConvert.SerializeObject(d3));
                                }

                                var d2 = new GeneralLog()
                                {
                                    name = "import jeehr thành công",
                                    data = item.username + "(" + commonInfo.CustomerID + ")",
                                    message = $"thành công"
                                };
                                _logger.LogTrace(JsonConvert.SerializeObject(d2));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var d2 = new GeneralLog()
                {
                    name = "jeeplatform.initialization.appupdate",
                    data = value,
                    message = $"error {ex.Message}"
                };
                _logger.LogError(JsonConvert.SerializeObject(d2));
            }
        }

        public bool IsPasswordExist(long UserID)
        {
            string sql = $"select Password from AccountList where UserID = {UserID}";
            return (GeneralReponsitory.IsExist(sql, _connectionString));
        }

        public void CreateAccountJeeHRNormalKafka(IdentityServerController identityServerController, string inernal_token, long customerID, string usernameCreatedBy, AccountManagementModel account)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            account.Fullname = textInfo.ToTitleCase(account.Fullname.Trim());
            var listAppCode = account.AppCode.ToList();

            if (listAppCode.Contains("JeeHR")) listAppCode.Remove("JeeHR");

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                if (GeneralReponsitory.IsExistUsernameCnn(cnn, account.Username, customerID))
                {
                    var common = GeneralReponsitory.GetCommonInfoCnn(cnn, 0, account.Username);
                    if (common.StaffID == 0)
                    {
                        GeneralReponsitory.SaveStaffIDCnn(common.UserID, account.StaffID, cnn);
                        var d3 = new GeneralLog()
                        {
                            name = "import jeehr",
                            data = account.Username + "(" + customerID + ")",
                            message = $"đã thêm staffid"
                        };
                        _logger.LogTrace(JsonConvert.SerializeObject(d3));
                        return;
                    }
                    else if (common.StaffID > 0) return;
                    var d2 = new GeneralLog()
                    {
                        name = "import jeehr",
                        data = account.Username + "(" + customerID + ")",
                        message = $"username đã bị trùng"
                    };
                    _logger.LogError(JsonConvert.SerializeObject(d2));
                    return;
                }
                try
                {
                    cnn.BeginTransaction();

                    _accountReponsitory.CreateAccount(true, cnn, account, usernameCreatedBy, customerID, false, false);
                    account.Userid = GeneralReponsitory.GetCommonInfoCnn(cnn, 0, account.Username).UserID;
                    _accountReponsitory.InsertAppCodeAccount(cnn, account.Userid, account.AppID, usernameCreatedBy, false);

                    var identity = GeneralReponsitory.InitIdentityServerUserModel(customerID, account);
                    string userId = identity.customData.JeeAccount.UserID.ToString();

                    var createUser = identityServerController.AddNewUserInternalNotAysnc(identity, inernal_token);
                    if (!createUser.IsSuccessful)
                    {
                        string returnValue = createUser.Content;
                        var d2 = new GeneralLog()
                        {
                            name = "import jeehr",
                            data = account.Username + "(" + customerID + ")",
                            message = $"Lỗi identity server {returnValue}"
                        };
                        _logger.LogError(JsonConvert.SerializeObject(d2));
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        return;
                    }

                    cnn.EndTransaction();

                    if (!listAppCode.Contains("JeeHR")) listAppCode.Add("JeeHR");

                    var obj = new
                    {
                        CustomerID = customerID,
                        AppCode = listAppCode,
                        UserID = account.Userid,
                        Username = account.Username,
                        IsInitial = false,
                        IsAdmin = false
                    };

                    _producer.PublishAsync(TopicAddNewCustomerUser, JsonConvert.SerializeObject(obj));
                }
                catch (Exception)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    throw;
                }
            }
        }
    }

    public class FindStaffID
    {
        public long staffID { get; set; } = 0;
    }
}