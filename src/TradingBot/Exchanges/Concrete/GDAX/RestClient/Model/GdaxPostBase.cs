﻿using Newtonsoft.Json;

namespace TradingBot.Exchanges.Concrete.GDAX.RestClient.Model
{
    internal class GdaxPostBase
    {
        [JsonProperty("request")]
        public string RequestUrl { get; set; }
    }

}
