﻿using DPSinfra.Kafka;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Controllers;
using JeeAccount.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        public AccountConsumerController(IConfiguration config, IProducer producer)
        {
            _config = config;
            string group = _config.GetValue<string>("KafkaConfig:ProjectName") + _config.GetValue<string>("KafkaConsumer:ConsumerAddNewCustomer");
            accountConsumer = new Consumer(_config, group);
            _producer = producer;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
            {
                accountConsumer.SubscribeAsync(_config.GetValue<string>("KafkaConsumer:ConsumerAddNewCustomer"), GetValue);
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await accountConsumer.closeAsync();
        }

        public void GetValue(string value)
        {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjCustomData>(value);
            var identity = new IdentityServerController();
            string username = GetObjectDB($"select Username from AccountList where UserID ={obj.userId}", _config.GetConnectionString("DefaultConnection")).ToString();
            var x = identity.UppdateCustomDataInternal(getSecretToken(), username, obj);
        }

        public string getSecretToken()
        {
            var secret = _config.GetValue<string>("Jwt:internal_secret");
            var projectName = _config.GetValue<string>("KafkaConfig:ProjectName");
            var token = JsonWebToken.issueToken(new TokenClaims { projectName = projectName }, secret);
            return token;
        }

        public object GetObjectDB(string sql, string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                return cnn.ExecuteScalar(sql);
            }
        }
    }
}