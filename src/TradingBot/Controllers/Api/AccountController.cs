﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradingBot.Exchanges.Concrete.Kraken;
using TradingBot.Exchanges.Concrete.Kraken.Entities;
using TradingBot.Infrastructure.Exceptions;

namespace TradingBot.Controllers.Api
{
    public class AccountController : BaseApiController
    {
        [HttpGet("{exchangeName}/balance")]
        public Task<Dictionary<string, decimal>> GetBalance(string exchangeName)
        {
            try
            {
                return Application.GetExchange(exchangeName).GetAccountBalance(CancellationToken.None);
            }
            catch (Exception e)
            {
                throw new StatusCodeException(HttpStatusCode.BadRequest, e.Message, e);
            }
        }

        [HttpGet("{exchangeName}/tradeBalance")]
        public Task<TradeBalanceInfo> GetTradeBalance(string exchangeName)
        {
            try
            {
                if (exchangeName != KrakenExchange.Name)
                    throw new NotSupportedException("Only Kraken exchange is supported");

                return ((KrakenExchange) Application.GetExchange(exchangeName)).GetTradeBalance(CancellationToken.None);
            }
            catch (Exception e)
            {
                throw new StatusCodeException(HttpStatusCode.BadRequest, e.Message, e);
            }
        }
    }
}