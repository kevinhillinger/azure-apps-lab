using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace ContosoEvents.Models
{
    [DataContract]
    public class TicketEvent
    {
        [JsonProperty(PropertyName = "id")]
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Summary { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ImageUrl { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public int TotalTickets { get; set; }

        [DataMember]
        public double PricePerTicket { get; set; }

        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public string PaymentProcessorUrl{ get; set; }

        [DataMember]
        public string PaymentProcessorAccount { get; set; }

        [DataMember]
        public string PaymentProcessorPassword { get; set; }

        [DataMember]
        public string SuccessEmailTemplate { get; set; }

        [DataMember]
        public string SuccessSmsTemplate { get; set; }

        [DataMember]
        public string FailedEmailTemplate { get; set; }

        [DataMember]
        public string FailedSmsTemplate { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static void Validate(TicketEvent tEvent)
        {
            if (tEvent == null)
                throw new Exception("NULL Model");

            if (string.IsNullOrEmpty(tEvent.Id))
                throw new Exception("Event id is null or empty!");

            if (string.IsNullOrEmpty(tEvent.Name))
                throw new Exception("Event name is null or empty!");

            if (tEvent.TotalTickets <= 0)
                throw new Exception("Event total tickets cannot be 0 or negative!");
        }
    }
}
