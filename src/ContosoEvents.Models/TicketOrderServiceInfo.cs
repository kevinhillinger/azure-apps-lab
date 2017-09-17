namespace ContosoEvents.Models
{
    public class TicketOrderServiceInfo
    {
        public string PartitionId { get; set; }
        public string PartitionKind { get; set; }
        public string PartitionStatus { get; set; }
        public string NodeName { get; set; }
        public string HealthState { get; set; }
        public string ServiceKind { get; set; }
        public int ItemsInQueue { get; set; }
    }
}
