using System;

namespace ContosoEvents.Web.Models.Api
{
    public class OrderRequest
    {
        public DateTime OrderDate { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EventId { get; set; }

        public string PaymentProcessorTokenId { get; set; }

        public int Tickets { get; set; }
    }
}