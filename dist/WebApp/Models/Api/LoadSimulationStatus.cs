namespace ContosoEvents.Web.Models.Api
{
    public class LoadSimulationStatus
    {
        public string PartitionId { get; set; }
        public string PartitionStatus { get; set; }
        public string NodeName { get; set; }
        public string HealthState { get; set; }
        public int ItemsInQueue { get; set; }
    }
}