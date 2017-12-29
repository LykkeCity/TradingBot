// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

using Microsoft.Rest;
using Newtonsoft.Json;

namespace Lykke.ExternalExchangesApi.Exchanges.BitMex.AutorestClient.Models
{
    public partial class StatsHistory
    {
        /// <summary>
        /// Initializes a new instance of the StatsHistory class.
        /// </summary>
        public StatsHistory()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the StatsHistory class.
        /// </summary>
        public StatsHistory(System.DateTime date, string rootSymbol, string currency = default(string), double? volume = default(double?), double? turnover = default(double?))
        {
            Date = date;
            RootSymbol = rootSymbol;
            Currency = currency;
            Volume = volume;
            Turnover = turnover;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        public System.DateTime Date { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "rootSymbol")]
        public string RootSymbol { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "volume")]
        public double? Volume { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "turnover")]
        public double? Turnover { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (RootSymbol == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "RootSymbol");
            }
        }
    }
}
