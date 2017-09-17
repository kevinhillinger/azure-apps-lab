using Shared.Services;
using System;

namespace ContosoEvents.Shared.Services
{
    public class ServiceFactory : IServiceFactory
    {
        private static ServiceFactory _instance;

        public static ServiceFactory GetInstance()
        {
            if (_instance == null)
                _instance = new ServiceFactory();

            return _instance;
        }

        private ServiceFactory()
        {

        }

        public IActorLocationService GetActorLocationService()
        {
            return new ActorLocationService();
        }

        public IServiceLocationService GetServiceLocationService()
        {
            return new ServiceLocationService();
        }

        public IUriBuilderService GetUrilBuilderService(string serviceInstance)
        {
            return new UriBuilderService(serviceInstance);
        }

        public IUriBuilderService GetUrilBuilderService(string applicationInstance, string serviceInstance)
        {
            return new UriBuilderService(applicationInstance, serviceInstance);
        }

        public ILoggerService GetLoggerService()
        {
            return new LoggerService();
        }

        public ISettingService GetSettingService()
        {
            return new SettingService();
        }

        public IHealthReporterService GetHealtheReporterService(ISettingService setting, ILoggerService logger, Guid partitionId, long replicaId, string nodeName, string serviceName)
        {
            return new HealthReporterService(setting, logger, partitionId, replicaId, nodeName, serviceName);
        }

        public IDataStoreService GetDataStoreService(ISettingService setting, ILoggerService logger)
        {
            return new DataStoreService(setting, logger);
        }

        public IExternalizationService GetExternalizationService(ISettingService setting, ILoggerService logger)
        {
            return new ExternalizationService(setting, logger);
        }

        public INotificationService GetNotificationService(ISettingService setting, ILoggerService logger, IDataStoreService dataStore)
        {
            return new NotificationService(setting, logger, dataStore);
        }

        public IPaymentProcessorService GetPaymentProcessorService(ISettingService setting, ILoggerService logger)
        {
            return new PaymentProcessorService(setting, logger);
        }
    }
}
