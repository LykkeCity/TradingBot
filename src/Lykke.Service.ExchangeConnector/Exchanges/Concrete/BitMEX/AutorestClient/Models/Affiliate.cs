// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace TradingBot.Exchanges.Concrete.AutorestClient.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class Affiliate
    {
        /// <summary>
        /// Initializes a new instance of the Affiliate class.
        /// </summary>
        public Affiliate()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Affiliate class.
        /// </summary>
        public Affiliate(double account, string currency, double? prevPayout = default(double?), double? prevTurnover = default(double?), double? prevComm = default(double?), System.DateTime? prevTimestamp = default(System.DateTime?), double? execTurnover = default(double?), double? execComm = default(double?), double? totalReferrals = default(double?), double? totalTurnover = default(double?), double? totalComm = default(double?), double? payoutPcnt = default(double?), double? pendingPayout = default(double?), System.DateTime? timestamp = default(System.DateTime?), double? referrerAccount = default(double?))
        {
            Account = account;
            Currency = currency;
            PrevPayout = prevPayout;
            PrevTurnover = prevTurnover;
            PrevComm = prevComm;
            PrevTimestamp = prevTimestamp;
            ExecTurnover = execTurnover;
            ExecComm = execComm;
            TotalReferrals = totalReferrals;
            TotalTurnover = totalTurnover;
            TotalComm = totalComm;
            PayoutPcnt = payoutPcnt;
            PendingPayout = pendingPayout;
            Timestamp = timestamp;
            ReferrerAccount = referrerAccount;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "account")]
        public double Account { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "prevPayout")]
        public double? PrevPayout { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "prevTurnover")]
        public double? PrevTurnover { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "prevComm")]
        public double? PrevComm { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "prevTimestamp")]
        public System.DateTime? PrevTimestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "execTurnover")]
        public double? ExecTurnover { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "execComm")]
        public double? ExecComm { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "totalReferrals")]
        public double? TotalReferrals { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "totalTurnover")]
        public double? TotalTurnover { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "totalComm")]
        public double? TotalComm { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "payoutPcnt")]
        public double? PayoutPcnt { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pendingPayout")]
        public double? PendingPayout { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public System.DateTime? Timestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "referrerAccount")]
        public double? ReferrerAccount { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Currency == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Currency");
            }
        }
    }
}