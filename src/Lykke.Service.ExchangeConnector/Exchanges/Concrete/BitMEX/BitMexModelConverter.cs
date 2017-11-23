﻿using System;
using System.Collections.Generic;
using TradingBot.Exchanges.Concrete.AutorestClient.Models;
using TradingBot.Exchanges.Concrete.Shared;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Models.Api;
using TradingBot.Trading;
using Instrument = TradingBot.Trading.Instrument;
using Order = TradingBot.Exchanges.Concrete.AutorestClient.Models.Order;
using Position = TradingBot.Exchanges.Concrete.AutorestClient.Models.Position;
using OrdStatus = TradingBot.Exchanges.Concrete.BitMEX.WebSocketClient.Model.OrdStatus;
using Side = TradingBot.Exchanges.Concrete.BitMEX.WebSocketClient.Model.Side;

namespace TradingBot.Exchanges.Concrete.BitMEX
{
    internal class BitMexModelConverter: ExchangeConverters
    {
        private const decimal SatoshiRate = 100000000;

        public BitMexModelConverter(IReadOnlyCollection<CurrencySymbol> currencySymbols,
            string exchangeName): base(currencySymbols, exchangeName)
        {

        }

        public static PositionModel ExchangePositionToModel(Position position)
        {
            return new PositionModel
            {
                // Symbol = ConvertSymbolFromBitMexToLykke(position.Symbol, configuration).Name,
                Symbol = "USDBTC", //HACK Hard code!
                PositionVolume = Convert.ToDecimal(position.CurrentQty),
                MaintMarginUsed = Convert.ToDecimal(position.MaintMargin) / SatoshiRate,
                RealisedPnL = Convert.ToDecimal(position.RealisedPnl) / SatoshiRate,
                UnrealisedPnL = Convert.ToDecimal(position.UnrealisedPnl) / SatoshiRate,
                PositionValue = -Convert.ToDecimal(position.MarkValue) / SatoshiRate,
                AvailableMargin = 0, // Nothing to map
                InitialMarginRequirement = Convert.ToDecimal(position.InitMarginReq),
                MaintenanceMarginRequirement = Convert.ToDecimal(position.MaintMarginReq)
            };
        }

        public static ExecutedTrade OrderToTrade(Order order)
        {
            var execTime = order.TransactTime ?? DateTime.UtcNow;
            var execPrice = (decimal)(order.Price ?? 0);
            var execVolume = (decimal)(order.OrderQty ?? 0);
            var tradeType = ConvertTradeType(order.Side);
            var status = ConvertExecutionStatus(order.OrdStatus);
            //  var instr = ConvertSymbolFromBitMexToLykke(order.Symbol, configuration);
            var instr = new Instrument(BitMexExchange.Name, "USDBTC"); //HACK Hard code!

            return new ExecutedTrade(instr, execTime, execPrice, execVolume, tradeType, order.OrderID, status) { Message = order.Text };
        }


        public ExecutedTrade OrderToTrade(WebSocketClient.Model.RowItem row)
        {
            if (row.AskPrice.HasValue && row.BidPrice.HasValue)
            {
                var lykkeInstrument = this.ExchangeSymbolToLykkeInstrument(row.Symbol);
                return new ExecutedTrade(
                    lykkeInstrument,
                    time: row.Timestamp,
                    price: row.Price ?? row.AvgPx ?? 0,
                    volume: (decimal)(row.OrderQty ?? row.CumQty ?? 0),
                    type: ConvertSideToModel(row.Side),
                    orderId: row.OrderID,
                    status: ConvertExecutionStatusToModel(row.OrdStatus));
            }
            else
            {
                throw new ArgumentException("Ask/bid price is not specified for a quote.", nameof(row));
            }
        }

        public Acknowledgement OrderToAck(WebSocketClient.Model.RowItem row)
        {
            var lykkeInstrument = this.ExchangeSymbolToLykkeInstrument(row.Symbol);
            return new Acknowledgement()
            {
                Instrument = lykkeInstrument.Name,
                Exchange = lykkeInstrument.Exchange,
                ClientOrderId = row.ClOrdID,
                ExchangeOrderId = row.OrderID,
                Success = true
            };
        }

        public static string ConvertOrderType(OrderType type)
        {
            switch (type)
            {
                case OrderType.Market:
                    return "Market";
                case OrderType.Limit:
                    return "Limit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static double ConvertVolume(decimal volume)
        {
            return (double)volume;
        }

        public static string ConvertTradeType(TradeType signalTradeType)
        {
            switch (signalTradeType)
            {
                case TradeType.Buy:
                    return "Buy";
                case TradeType.Sell:
                    return "Sell";
                default:
                    throw new ArgumentOutOfRangeException(nameof(signalTradeType), signalTradeType, null);
            }
        }

        public static TradeType ConvertTradeType(string signalTradeType)
        {
            switch (signalTradeType)
            {
                case "Buy":
                    return TradeType.Buy;
                case "Sell":
                    return TradeType.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signalTradeType), signalTradeType, null);
            }
        }

        public static TradeType ConvertSideToModel(Side side)
        {
            switch (side)
            {
                case Side.Buy:
                    return TradeType.Buy;
                case Side.Sell:
                    return TradeType.Sell;
                default:
                    return TradeType.Unknown;
            }
        }

        public static ExecutionStatus ConvertExecutionStatusToModel(OrdStatus status)
        {
            switch (status)
            {
                case OrdStatus.New:
                    return ExecutionStatus.New;
                case OrdStatus.PartiallyFilled:
                    return ExecutionStatus.PartialFill;
                case OrdStatus.Filled:
                    return ExecutionStatus.Fill;
                case OrdStatus.Canceled:
                    return ExecutionStatus.Cancelled;
                default:
                    return ExecutionStatus.Unknown;
            }
        }

        public static ExecutionStatus ConvertExecutionStatus(string executionStatus)
        {
            switch (executionStatus)
            {
                case "New":
                    return ExecutionStatus.New;
                case "Filled":
                    return ExecutionStatus.Fill;
                case "Partially Filled":
                    return ExecutionStatus.PartialFill;
                case "Canceled":
                    return ExecutionStatus.Cancelled;
                default:
                    return ExecutionStatus.Unknown;
            }
        }

        public static TradeBalanceModel ExchangeBalanceToModel(Margin bitmexMargin)
        {
            var model = new TradeBalanceModel
            {
                AccountCurrency = "BTC", // The only currency supported
                Totalbalance = Convert.ToDecimal(bitmexMargin.MarginBalance) / SatoshiRate,
                UnrealisedPnL = Convert.ToDecimal(bitmexMargin.UnrealisedPnl) / SatoshiRate,
                MaringAvailable = Convert.ToDecimal(bitmexMargin.AvailableMargin) / SatoshiRate,
                MarginUsed = Convert.ToDecimal(bitmexMargin.MaintMargin) / SatoshiRate
            };
            return model;
        }

        public TickPrice QuoteToModel(WebSocketClient.Model.RowItem row)
        {
            if (row.AskPrice.HasValue && row.BidPrice.HasValue)
            {
                var lykkeInstrument = this.ExchangeSymbolToLykkeInstrument(row.Symbol);
                return new TickPrice(lykkeInstrument, row.Timestamp, (decimal)row.AskPrice.Value, (decimal)row.BidPrice.Value);
            }
            else
            {
                throw new ArgumentException("Ask/bid price is not specified for a quote.", nameof(row));
            }
        }
    }
}
