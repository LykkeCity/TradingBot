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

    public partial class AccessToken
    {
        /// <summary>
        /// Initializes a new instance of the AccessToken class.
        /// </summary>
        public AccessToken()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccessToken class.
        /// </summary>
        /// <param name="ttl">time to live in seconds (2 weeks by
        /// default)</param>
        public AccessToken(string id, double? ttl = default(double?), System.DateTime? created = default(System.DateTime?), double? userId = default(double?))
        {
            Id = id;
            Ttl = ttl;
            Created = created;
            UserId = userId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets time to live in seconds (2 weeks by default)
        /// </summary>
        [JsonProperty(PropertyName = "ttl")]
        public double? Ttl { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public System.DateTime? Created { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "userId")]
        public double? UserId { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Id == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Id");
            }
        }
    }
}
