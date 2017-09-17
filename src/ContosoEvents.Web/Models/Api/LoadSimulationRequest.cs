namespace ContosoEvents.Web.Models.Api
{
    public class LoadSimulationRequest
    {
        public string BaseUrl { get; set; }
        public string EventId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Tag { get; set; }
        public int Iterations { get; set; }
    }
}