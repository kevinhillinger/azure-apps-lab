namespace ContosoEvents.Shared.Helpers
{
    public class Constants
    {
        // Mosaic Sales Measures Calculator App
        public const string ContosoEventsApplicationName = "fabric:/ContosoEventsApp";
        public const string ContosoEventsApplicationInstance = "ContosoEventsApp";
        public const string ContosoEventsTicketOrderServiceName = "TicketOrderService";
        public const string ContosoEventsTicketOrderActorName = "TicketOrderActorService";
        public const string ContosoEventsEventActorName = "EventActorService";

        // Used as service properties for logging
        public const string ServicePropType = "Type";
        public const string ServicePropId = "Id";
        public const string ServicePropApplicationType = "ApplicationType";
        public const string ServicePropApplicationName = "ApplicationName";
        public const string ServicePropServiceType = "ServiceType";
        public const string ServicePropServiceName = "ServiceName";
        public const string ServicePropPartitionId = "PartitionId";
        public const string ServicePropReplicationId = "ReplicationId";
        public const string ServicePropNode = "Node";
    }
}
