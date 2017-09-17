using Newtonsoft.Json;
using System;

namespace ContosoEvents.Web.Models.Api
{
    public class Order
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EventId { get; set; }

        public string PaymentProcessorTokenId { get; set; }

        public string PaymentProcessorConfirmation { get; set; }

        public int Tickets { get; set; }

        public DateTime? FulfillDate { get; set; }

        public DateTime? CancellationDate { get; set; }

        public string Tag { get; set; }

        public bool IsFulfilled { get; set; }

        public bool IsCancelled { get; set; }

        public string Note { get; set; }
    }
}