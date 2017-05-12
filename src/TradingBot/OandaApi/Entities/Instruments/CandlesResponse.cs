﻿using System.Collections.Generic;

namespace TradingBot.OandaApi.Entities.Instruments
{
    public class CandlesResponse
    {
        /// <summary>
        /// The instrument whose Prices are represented by the candlesticks.
        /// </summary>
        public string Instrument { get; set; }

        /// <summary>
        /// The granularity of the candlesticks provided.
        /// </summary>
        public CanglestickGranularity Granularity { get; set; }

        /// <summary>
        /// The list of candlesticks that satisfy the request.
        /// </summary>
        public List<Candlestick> Candles { get; set; }
    }
}
