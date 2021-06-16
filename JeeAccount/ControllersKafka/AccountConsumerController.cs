﻿using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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

        public AccountConsumerController(IConfiguration config, IProducer producer, ILogger<AccountConsumerController> logger)
        {
            _config = config;
            accountConsumer = new Consumer(_config, _config.GetValue<string>("KafkaConfig:Consumer:JeeAccountGroup"));
            _producer = producer;
            _logger = logger;
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

                var identity = new IdentityServerController();
                string username = GetObjectDB($"select Username from AccountList where UserID = {obj.userId}", _config.GetValue<string>("AppConfig:Connection"));
                if (!string.IsNullOrEmpty(username))
                {
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
    }
}