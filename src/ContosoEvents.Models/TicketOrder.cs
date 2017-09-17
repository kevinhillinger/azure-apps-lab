using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace ContosoEvents.Models
{
    [Serializable]
    [DataContract]
    public class TicketOrder
    {
        public TicketOrder()
        {
            OrderDate = DateTime.Now;
            FulfillDate = null;
            CancellationDate = null;
            IsFulfilled = false;
            IsCancelled = false;
            Note = "";
        }

        [JsonProperty(PropertyName = "id")]
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime? FulfillDate { get; set; }
        [DataMember]
        public DateTime? CancellationDate { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Tag { get; set; }
        [DataMember]
        public string EventId { get; set; }
        [DataMember]
        public string PaymentProcessorTokenId { get; set; }
        [DataMember]
        public string PaymentProcessorConfirmation { get; set; }
        [DataMember]
        public int Tickets { get; set; }
        [DataMember]
        public double PricePerTicket { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public bool IsFulfilled { get; set; }
        [DataMember]
        public bool IsCancelled { get; set; }
        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public OrderStatuses Status { get; set; }

        public static void Validate(TicketOrder order)
        {
            if (order == null)
                throw new Exception("NULL Model");

            if (order.OrderDate == null)
                order.OrderDate = DateTime.Now;

            if (string.IsNullOrEmpty(order.Tag))
                order.Tag = "Manual";

            if (string.IsNullOrEmpty(order.EventId))
                throw new Exception("Event id is null or empty!");

            if (string.IsNullOrEmpty(order.PaymentProcessorTokenId))
                throw new Exception("Payment processor token is null or empty!");

            if (string.IsNullOrEmpty(order.UserName))
                throw new Exception("User name is null or empty!");

            if (order.Tickets <= 0)
                throw new Exception("Order tickets cannot be 0 or negative!");
        }
    }

    public enum OrderStatuses
    {
        Fufilled,
        TicketsExhausted,
        CreditCardDenied,
        Cancelled,
        Invalid
    }
}
