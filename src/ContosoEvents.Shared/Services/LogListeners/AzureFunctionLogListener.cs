using ContosoEvents.Models;
using ContosoEvents.Shared.Services.LogSources;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;

namespace ContosoEvents.Shared.Services.LogListeners
{
    public class AzureFunctionLogListener : GenericLogListener
    {
        private CloudQueue _logQueue;

        public AzureFunctionLogListener(ILogMessageEventSource source, ISettingService setting) : 
            base(source, setting) 
        {

            InitializeStorage();
        }

        public override void ProcessLogMessage(LogMessage message)
        {
            try
            {
                // Enqueue the event to a queue so it can be processed by an Azure function or whatever
                if (_logQueue != null)
                {
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message));
                    _logQueue.AddMessage(queueMessage);
                }
            }
            catch (Exception ex)
            {
                /* Ignore for now */;
            }
        }

        // PRIVATE
        private void InitializeStorage()
        {
            var error = "";

            try
            {
                // Open storage account using credentials from .cscfg file.
                // This is a common storage for the emails
                var storageAccount = CloudStorageAccount.Parse(TheSettingService.GetAzureStorageConnectionString());

                // Get context object for working with queues, and 
                // set a default retry policy appropriate for a web user interface.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                //queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

                var logQueueName = TheSettingService.GetAzureStorageLogQueueName();
                if (!string.IsNullOrEmpty(logQueueName))
                {
                    _logQueue = queueClient.GetQueueReference(logQueueName);
                    _logQueue.CreateIfNotExists();
                }
            }
            catch (Exception ex)
            {
                /* Ignore for now */
                ;
            }
        }
    }
}
