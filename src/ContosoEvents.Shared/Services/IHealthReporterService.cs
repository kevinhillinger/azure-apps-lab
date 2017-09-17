using System.Fabric.Health;

namespace ContosoEvents.Shared.Services
{
    public interface IHealthReporterService
    {
        void SendReportForService(HealthState healthState, string message);
        void SendReportForNode(HealthState healthState, string message);
        void SendReportForPartition(HealthState healthState, string message);
    }
}
