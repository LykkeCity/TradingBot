﻿namespace TradingBot.Infrastructure.Configuration
{
    public class RabbitMqConfigurationBase
    {
        public bool Enabled { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// see https://www.rabbitmq.com/uri-spec.html
        /// </summary>
        public string GetConnectionString()
        {
            var connectionString = "amqp://";

            if (!string.IsNullOrEmpty(Username))
            {
                var amqpAuthority = Username;

                if (!string.IsNullOrEmpty(Password))
                    amqpAuthority += $":{Password}";

                connectionString += $"{amqpAuthority}@";
            }

            connectionString += Host;

            if (Port > 0)
                connectionString += $":{Port}";

            return connectionString;
        }
    }
}