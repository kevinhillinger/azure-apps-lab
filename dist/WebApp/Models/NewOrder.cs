using ContosoEvents.Web.Models.Api;

namespace ContosoEvents.Web.Models
{
    public class NewOrder
    {
        public Event Event { get; set; }

        public int TicketCount { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string DeliveryEmail { get; set; }
        public string PhoneNumber { get; set; }

        public string PaymentProcessorToken { get; set; }
    }
}