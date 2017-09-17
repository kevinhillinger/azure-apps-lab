using ContosoEvents.Models;
using ContosoEvents.Shared.Contracts;
using ContosoEvents.Shared.Handlers;
using ContosoEvents.Shared.Helpers;
using ContosoEvents.Shared.Services;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoEvents.TicketOrderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TicketOrderService : StatefulService, ITicketOrderService
    {
        private const string LOG_TAG = "TicketOrderService";
        private const string OrderQueueName = "OrderQueue";

        private static TimeSpan TxTimeout = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Temporary property-injection for an ILoggerService, IActorLocationService & IMosaicConnectorService until constructor injection is available.
        /// </summary>
        public ISettingService SettingService { private get; set; }
        public ILoggerService LoggerService { private get; set; }
        public IActorLocationService ActorLocationService { private get; set; }
        public IHealthReporterService HealthReporterService { private get; set; }

        public TicketOrderService(StatefulServiceContext context)
            : base(context)
        {
            this.SettingService = ServiceFactory.GetInstance().GetSettingService();
            this.LoggerService = ServiceFactory.GetInstance().GetLoggerService();
            this.ActorLocationService = ServiceFactory.GetInstance().GetActorLocationService();
            this.HealthReporterService = ServiceFactory.GetInstance().GetHealtheReporterService(this.SettingService, this.LoggerService, Context.PartitionId, Context.ReplicaId, Context.NodeContext.NodeName, Context.ServiceName.ToString());

            //TODO: Exercise 7 - Task 1
            // Uncomment this line to cause a simulated failure upon upgrade!
            //this.HealthReporterService.SendReportForService(HealthState.Error, "Simulated Error");
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
                {
                    new ServiceReplicaListener(context =>
                        this.CreateServiceRemotingListener(context),
                        "rpcPrimaryEndpoint",
                        false)
                };
        }

        // ITicketDispenserService Interface Implementation
        public async Task<string> EnqueueOrder(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "EnqueueOrder", GetServiceProperties());
            string orderId = Guid.NewGuid().ToString();
            bool blProceed = true;

            try
            {
                // Do some sanity checking
                TicketOrder.Validate(order);

                // Quick check to see if tickets are available
                if (SettingService.IsTicketAvailabilityCheck())
                {
                    IEventActor eventActor = this.ActorLocationService.Create<IEventActor>(new ActorId(order.EventId), Constants.ContosoEventsApplicationName);
                    if (!await eventActor.CheckTickets(order))
                    {
                        handler.Info("Processing order - Event Id: " + order.EventId + " - Order Id: " + orderId + " - tickets: " + order.Tickets + " - Tickets are exhausted!");
                        // Do not throw an exception because we will get a health issue
                        // throw new Exception("Tickets are exhausted!");
                        // Need to alert the admin only as tickets are exhausted

                        // Prevent proceeding with the order
                        blProceed = false;
                        // Reset the order id to indicate to the Web API client that the order did not go through
                        orderId = ""; 
                    }
                }

                if (blProceed)
                {
                    order.Id = orderId;
                    handler.Info("Processing order - Event Id: " + order.EventId + " - Order Id: " + orderId + " - tickets: " + order.Tickets);

                    // Gets (or creates) a replicated queue called "OrderQueue" in this partition.
                    var requests = await this.StateManager.GetOrAddAsync<IReliableQueue<TicketOrder>>(OrderQueueName);

                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        await requests.EnqueueAsync(tx, order);
                        await tx.CommitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                handler.Stop(error);
                if (!string.IsNullOrEmpty(error))
                {
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("EnqueueOrder", error));
                }
            }

            return orderId;
        }

        public async Task<string> GetNodeName()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "GetNodeName", GetServiceProperties());
            string nodeName = "Unknown";

            try
            {
                nodeName = Context.NodeContext.NodeName;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                handler.Stop(error);
                if (!string.IsNullOrEmpty(error))
                {
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("GetNodeName", error));
                }
            }

            return nodeName;
        }

        public async Task<int> GetOrdersCounter(CancellationToken ct)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "GetOrdersCounter", GetServiceProperties());
            IList<TicketOrder> items = new List<TicketOrder>();

            try
            {
                var requests = await this.StateManager.GetOrAddAsync<IReliableQueue<TicketOrder>>(OrderQueueName);

                using (var tx = this.StateManager.CreateTransaction())
                {
                    IAsyncEnumerator<TicketOrder> enumerator = (await requests.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(ct))
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw ex;
            }
            finally
            {
                handler.Stop(error);
                if (!string.IsNullOrEmpty(error))
                {
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("GetOrdersCounter", error));
                }
            }

            return items.Count;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Gets (or creates) a replicated queue called "OrderQueue" in this partition.
            var requests = await this.StateManager.GetOrAddAsync<IReliableQueue<TicketOrder>>(OrderQueueName);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await requests.TryDequeueAsync(tx, TxTimeout, cancellationToken);

                    if (result.HasValue)
                    {
                        var error = "";
                        var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
                        handler.Start(LOG_TAG, "AcquiredQueueItem", GetServiceProperties());

                        try
                        {
                            TicketOrder order = result.Value;

                            IActorLocationService locator = ServiceFactory.GetInstance().GetActorLocationService();
                            ITicketOrderActor orderActor = locator.Create<ITicketOrderActor>(new ActorId(order.Id), Constants.ContosoEventsApplicationName);
                            await orderActor.ProcessOrder(order);

                        }
                        catch (Exception ex)
                        {
                            error = ex.Message;
                        }
                        finally
                        {
                            handler.Stop(error);
                            if (!string.IsNullOrEmpty(error))
                            {
                                this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("AcquiredQueueItem", error));
                            }
                        }

                        // This commits the dequeue operations.
                        // If the request to add the stock to the inventory service throws, this commit will not execute
                        // and the items will remain on the queue, so we can be sure that we didn't dequeue items
                        // that didn't get saved successfully in the inventory service.
                        // However there is a very small chance that the stock was added to the inventory service successfully,
                        // but service execution stopped before reaching this commit (machine crash, for example).
                        await tx.CommitAsync();
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(this.SettingService.GetStatefulServiceLoopPause()), cancellationToken);
                }
            }
        }

        public async Task UpdateHealthState(HealthState state, string message)
        {
            this.HealthReporterService.SendReportForService(state, message);
        }

        // PRIVATE
        private string GetHealthErrorMessage(string method, string message)
        {
            return LOG_TAG + " - " + method + " - " + message;
        }

        private Dictionary<string, string> GetServiceProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, GetType().ToString());
            props.Add(Constants.ServicePropId, Context.ReplicaOrInstanceId.ToString());
            props.Add(Constants.ServicePropApplicationType, Context.CodePackageActivationContext.ApplicationTypeName);
            props.Add(Constants.ServicePropApplicationName, Context.CodePackageActivationContext.ApplicationName);
            props.Add(Constants.ServicePropServiceType, Context.ServiceTypeName);
            props.Add(Constants.ServicePropServiceName, Context.ServiceName.ToString());
            props.Add(Constants.ServicePropPartitionId, Context.PartitionId + "");
            props.Add(Constants.ServicePropReplicationId, Context.ReplicaId + "");
            props.Add(Constants.ServicePropNode, Context.NodeContext.NodeName);
            return props;
        }
    }
}
