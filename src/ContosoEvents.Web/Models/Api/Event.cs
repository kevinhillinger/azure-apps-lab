using Newtonsoft.Json;
using System;

namespace ContosoEvents.Web.Models.Api
{
    public class Event
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Name { get; set; }
        
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TotalTickets { get; set; }

        public double PricePerTicket { get; set; }

        public string Currency { get; set; }
    }
}