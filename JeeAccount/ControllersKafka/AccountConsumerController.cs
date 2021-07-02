using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Services.AccountManagementService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAccountManagementService _accountManagementService;

        public AccountConsumerController(IConfiguration config, IProducer producer, ILogger<AccountConsumerController> logger, IAccountManagementService accountManagementService)
        {
            _config = config;
            accountConsumer = new Consumer(_config, _config.GetValue<string>("KafkaConfig:Consumer:JeeAccountGroup"));
            _producer = producer;
            _logger = logger;
            _accountManagementService = accountManagementService;
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

                if (obj.updateField.Equals("jee-hr", StringComparison.OrdinalIgnoreCase))
                {
                    var fieldvalue_json = JsonConvert.SerializeObject(obj.fieldValue);

                    var staffID = JsonConvert.DeserializeObject<FindStaffID>(fieldvalue_json);

                    if (staffID.staffID > 0)
                    {
                        SaveStaffID(staffID.staffID, obj.userId, _config.GetValue<string>("AppConfig:Connection"));
                    }
                    var inputModel = new InputApiModel();
                    inputModel.StaffID = staffID.staffID.ToString();
                    inputModel.Userid = obj.userId.ToString();

                    _accountManagementService.UpdateOneStaffIDByInputApiModel(inputModel);
                }
                string username = GetObjectDB($"select Username from AccountList where UserID = {obj.userId}", _config.GetValue<string>("AppConfig:Connection"));
                if (!string.IsNullOrEmpty(username))
                {
                    var identity = new IdentityServerController();
                    var x = identity.UppdateCustomDataInternal(GetSecretToken(), username, obj);
                }
            }
            catch (Exception ex)
            {
                var d2 = new GeneralLog()
                {
                    name = "jee-account",
                    data = value,
                    message = $"jeeplatform.initialization.appupdate error {ex.Message}"
                };
                _logger.LogError(JsonConvert.SerializeObject(d2));
            }
        }

        private string GetSecretToken()
        {
            var secret = _config.GetValue<string>("Jwt:internal_secret");
            var projectName = _config.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }

        private string GetObjectDB(string sql, string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var x = cnn.ExecuteScalar(sql);
                if (x != null)
                    return x.ToString();
                return "";
            }
        }

        private void SaveStaffID(long StaffID, long userId, string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                Hashtable val = new Hashtable();
                val.Add("StaffID", StaffID);
                SqlConditions cond = new SqlConditions();
                cond.Add("UserID", userId);
                int x = cnn.Update(val, cond, "AccountList");
            }
        }
    }

    public class FindStaffID
    {
        public long staffID { get; set; } = 0;
    }
}