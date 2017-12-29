﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.ExternalExchangesApi.Exchanges.BitMex.WebSocketClient;
using Lykke.ExternalExchangesApi.Exchanges.BitMex.WebSocketClient.Model;
using Newtonsoft.Json;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Trading;
using Action = Lykke.ExternalExchangesApi.Exchanges.BitMex.WebSocketClient.Model.Action;

namespace TradingBot.Exchanges.Concrete.BitMEX
{
    internal sealed class BitMexExecutionHarvester
    {
        private readonly ILog _log;
        private readonly BitMexModelConverter _mapper;
        private Func<OrderStatusUpdate, Task> _tradeHandler;

        public BitMexExecutionHarvester(BitMexExchangeConfiguration configuration, IBitmexSocketSubscriber socketSubscriber, ILog log)
        {
            _log = log.CreateComponentScope(nameof(BitMexExecutionHarvester));
            socketSubscriber.Subscribe(BitmexTopic.execution, HandleExecutionResponseAsync);
            _mapper = new BitMexModelConverter(configuration.SupportedCurrencySymbols, BitMexExchange.Name);
        }

        public void AddExecutedTradeHandler(Func<OrderStatusUpdate, Task> handler)
        {
            _tradeHandler = handler;
        }

        private async Task HandleExecutionResponseAsync(TableResponse table)
        {
            if (_tradeHandler == null)
            {
                throw new InvalidOperationException("Acknowledgment handler or executed trader is not set.");
            }

            if (!ValidateOrder(table))
            {
                await _log.WriteWarningAsync(nameof(BitMexExecutionHarvester), nameof(HandleExecutionResponseAsync),
                    $"Ignoring invalid 'order' message: '{JsonConvert.SerializeObject(table)}'");
                return;
            }

            switch (table.Action)
            {
                case Action.Insert:
                    var acks = table.Data.Select(row => _mapper.OrderToTrade(row));
                    foreach (var ack in acks)
                    {
                        if (ack.ExecutionStatus == OrderExecutionStatus.New)
                        {
                            continue;
                        }
                        await _tradeHandler(ack);
                    }
                    break;
                case Action.Partial:
                    //  Just ignore
                    break;
                default:
                    await _log.WriteWarningAsync(nameof(HandleExecutionResponseAsync), "Execution response", $"Unexpected response {table.Action}");
                    break;
            }
        }

        private static bool ValidateOrder(TableResponse table)
        {
            return table?.Data != null && table.Data.All(item =>
                       !string.IsNullOrEmpty(item.Symbol)
                       && !string.IsNullOrEmpty(item.OrderID));
        }
    }
}