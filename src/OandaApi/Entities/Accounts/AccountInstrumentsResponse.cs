﻿using System.Collections.Generic;
using TradingBot.OandaApi.Entities.Instruments;

namespace TradingBot.OandaApi.Entities.Accounts
{
    /// <summary>
    /// The list of tradeable instruments for the Account has been provided.
    /// </summary>
    public class AccountInstrumentsResponse
    {
        /// <summary>
        /// The requested list of instruments.
        /// </summary>
        public List<Instrument> Instruments { get; set; }
        
        /// <summary>
        /// The ID of the most recent Transaction created for the Account.
        /// </summary>
        public int LastTransactionID { get; set; }
    }
}
