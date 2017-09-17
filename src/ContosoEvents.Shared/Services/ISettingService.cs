namespace ContosoEvents.Shared.Services
{
    public interface ISettingService
    {
        string GetAzureStorageConnectionString();
        string GetAzureStorageLogsTable();
        string GetAzureStorageExternalizationQueueName();
        string GetAzureStorageSimulationQueueName();
        string GetAzureStorageBackupQueueName();
        string GetAzureStorageLogQueueName();
        string GetDataStoreEndpointUri();
        string GetDataStorePrimaryKey();
        string GetDataStoreDatabaseName();
        string GetDataStoreEventsCollectionName();
        string GetDataStoreOrdersCollectionName();
        string GetDataStoreLogMessagesCollectionName();
        bool IsTicketAvailabilityCheck();
        bool IsEtwLogging();
        bool IsAzureStorageLogging();
        bool IsAzureFunctionLogging();
        int GetStatefulServiceLoopPause();
        int GetActorBackupReminderDueInMinutes();
        int GetActorBackupReminderPeriodicInMinutes();
        int GetHealthIssuesTimeToLive();
        string GetEmailServerUrl();
        int GetEmailServerPort();
        string GetEmailServerUserName();
        string GetEmailServerPassword();
    }
}
