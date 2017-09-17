using Shared.Services;
using System;
using System.Fabric;

namespace ContosoEvents.Shared.Services
{
    // http://stackoverflow.com/questions/35012146/where-to-store-configuration-values-in-azure-service-fabric-application            
    // http://stackoverflow.com/questions/33928204/where-do-you-set-and-access-run-time-configuration-parameters-per-environment-fo
    public class SettingService : ISettingService
    {
        private const string LOG_TAG = "SettingService";

        private const string ConfigurationSectionName = "ServiceRunTimeConfig";
        private const string StorageConnectionStringKey = "StorageConnectionString";
        private const string DataStorageEndpointUriKey = "DataStorageEndpointUri";
        private const string DataStoragePrimaryKey = "DataStoragePrimaryKey";
        private const string DataStorageDatabaseNameKey = "DataStorageDatabaseName";
        private const string DataStorageEventsCollectionNameKey = "DataStorageEventsCollectionName";
        private const string DataStorageOrdersCollectionNameKey = "DataStorageOrdersCollectionName";
        private const string DataStorageLogMessagesCollectionNameKey = "DataStorageLogMessagesCollectionName";
        private const string LogsTableStorageNameKey = "LogsStorageTableName";
        private const string ExternalizationQueueNameKey = "ExternalizationQueueName";
        private const string SimulationQueueNameKey = "SimulationQueueName";
        private const string BackupQueueNameKey = "BackupQueueName";
        private const string LogQueueNameKey = "LogQueueName";
        private const string IsTicketAvailabilityCheckKey = "IsTicketAvailabilityCheck";
        private const string IsEtwLoggingKey = "IsEtwLogging";
        private const string IsAzureTableStorageLoggingKey = "IsAzureTableStorageLogging";
        private const string IsAzureFunctionLoggingKey = "IsAzureFunctionLogging";
        private const string StatefulServiceLoopPauseKey = "StatefulServiceLoopPause";
        private const string ActorBackupReminderDueInMinutesKey = "ActorBackupReminderDueInMinutes";
        private const string ActorBackupReminderPeriodicInMinutesKey = "ActorBackupReminderPeriodicInMinutes";
        private const string HealthIssuesTimeToLiveKey = "HealthIssuesTimeToLive";
        private const string EmailServerUrlKey = "EmailServerUrl";
        private const string EmailServerPortKey = "EmailServerPort";
        private const string EmailServerUserNameKey = "EmailServerUserName";
        private const string EmailServerPasswordKey = "EmailServerPassword";

        private ICodePackageActivationContext _serviceContext;

        public SettingService()
        {
            // This is really neat! The FabricRuntime.GetActivationContext() gives me access to the config context!!! :-)
            _serviceContext = FabricRuntime.GetActivationContext();
        }

        public string GetAzureStorageConnectionString()
        {
            return GetSectionParameterValue(ConfigurationSectionName, StorageConnectionStringKey);
        }

        public string GetAzureStorageLogsTable()
        {
            return GetSectionParameterValue(ConfigurationSectionName, LogsTableStorageNameKey);
        }

        public string GetAzureStorageExternalizationQueueName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, ExternalizationQueueNameKey);
        }

        public string GetAzureStorageSimulationQueueName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, SimulationQueueNameKey);
        }

        public string GetAzureStorageBackupQueueName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, BackupQueueNameKey);
        }

        public string GetAzureStorageLogQueueName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, LogQueueNameKey);
        }

        public string GetDataStoreEndpointUri()
        {
            return GetSectionParameterValue(ConfigurationSectionName, DataStorageEndpointUriKey);
        }

        public string GetDataStorePrimaryKey()
        {
            return GetSectionParameterValue(ConfigurationSectionName, DataStoragePrimaryKey);
        }

        public string GetDataStoreDatabaseName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, DataStorageDatabaseNameKey);
        }

        public string GetDataStoreEventsCollectionName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, DataStorageEventsCollectionNameKey);
        }

        public string GetDataStoreOrdersCollectionName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, DataStorageOrdersCollectionNameKey);
        }

        public string GetDataStoreLogMessagesCollectionName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, DataStorageLogMessagesCollectionNameKey);
        }

        public bool IsTicketAvailabilityCheck()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, IsTicketAvailabilityCheckKey);
            bool isCheck = true;
            bool.TryParse(setting, out isCheck);
            return isCheck;
        }

        public bool IsEtwLogging()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, IsEtwLoggingKey);
            bool isLogging = true;
            bool.TryParse(setting, out isLogging);
            return isLogging;
        }

        public bool IsAzureStorageLogging()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, IsAzureTableStorageLoggingKey);
            bool isLogging = true;
            bool.TryParse(setting, out isLogging);
            return isLogging;
        }

        public bool IsAzureFunctionLogging()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, IsAzureFunctionLoggingKey);
            bool isLogging = true;
            bool.TryParse(setting, out isLogging);
            return isLogging;
        }

        public int GetStatefulServiceLoopPause()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, StatefulServiceLoopPauseKey);
            int pause = 1;
            Int32.TryParse(setting, out pause);
            return pause;
        }

        public int GetActorBackupReminderDueInMinutes()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, ActorBackupReminderDueInMinutesKey);
            int pause = 1;
            Int32.TryParse(setting, out pause);
            return pause;
        }

        public int GetActorBackupReminderPeriodicInMinutes()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, ActorBackupReminderPeriodicInMinutesKey);
            int pause = 1;
            Int32.TryParse(setting, out pause);
            return pause;
        }

        public int GetHealthIssuesTimeToLive()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, HealthIssuesTimeToLiveKey);
            int mins = 1;
            Int32.TryParse(setting, out mins);
            return mins;
        }

        public string GetEmailServerUrl()
        {
            return GetSectionParameterValue(ConfigurationSectionName, EmailServerUrlKey);
        }

        public int GetEmailServerPort()
        {
            var setting = GetSectionParameterValue(ConfigurationSectionName, EmailServerPortKey);
            int port = 0;
            Int32.TryParse(setting, out port);
            return port;
        }

        public string GetEmailServerUserName()
        {
            return GetSectionParameterValue(ConfigurationSectionName, EmailServerUserNameKey);
        }

        public string GetEmailServerPassword()
        {
            return GetSectionParameterValue(ConfigurationSectionName, EmailServerPasswordKey);
        }

        // ** PRIVATE **//
        private string GetSectionParameterValue(string section, string parameterKey)
        {
            try
            {
                if (_serviceContext == null)
                    return "";

                var parameterValue = "";
                var configurationPackage = _serviceContext.GetConfigurationPackageObject("Config");
                if (configurationPackage != null)
                {
                    var configSection = configurationPackage.Settings.Sections[ConfigurationSectionName];
                    if (configSection != null)
                    {
                        var connectorParameter = configSection.Parameters[parameterKey];
                        if (connectorParameter != null)
                        {
                            parameterValue = connectorParameter.Value;
                        }
                    }
                }

                return parameterValue;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
