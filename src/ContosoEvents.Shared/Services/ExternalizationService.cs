using System;
using System.Threading.Tasks;
using ContosoEvents.Models;
using Microsoft.WindowsAzure.Storage.Queue;
using ContosoEvents.Shared.Handlers;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using ContosoEvents.Shared.Contracts;

namespace ContosoEvents.Shared.Services
{
    public class ExternalizationService : IExternalizationService
    {
        const string LOG_TAG = "ExternalizationService";

        private CloudQueue _externalizationQueue;
        private CloudQueue _simulationQueue;
        private CloudQueue _backupQueue;

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public ExternalizationService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;

            InitializeStorage();
        }

        public async Task Externalize(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Externalize", null);

            try
            {
                // Enqueue the event to a queue so it can be processed by an Azure function or whatever
                if (_externalizationQueue != null)
                {
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(order));
                    await _externalizationQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task Simulate(TicketOrderSimulationRequest request)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Simulate", null);

            try
            {
                // Enqueue the event to a queue so it can be processed by an Azure function or whatever
                if (_simulationQueue != null)
                {
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(request));
                    await _simulationQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        public async Task Backup(EventActorState eventState)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Backup", null);

            try
            {
                // Enqueue the event to a queue so it can be processed by an Azure function or whatever
                if (_backupQueue != null)
                {
                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(eventState));
                    await _backupQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        private void InitializeStorage()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "InitializeStorage", null);

            try
            {
                // Open storage account using credentials from .cscfg file.
                // This is a common storage for the emails
                var storageAccount = CloudStorageAccount.Parse(_settingService.GetAzureStorageConnectionString());

                // Get context object for working with queues, and 
                // set a default retry policy appropriate for a web user interface.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                //queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

                var externalizationQueueName = _settingService.GetAzureStorageExternalizationQueueName();
                if (!string.IsNullOrEmpty(externalizationQueueName))
                {
                    _externalizationQueue = queueClient.GetQueueReference(externalizationQueueName);
                    _externalizationQueue.CreateIfNotExists();
                }

                var simulationQueueName = _settingService.GetAzureStorageSimulationQueueName();
                if (!string.IsNullOrEmpty(simulationQueueName))
                {
                    _simulationQueue = queueClient.GetQueueReference(simulationQueueName);
                    _simulationQueue.CreateIfNotExists();
                }

                var backupQueueName = _settingService.GetAzureStorageBackupQueueName();
                if (!string.IsNullOrEmpty(backupQueueName))
                {
                    _backupQueue = queueClient.GetQueueReference(backupQueueName);
                    _backupQueue.CreateIfNotExists();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                handler.Stop(error);
            }
        }
    }
}
