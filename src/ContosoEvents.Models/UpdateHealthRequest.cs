using System;

namespace ContosoEvents.Models
{
    public class UpdateHealthRequest
    {
        public string ServiceName { get; set; }
        public string State { get; set; }
        public string Message { get; set; }

        public static void Validate(UpdateHealthRequest request)
        {
            if (request == null)
                throw new Exception("NULL request");

            if (string.IsNullOrEmpty(request.Message))
                throw new Exception("Message is null or empty!");

            if (string.IsNullOrEmpty(request.State))
                throw new Exception("State is null or empty!");
        }
    }
}
