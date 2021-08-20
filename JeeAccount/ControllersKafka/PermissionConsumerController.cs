using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                var obj = JsonConvert.DeserializeObject<ObjCustomData>(value);

                string username = GetObjectDB($"select Username from AccountList where UserID = {obj.userId}", _config.GetValue<string>("AppConfig:Connection"));
                if (!string.IsNullOrEmpty(username))
                {
                    var identity = new IdentityServerController();
                    _ = identity.UppdateCustomDataInternal(GeneralService.GetInternalToken(_config), username, obj);
                }
                else
                {
                    throw new Exception("Username is null");
                }
            }
            catch (Exception ex)
            {
                var d2 = new GeneralLog()
                {
                    name = "jeeplatform.updateadmin.appupdate",
                    data = value,
                    message = $"{ex.Message}"
                };
                _logger.LogError(JsonConvert.SerializeObject(d2));
            }
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
    }
}