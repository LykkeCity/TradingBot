﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using TradingBot.Communications;
using TradingBot.Exchanges.Abstractions;
using TradingBot.Infrastructure.Exceptions;
using TradingBot.Repositories;
using TradingBot.Trading;

namespace TradingBot.Handlers
{
    internal class TradingSignalsHandler : Handler<TradingSignal>
    {
        private readonly IReadOnlyDictionary<string, Exchange> exchanges;
        private readonly ILog logger;
        private readonly TranslatedSignalsRepository translatedSignalsRepository;
        private readonly TimeSpan tradingSignalsThreshold = TimeSpan.FromMinutes(5);
        private readonly TimeSpan apiTimeout;

        public TradingSignalsHandler(Dictionary<string, Exchange> exchanges, ILog logger, TranslatedSignalsRepository translatedSignalsRepository, TimeSpan apiTimeout)
        {
            this.exchanges = exchanges;
            this.logger = logger;
            this.translatedSignalsRepository = translatedSignalsRepository;
            this.apiTimeout = apiTimeout;
        }
        
        public override Task Handle(TradingSignal message)
        {
            if (message == null || message.Instrument == null || string.IsNullOrEmpty(message.Instrument.Name))
            {
                return logger.WriteWarningAsync(
                    nameof(TradingSignalsHandler),
                    nameof(Handle),
                    message?.ToString(),
                    "Received an unconsistent signal");
            }
            
            if (!exchanges.ContainsKey(message.Instrument.Exchange))
            {
                return logger.WriteWarningAsync(
                        nameof(TradingSignalsHandler),
                        nameof(Handle),
                        message.ToString(),
                        $"Received a trading signal for unconnected exchange {message.Instrument.Exchange}");
            }
                
            return HandleTradingSignals(exchanges[message.Instrument.Exchange], message);    
        }

        private async Task HandleTradingSignals(Exchange exchange, TradingSignal signal)
        {
            await logger.WriteInfoAsync(nameof(TradingSignalsHandler), nameof(HandleTradingSignals), signal.ToString(), "New trading signal to be handled.");
            
            var translatedSignal = new TranslatedSignalTableEntity(SignalSource.RabbitQueue, signal);

            try
            {
                switch (signal.Command)
                {
                    case OrderCommand.Create:
                        await HandleCreation(signal, translatedSignal, exchange);
                        break;
                    case OrderCommand.Cancel:
                        await HandleCancellation(signal, translatedSignal, exchange);
                        break;
                        
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                translatedSignal.Failure(e);
            }
            finally
            {
                translatedSignalsRepository.Save(translatedSignal);

                await logger.WriteInfoAsync(nameof(TradingSignalsHandler), nameof(HandleTradingSignals),
                    signal.ToString(), "Signal handled. Waiting for another one.");
            }
        }

        private async Task HandleCreation(TradingSignal signal, TranslatedSignalTableEntity translatedSignal,
            Exchange exchange)
        {
            try
            {
                if (!signal.IsTimeInThreshold(tradingSignalsThreshold))
                {
                    translatedSignal.Failure("The signal is too old");

                    await logger.WriteInfoAsync(nameof(TradingSignalsHandler),
                        nameof(HandleCreation),
                        signal.ToString(),
                        "Skipping old signal");

                    return;
                }

                var executedTrade = await exchange.AddOrderAndWaitExecution(signal, translatedSignal, apiTimeout);

                bool orderAdded = executedTrade.Status == ExecutionStatus.New ||
                                  executedTrade.Status == ExecutionStatus.Pending;

                bool orderFilled = executedTrade.Status == ExecutionStatus.Fill ||
                                   executedTrade.Status == ExecutionStatus.PartialFill;

                if (orderAdded || orderFilled)
                {
                    await logger.WriteInfoAsync(nameof(TradingSignalsHandler),
                        nameof(HandleCreation),
                        signal.ToString(),
                        "Created new order");
                }
                else
                {
                    await logger.WriteWarningAsync(nameof(TradingSignalsHandler),
                        nameof(HandleCreation),
                        signal.ToString(),
                        $"Added order is in unexpected status: {executedTrade}");

                    translatedSignal.Failure($"Added order is in unexpected status: {executedTrade}");
                }

                await exchange.CallAcknowledgementsHandlers(CreateAcknowledgement(exchange, orderAdded, signal, translatedSignal));

                if (orderFilled)
                {
                    await exchange.CallExecutedTradeHandlers(executedTrade);
                }
            }
            catch (ApiException e)
            {
                await logger.WriteInfoAsync(nameof(TradingSignalsHandler), nameof(HandleCreation), signal.ToString(), e.Message);
                translatedSignal.Failure(e);
                await exchange.CallAcknowledgementsHandlers(CreateAcknowledgement(exchange, false, signal, translatedSignal, e));
            }
            catch (Exception e)
            {
                await logger.WriteErrorAsync(nameof(TradingSignalsHandler), nameof(HandleCreation), signal.ToString(), e);
                translatedSignal.Failure(e);
                await exchange.CallAcknowledgementsHandlers(CreateAcknowledgement(exchange, false, signal, translatedSignal, e));
            }
        }

        private async Task HandleCancellation(TradingSignal signal, TranslatedSignalTableEntity translatedSignal,
            Exchange exchange)
        {
            try
            {
                var executedTrade = await exchange.CancelOrderAndWaitExecution(signal, translatedSignal, apiTimeout);

                if (executedTrade.Status == ExecutionStatus.Cancelled)
                {
                    await exchange.CallExecutedTradeHandlers(executedTrade);
                }
                else
                {
                    var message =
                        $"Executed trade status {executedTrade.Status} after calling 'exchange.CancelOrderAndWaitExecution'";
                    translatedSignal.Failure(message);
                    await logger.WriteWarningAsync(nameof(TradingSignalsHandler),
                        nameof(HandleCancellation),
                        signal.ToString(),
                        message);
                }
            }
            catch (ApiException e)
            {
                translatedSignal.Failure(e);
                await logger.WriteInfoAsync(nameof(TradingSignalsHandler), nameof(HandleCancellation),
                    signal.ToString(),
                    e.Message);
            }
            catch (Exception e)
            {
                translatedSignal.Failure(e);
                await logger.WriteErrorAsync(nameof(TradingSignalsHandler),
                    nameof(HandleCancellation),
                    signal.ToString(),
                    e);
            }
        }

        private static Acknowledgement CreateAcknowledgement(Exchange exchange, bool success,
            TradingSignal arrivedSignal, TranslatedSignalTableEntity translatedSignal, Exception exception = null)
        {
            var ack = new Acknowledgement()
            {
                Success = success,
                Exchange = exchange.Name,
                Instrument = arrivedSignal.Instrument.Name,
                ClientOrderId = arrivedSignal.OrderId,
                ExchangeOrderId = translatedSignal.ExternalId,
                Message = translatedSignal.ErrorMessage
            };

            if (exception != null)
            {
                switch (exception)
                {
                    case InsufficientFundsException _:
                        ack.FailureType = AcknowledgementFailureType.InsufficientFunds;
                        break;
                    case ApiException _:
                        ack.FailureType = AcknowledgementFailureType.ExchangeError;
                        break;
                    default:
                        ack.FailureType = AcknowledgementFailureType.ConnectorError;
                        break;
                }
            }
            else
            {
                ack.FailureType = AcknowledgementFailureType.None;
            }
            
            return ack;
        }
    }
}
