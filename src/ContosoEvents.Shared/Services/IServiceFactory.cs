using Shared.Services;
using System;

namespace ContosoEvents.Shared.Services
{
    public interface IServiceFactory
    {
        IActorLocationService GetActorLocationService();
        IServiceLocationService GetServiceLocationService();
        IUriBuilderService GetUrilBuilderService(string serviceInstance);
        IUriBuilderService GetUrilBuilderService(string applicationInstance, string serviceInstance);
        ILoggerService GetLoggerService();
        ISettingService GetSettingService();
        IHealthReporterService GetHealtheReporterService(ISettingService setting, ILoggerService logger, Guid partitionId, long replicaId, string nodeName, string serviceName);
        IDataStoreService GetDataStoreService(ISettingService setting, ILoggerService logger);
        IExternalizationService GetExternalizationService(ISettingService setting, ILoggerService logger);
        INotificationService GetNotificationService(ISettingService setting, ILoggerService logger, IDataStoreService dataStore);
    }
}
