using System;

namespace ContosoEvents.Models
{
    public class TicketOrderSimulationRequest
    {
        public string BaseUrl { get; set; }
        public string EventId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Tag { get; set; }
        public int Iterations { get; set; }

        public static void Validate(TicketOrderSimulationRequest request)
        {
            if (request == null)
                throw new Exception("NULL request");

            if (string.IsNullOrEmpty(request.BaseUrl))
                throw new Exception("Base URL is null or empty!");

            if (string.IsNullOrEmpty(request.EventId))
                throw new Exception("Event id is null or empty!");

            if (string.IsNullOrEmpty(request.UserName))
                throw new Exception("User name is null or empty!");

            if (string.IsNullOrEmpty(request.Email))
                throw new Exception("Email is null or empty!");

            if (string.IsNullOrEmpty(request.Tag))
                throw new Exception("Tag is null or empty!");

            if (request.Iterations <= 0)
                throw new Exception("Iterations cannot be 0 or negative!");
        }
    }
}
