﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingBot.Communications;
using TradingBot.Exchanges.Abstractions;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Infrastructure.Logging;
using TradingBot.Trading;

namespace TradingBot.Exchanges.Concrete.HistoricalData
{
    public class HistoricalDataExchange : Exchange
    {
        public new static readonly string Name = "historical";
        
        private readonly HistoricalDataConfig config;

        private HistoricalDataReader reader;
        private bool stopRequested;
        
        public HistoricalDataExchange(HistoricalDataConfig config, TranslatedSignalsRepository translatedSignalsRepository) : base(Name, config, translatedSignalsRepository)
        {
            this.config = config;
        }

        protected override Task<bool> TestConnectionImpl(CancellationToken cancellationToken)
        {
            return Task.FromResult(File.Exists(config.BaseDirectory + string.Format(config.FileName, config.StartDate)));
        }

        private Task pricesCycle;

        public override Task OpenPricesStream()
        {
            stopRequested = false;

            pricesCycle = Task.Run(async () =>
            {
                var paths = new List<string>();

                for (DateTime day = config.StartDate; day <= config.EndDate; day = day.AddDays(1))
                {
                    var fileName = string.Format(config.FileName, day);
                    paths.Add(config.BaseDirectory + fileName);
                }
            
                reader = new HistoricalDataReader(paths.ToArray(), LineParsers.ParseTickLine);
            
                using (var enumerator = reader.GetEnumerator())
                    while (!stopRequested && enumerator.MoveNext())
                    {
                        await CallHandlers(new InstrumentTickPrices(Instruments.First(), new TickPrice[] { enumerator.Current }));
                    }
            });

            return Task.FromResult(0);
        }

        public override async Task ClosePricesStream()
        {
            stopRequested = true;
            reader?.Dispose();

            await pricesCycle;
        }

        protected override Task<bool> AddOrderImpl(Instrument instrument, TradingSignal signal, TranslatedSignalTableEntity translatedSignal)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> CancelOrderImpl(Instrument instrument, TradingSignal signal, TranslatedSignalTableEntity translatedSignal)
        {
            throw new NotImplementedException();
        }

        public override Task<ExecutedTrade> AddOrderAndWaitExecution(Instrument instrument, TradingSignal signal, TranslatedSignalTableEntity translatedSignal, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public override Task<ExecutedTrade> CancelOrderAndWaitExecution(Instrument instrument, TradingSignal signal, TranslatedSignalTableEntity translatedSignal, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}