﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.ExternalExchangesApi.Exceptions;
using TradingBot.Infrastructure.Auth;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Models.Api;
using TradingBot.Trading;

namespace TradingBot.Controllers.Api
{
    [ApiKeyAuth]
    public sealed class AccountController : BaseApiController
    {
        private readonly TimeSpan _timeout;

        public AccountController(IApplicationFacade app, AppSettings appSettings)
            : base(app)
        {
            _timeout = appSettings.AspNet.ApiTimeout;

        }

        /// <summary>
        /// Returns a simple balance for the exchange
        /// </summary>
        /// <param name="exchangeName">The exchange name</param>
        /// <returns></returns>
        [HttpGet("balance")]
        [SwaggerOperation("GetBalance")]
        [ProducesResponseType(typeof(IEnumerable<AccountBalance>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        private async Task<IActionResult> GetBalance([Required][FromQuery]string exchangeName)// Intentionally disabled
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                return BadRequest($"Invalid {nameof(exchangeName)}");
            }
            try
            {
                return Ok(await Application.GetExchange(exchangeName).GetAccountBalance(_timeout));
            }
            catch (Exception e)
            {
                throw new StatusCodeException(HttpStatusCode.BadRequest, e.Message, e);
            }
        }

        /// <summary>
        /// Returns full balance information on the exchange
        /// </summary>
        /// <param name="exchangeName">The exchange name</param>
        /// <returns></returns>
        [SwaggerOperation("GetTradeBalance")]
        [HttpGet("tradeBalance")]
        [ProducesResponseType(typeof(IEnumerable<TradeBalanceModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTradeBalance([FromQuery]string exchangeName)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                return BadRequest($"Invalid {nameof(exchangeName)}");
            }
            try
            {
                return Ok(await Application.GetExchange(exchangeName).GetTradeBalances(_timeout));
            }
            catch (Exception e)
            {
                throw new StatusCodeException(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }
    }
}
