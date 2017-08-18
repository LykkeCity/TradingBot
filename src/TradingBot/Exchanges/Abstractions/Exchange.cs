﻿using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TradingBot.Handlers;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Infrastructure.Logging;
using TradingBot.Trading;

namespace TradingBot.Exchanges.Abstractions
{
    public abstract class Exchange
    {
        protected ILogger Logger = Logging.CreateLogger<Exchange>();

        private readonly List<Handler<InstrumentTickPrices>> tickPriceHandlers = new List<Handler<InstrumentTickPrices>>();

        private readonly List<Handler<ExecutedTrade>> executedTradeHandlers = new List<Handler<ExecutedTrade>>();
        
        public string Name { get; }

        public IExchangeConfiguration Config { get; }

        protected Exchange(string name, IExchangeConfiguration config)
        {
            Name = name;
            Config = config;
            
            decimal initialValue = 100m; // TODO: get initial value from config? or get if from real exchange.

            if (config.Instruments == null || config.Instruments.Length == 0)
            {
                throw new ArgumentException($"There is no instruments in the settings for {Name} exchange");
            }
            
            Instruments = config.Instruments.Select(x => new Instrument(Name, x)).ToList();
            Positions = Instruments.ToDictionary(x => x.Name, x => new Position(x, initialValue));

            AllSignals = Instruments.ToDictionary(x => x.Name, x => new List<TradingSignal>());
            ActualSignals = Instruments.ToDictionary(x => x.Name, x => new LinkedList<TradingSignal>());
            ExecutedTrades = Instruments.ToDictionary(x => x.Name, x => new List<ExecutedTrade>());
        }

        public void AddTickPriceHandler(Handler<InstrumentTickPrices> handler)
        {
            tickPriceHandlers.Add(handler);
        }

        public void AddExecutedTradeHandler(Handler<ExecutedTrade> handler)
        {
            executedTradeHandlers.Add(handler);
        }

        public IReadOnlyList<Instrument> Instruments { get; }
        
        public IReadOnlyDictionary<string, Position> Positions { get; }

        protected Dictionary<string, List<TradingSignal>> AllSignals;
        protected Dictionary<string, LinkedList<TradingSignal>> ActualSignals;
        protected Dictionary<string, List<ExecutedTrade>> ExecutedTrades;
        
        public Task<bool> TestConnection()
        {
            return TestConnection(CancellationToken.None);
        }

        public async Task<bool> TestConnection(CancellationToken cancellationToken)
        {
            Logger.LogDebug("Trying to test connection...");

            try
            {
				bool result = await TestConnectionImpl(cancellationToken);

				if (result)
				{
					Logger.LogInformation("Connection tested successfully.");
				}
				else
				{
					Logger.LogWarning("Connection test failed.");
				}

				return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "Connection test failed with error");
                return false;
            }
        }

        protected abstract Task<bool> TestConnectionImpl(CancellationToken cancellationToken);

        //public abstract Task<AccountInfo> GetAccountInfo(CancellationToken cancellationToken);


        public abstract Task OpenPricesStream();

        protected Task CallHandlers(InstrumentTickPrices tickPrices)
        {
            return Task.WhenAll(tickPriceHandlers.Select(x => x.Handle(tickPrices)));
        }

        protected Task CallExecutedTradeHandlers(ExecutedTrade trade)
        {
            return Task.WhenAll(executedTradeHandlers.Select(x => x.Handle(trade)));
        }

        public abstract void ClosePricesStream();

        
        protected readonly object ActualSignalsSyncRoot = new object();
        
        public virtual Task PlaceTradingOrders(InstrumentTradingSignals signals)
        {
            lock (ActualSignalsSyncRoot)
            {
                if (!ActualSignals.ContainsKey(signals.Instrument.Name))
                {
                    Logger.LogWarning($"ActualSignals doesn't contains a key {signals.Instrument.Name}. It has keys: {string.Join(", ", ActualSignals.Keys)}");
                    return Task.FromResult(0);
                }
                
                foreach (var arrivedSignal in signals.TradingSignals)
                {
                    TradingSignal existing;
                    
                    switch (arrivedSignal.Command)
                    {
                        case OrderCommand.Create:

                            existing = ActualSignals[signals.Instrument.Name]
                                .SingleOrDefault(x => x.OrderId == arrivedSignal.OrderId);
                            
                            if (existing != null)
                                Logger.LogDebug($"An order with id {arrivedSignal.OrderId} already in actual signals.");
                                // TODO: return message from the method
                            
                            ActualSignals[signals.Instrument.Name].AddLast(arrivedSignal);
                            AddOrder(signals.Instrument, arrivedSignal).Wait();
                            Logger.LogDebug($"Created new order {arrivedSignal}");
                            
                            break;
                            
                        case OrderCommand.Edit:
                            throw new NotSupportedException("Do not support edit signal");
                            //break;
                            
                        case OrderCommand.Cancel:
                            
                            existing = ActualSignals[signals.Instrument.Name]
                                .SingleOrDefault(x => x.OrderId == arrivedSignal.OrderId);

                            if (existing != null)
                            {
                                ActualSignals[signals.Instrument.Name].Remove(existing);
                                CancelOrder(signals.Instrument, existing).Wait();
                                Logger.LogDebug($"Canceled order {arrivedSignal}");
                            }
                            else
                                Logger.LogWarning($"Command for cancel unexisted order {arrivedSignal}");
                            
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (signals.TradingSignals.Any(x => x.Command == OrderCommand.Create))
                {
                    Logger.LogDebug($"Current orders:\n {string.Join("\n", ActualSignals[signals.Instrument.Name])}");
                }
            }
            
            return Task.FromResult(0);            
        }

        protected abstract Task<bool> AddOrder(Instrument instrument, TradingSignal signal);

        protected abstract Task<bool> CancelOrder(Instrument instrument, TradingSignal signal);

        public Dictionary<string, LinkedList<TradingSignal>> ActualOrders => ActualSignals; // TODO: to readonly dictionary and collection

        public abstract Task<ExecutedTrade> AddOrderAndWaitExecution(Instrument instrument, TradingSignal signal,
            TimeSpan timeout);

        public abstract Task<ExecutedTrade> CancelOrderAndWaitExecution(Instrument instrument, TradingSignal signal,
            TimeSpan timeout);
    }
}
