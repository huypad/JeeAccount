using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Reponsitories;
using JeeAccount.Services;
using JeeCustomer.ConsumerKafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static JeeCustomer.ConsumerKafka.AccountConsumerController;

namespace JeeAccount.ControllersKafka
{
    public class PermissionConsumerController : IHostedService
    {
        private IConfiguration _config;
        private Consumer _consumer;
        private IProducer _producer;
        private readonly ILogger<PermissionConsumerController> _logger;

        public PermissionConsumerController(IConfiguration config, IProducer producer, ILogger<PermissionConsumerController> logger)
        {
            _config = config;
            _consumer = new Consumer(_config, _config.GetValue<string>("KafkaConfig:Consumer:JeeAccountGroup"));
            _producer = producer;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
            {
                _consumer.SubscribeAsync(_config.GetValue<string>("KafkaConfig:TopicConsume:JeeplatformUpdateadminAppupdate"), GetValue);
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _consumer.closeAsync();
        }

        public void GetValue(string value)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjCustomData>(value);
                var objRemove = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjCustomDataRoles>(value);
                string username = GeneralReponsitory.GetObjectDB($"select Username from AccountList where UserID = {obj.userId}", _config.GetValue<string>("AppConfig:Connection"));
                if (!string.IsNullOrEmpty(username))
                {
                    var identity = new IdentityServerController();
                    _ = identity.UppdateCustomDataInternal(GeneralService.GetInternalToken(_config), username, obj);
                }
                if (obj.updateField.Equals("jee-hr", StringComparison.OrdinalIgnoreCase))
                {
                    var fieldvalue_json = JsonConvert.SerializeObject(obj.fieldValue);

                    var staffID = JsonConvert.DeserializeObject<FindStaffID>(fieldvalue_json);

                    if (staffID.staffID > 0)
                    {
                        GeneralReponsitory.SaveStaffID(staffID.staffID, obj.userId, _config.GetValue<string>("AppConfig:Connection"));
                    }

                    if (string.IsNullOrEmpty(objRemove.fieldValue.roles))
                    {
                        GeneralReponsitory.RemoveAppCodeJeeHRKafka(_config.GetValue<string>("AppConfig:Connection"), obj.userId);
                    }
                    else
                    {
                        GeneralReponsitory.InsertAppCodeJeeHRKafka(_config.GetValue<string>("AppConfig:Connection"), obj.userId);
                    }

                    _ = GeneralReponsitory.UpdateJeeAccountCustomDataByInputApiModel(obj.userId, _config.GetValue<string>("AppConfig:Connection"), GeneralService.GetInternalToken(_config));
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
    }
}