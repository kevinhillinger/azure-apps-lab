using ContosoEvents.Models;
using ContosoEvents.Shared.Contracts;
using ContosoEvents.Shared.Handlers;
using ContosoEvents.Shared.Helpers;
using ContosoEvents.Shared.Services;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoEvents.EventActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class EventActor : Actor, IEventActor, IRemindable
    {
        const string LOG_TAG = "EventActor";

        const string ActorStatePropertyName = "MyState";

        const string BackupReminder = "BackupReminder";

        /// <summary>
        /// Temporary property-injection for an ILoggerService, IActorLocationService until constructor injection is available.
        /// </summary>
        public IActorLocationService ActorLocationService { private get; set; }
        public ISettingService SettingService { private get; set; }
        public ILoggerService LoggerService { private get; set; }
        public IDataStoreService DataStoreService { private get; set; }
        public IHealthReporterService HealthReporterService { private get; set; }
        public IExternalizationService ExternalizationService { private get; set; }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            this.ActorLocationService = ServiceFactory.GetInstance().GetActorLocationService();
            this.SettingService = ServiceFactory.GetInstance().GetSettingService();
            this.LoggerService = ServiceFactory.GetInstance().GetLoggerService();
            this.DataStoreService = ServiceFactory.GetInstance().GetDataStoreService(this.SettingService, this.LoggerService);
            this.HealthReporterService = ServiceFactory.GetInstance().GetHealtheReporterService(this.SettingService, this.LoggerService, ActorService.Context.PartitionId, ActorService.Context.ReplicaId, ActorService.Context.NodeContext.NodeName, ActorService.Context.ServiceName.ToString());
            this.ExternalizationService = ServiceFactory.GetInstance().GetExternalizationService(this.SettingService, this.LoggerService);

            var error = "";
            var message = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "OnActivateAsync", GetActorProperties());

            try
            {
                EventActorState state = await GetEntityStateAsync();
                if (state == null)
                {
                    await Reload();
                }

                state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                message = string.Format("Event Actor {0} activated", this.Id.GetStringId());
                handler.Info(message);

                handler.Info("Starting a reminder to schedule state backups every " + SettingService.GetActorBackupReminderPeriodicInMinutes() + "mins");
                await this.RegisterReminderAsync(
                    BackupReminder,
                    null,
                    TimeSpan.FromMinutes(SettingService.GetActorBackupReminderDueInMinutes()),            // Fire initially 5 minutes from now
                    TimeSpan.FromMinutes(SettingService.GetActorBackupReminderPeriodicInMinutes()));     // Every 60 minutes periodic firing
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, error);
                }
            }
        }

        protected override Task OnDeactivateAsync()
        {
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "OnDeactivateAsync", GetActorProperties());
            handler.Stop();
            return base.OnDeactivateAsync();
        }

        public async Task Reload()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "Reload", GetActorProperties());

            try
            {
                // Given the actor id (which is the event id), re-load the event stats
                TicketEventStats stats = await DataStoreService.GetEventStats(this.Id.GetStringId());

                var state = new EventActorState(stats);

                // Make sure the state is saved
                await SetEntityStateAsync(state);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("GetStats", error));
                }
            }
        }

        public async Task<TicketEventStats> GetStats()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "GetStats", GetActorProperties());
            var stats = new TicketEventStats();

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                stats = new TicketEventStats()
                {
                    Tickets = state.Tickets,
                    RequestedTickets = state.RequestedTickets,
                    FailedTickets = state.FailedTickets,
                    CancelledTickets = state.CancelledTickets,
                    Orders = state.Orders.Count
                };
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("GetStats", error));
                }
            }

            return stats;
        }

        public async Task<List<TicketOrder>> GetHistoricalOrders()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "GetHistoricalOrders", GetActorProperties());
            List<TicketOrder> orders = new List<TicketOrder>();

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                orders = state.Orders.OrderBy(o => o.OrderDate).ToList();
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("GetHistoricalOrders", error));
                }
            }

            return orders;
        }

        public async Task<bool> CheckTickets(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "CheckTickets", GetActorProperties());
            bool isAvailable = false;

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                isAvailable = ((state.Tickets - state.RequestedTickets + state.FailedTickets + state.CancelledTickets) >= order.Tickets);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("CheckTickets", error));
                }
            }

            return isAvailable;
        }

        public async Task<bool> ReserveTickets(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "ReserveTickets", GetActorProperties());
            bool isAvailable = false;

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                if ((state.Tickets - state.RequestedTickets + state.FailedTickets + state.CancelledTickets) >= order.Tickets)
                {
                    isAvailable = true;
                    state.RequestedTickets += order.Tickets;
                    state.Orders.Add(order);
                    await SetEntityStateAsync(state);
                }
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("ReserveTickets", error));
                }
            }

            return isAvailable;
        }

        public async Task FailTickets(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "FailTickets", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                state.FailedTickets += order.Tickets;
                state.Orders.Add(order);
                await SetEntityStateAsync(state);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("FailTickets", error));
                }
            }
        }

        public async Task CancelTickets(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "CancelTickets", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);
                state.CancelledTickets += order.Tickets;
                state.Orders.Add(order);
                await SetEntityStateAsync(state);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("CancelTickets", error));
                }
            }
        }

        public async Task UpdateHealthState(HealthState state, string message)
        {
            this.HealthReporterService.SendReportForService(state, message);
        }

        // IRemindable Interface Implementation
        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "ReceiveReminderAsync", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<EventActorState>(ActorStatePropertyName);

                switch (reminderName)
                {
                    case BackupReminder:
                        {
                            await this.ExternalizationService.Backup(state);
                            return;
                        }
                    default:
                        {
                            // We should never arrive here normally. The system won't call reminders that don't exist. 
                            // But for our own sake in case we add a new reminder somewhere and forget to handle it, this will remind us.
                            handler.Info("Unknown reminder: " + reminderName);
                            return;
                        }
                }
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("ReceiveReminderAsync", error));
                }
            }
        }

        // Private
        private string GetHealthErrorMessage(string method, string message)
        {
            return LOG_TAG + " - " + method + " - " + message;
        }

        private Dictionary<string, string> GetActorProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, GetType().ToString());
            props.Add(Constants.ServicePropId, Id.ToString());
            props.Add(Constants.ServicePropApplicationType, ActorService.Context.CodePackageActivationContext.ApplicationTypeName);
            props.Add(Constants.ServicePropApplicationName, ActorService.Context.CodePackageActivationContext.ApplicationName);
            props.Add(Constants.ServicePropServiceType, ActorService.Context.ServiceTypeName);
            props.Add(Constants.ServicePropServiceName, ActorService.Context.ServiceName.ToString());
            props.Add(Constants.ServicePropPartitionId, ActorService.Context.PartitionId + "");
            props.Add(Constants.ServicePropReplicationId, ActorService.Context.ReplicaId + "");
            props.Add(Constants.ServicePropNode, ActorService.Context.NodeContext.NodeName);
            return props;
        }

        private async Task<EventActorState> GetEntityStateAsync()
        {
            ConditionalValue<EventActorState> stateResult = await this.StateManager.TryGetStateAsync<EventActorState>(ActorStatePropertyName);
            if (stateResult.HasValue)
            {
                return stateResult.Value;
            }
            else
            {
                return null;
            }
        }

        private async Task SetEntityStateAsync(EventActorState state)
        {
            await this.StateManager.SetStateAsync<EventActorState>(ActorStatePropertyName, state);
            // Just to make sure though it is probably not needed
            await this.SaveStateAsync();
        }
    }
}
