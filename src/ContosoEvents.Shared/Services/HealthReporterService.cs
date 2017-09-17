using ContosoEvents.Shared.Handlers;
using System;
using System.Fabric;
using System.Fabric.Health;

namespace ContosoEvents.Shared.Services
{
    public class HealthReporterService : IHealthReporterService
    {
        const string LOG_TAG = "HealthReporterService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        private Guid _partitionId;
        private long _replicaId;
        private string _nodeName;
        private string _serviceName;

        public HealthReporterService(ISettingService setting, ILoggerService logger, Guid partitionId, long replicaId, string nodeName, string serviceName)
        {
            _settingService = setting;
            _loggerService = logger;

            _partitionId = partitionId;
            _replicaId = replicaId;
            _nodeName = nodeName;
            _serviceName = serviceName;
        }

        public void SendReportForService(HealthState healthState, string message)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "SendReportForService", null);

            try
            {
                HealthReport healthReport = new StatefulServiceReplicaHealthReport(_partitionId,
                    _replicaId,
                    new HealthInformation(_serviceName, message, healthState));

                healthReport.HealthInformation.TimeToLive = TimeSpan.FromMinutes(_settingService.GetHealthIssuesTimeToLive());
                healthReport.HealthInformation.RemoveWhenExpired = false;
                ReportHealth(healthReport);

                SendReportForNode(healthState, message);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                /* Ignore */
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public void SendReportForNode(HealthState healthState, string message)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "SendReportForNode", null);

            try
            {
                HealthReport healthReport = new NodeHealthReport(_nodeName,
                    new HealthInformation(_serviceName, message, healthState));

                healthReport.HealthInformation.TimeToLive = TimeSpan.FromMinutes(_settingService.GetHealthIssuesTimeToLive());
                healthReport.HealthInformation.RemoveWhenExpired = false;
                ReportHealth(healthReport);

                SendReportForPartition(healthState, message);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                /* Ignore */
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public void SendReportForPartition(HealthState healthState, string message)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "SendReportForPartition", null);

            try
            {
                HealthReport healthReport = new PartitionHealthReport(_partitionId,
                    new HealthInformation(_serviceName, message, healthState));

                healthReport.HealthInformation.TimeToLive = TimeSpan.FromMinutes(_settingService.GetHealthIssuesTimeToLive());
                healthReport.HealthInformation.RemoveWhenExpired = false;
                ReportHealth(healthReport);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                /* Ignore */
            }
            finally
            {
                handler.Stop(error);
            }
        }

        // Private
        private static void ReportHealth(HealthReport healthReport)
        {
            var client = new FabricClient(new FabricClientSettings()
            {
                HealthReportSendInterval = TimeSpan.FromSeconds(0) // send immediately
            });

            client.HealthManager.ReportHealth(healthReport);
        }
    }
}
