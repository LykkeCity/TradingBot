﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TradingBot.Common.Communications;
using TradingBot.Exchanges.Abstractions;
using TradingBot.Trading;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Exchanges;
using System.Linq;
using System.Text;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using System.Collections.Generic;
using TradingBot.Communications;
using TradingBot.Infrastructure.Logging;

namespace TradingBot
{
    public class GetPricesCycle
    {
        private readonly ILogger logger = Logging.CreateLogger<GetPricesCycle>();

        public GetPricesCycle(Configuration config)
        {
            this.config = config;

			exchange = ExchangeFactory.CreateExchange(config.Exchanges);
        }


        private readonly Exchange exchange;

		private CancellationTokenSource ctSource;

        private readonly Configuration config;

        private RabbitMqPublisher<InstrumentTickPrices> rabbitPublisher;

        private Dictionary<Instrument, AzureTablePricesPublisher> azurePublishers;

        public async Task Start()
        {
            ctSource = new CancellationTokenSource();
            var token = ctSource.Token;

            if (exchange == null)
            {
                logger.LogInformation("There is no enabled exchange.");
                return;
            }
            
            
            logger.LogInformation($"Price cycle starting for exchange {exchange.Name}...");

            bool connectionTestPassed = await new Reconnector(times: 5, pause: TimeSpan.FromSeconds(10)) // TODO: Use Polly
                .ConnectAsync(exchange.TestConnection, token);

            if (!connectionTestPassed)
            {
                logger.LogError($"Price cycle not started: no connection to exchange {exchange.Name}");
                return;
            }

            if (config.RabbitMq.Enabled)
            {

                var rabbitSettings = new RabbitMqPublisherSettings()
                {
                    ConnectionString = config.RabbitMq.Host,
                    ExchangeName = config.RabbitMq.ExchangeName
                };
                
                var rabbitConsole = new RabbitConsole();
                
                rabbitPublisher = new RabbitMqPublisher<InstrumentTickPrices>(rabbitSettings)
                    .SetSerializer(new InstrumentTickPricesSerializer())
                    .SetLogger(new LogToConsole())
                    .SetPublishStrategy(new DefaultFnoutPublishStrategy())
                    .SetConsole(rabbitConsole)
                    .Start();
            }

            if (config.AzureTable.Enabled)
            {
                azurePublishers = exchange.Instruments.ToDictionary(
	                x => x,
	                x => new AzureTablePricesPublisher(x, config.AzureTable.TableName, config.AzureTable.StorageConnectionString));
            }


            var task = exchange.OpenPricesStream(PublishTickPrices);

            while (!token.IsCancellationRequested)
			{
                await Task.Delay(TimeSpan.FromSeconds(15), token);
				logger.LogDebug($"GetPricesCycle Heartbeat: {DateTime.Now}");
			}

			if (task.Status == TaskStatus.Running)
			{
				task.Wait();
			}
        }

        private async void PublishTickPrices(InstrumentTickPrices prices)
        {
            logger.LogDebug($"{DateTime.Now}. {prices.TickPrices.Length} prices received for: {prices.Instrument}");

			if (config.RabbitMq.Enabled)
			{
			    await rabbitPublisher.ProduceAsync(prices);
			}

            if (config.AzureTable.Enabled)
            {
                await azurePublishers[prices.Instrument].Publish(prices);
            }
        }


        public void Stop()
        {
            logger.LogInformation("Stop requested");
            ctSource.Cancel();

            exchange?.ClosePricesStream();

            ((IStopable) rabbitPublisher)?.Stop();
        }

        public class MessageSerializer : IRabbitMqSerializer<string>
        {
            public byte[] Serialize(string model)
            {
                return Encoding.UTF8.GetBytes(model);
            }
        }

        public class RabbitConsole : IConsole
        {
            public void WriteLine(string line)
            {
                Console.WriteLine(line);
            }
        }
    }
}