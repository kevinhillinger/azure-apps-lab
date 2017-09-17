using ContosoEvents.Models;
using ContosoEvents.Shared.Contracts;
using ContosoEvents.Shared.Handlers;
using ContosoEvents.Shared.Helpers;
using ContosoEvents.Shared.Services;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Fabric.Health;
using System.Threading.Tasks;

namespace ContosoEvents.TicketOrderActor
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
    internal class TicketOrderActor : Actor, ITicketOrderActor
    {
        const string LOG_TAG = "TicketOrderActor";

        const string ActorStatePropertyName = "MyState";

        /// <summary>
        /// Temporary property-injection for an ILoggerService, IActorLocationService until constructor injection is available.
        /// </summary>
        public IActorLocationService ActorLocationService { private get; set; }
        public ISettingService SettingService { private get; set; }
        public ILoggerService LoggerService { private get; set; }
        public IDataStoreService DataStoreService { private get; set; }
        public IHealthReporterService HealthReporterService { private get; set; }
        public IExternalizationService ExternalizationService { private get; set; }
        public INotificationService NotificationService { private get; set; }
        public IPaymentProcessorService PaymentProcessorService { private get; set; }

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
            this.NotificationService = ServiceFactory.GetInstance().GetNotificationService(this.SettingService, this.LoggerService, this.DataStoreService);
            this.PaymentProcessorService = ServiceFactory.GetInstance().GetPaymentProcessorService(this.SettingService, this.LoggerService);

            var error = "";
            var message = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "OnActivateAsync", GetActorProperties());

            try
            {
                TicketOrder state = await GetEntityStateAsync();
                if (state == null)
                {
                    // Make sure the state is saved
                    await SetEntityStateAsync(state);
                }

                state = await this.StateManager.GetStateAsync<TicketOrder>(ActorStatePropertyName);
                message = string.Format("Ticket Order Actor {0} activated", this.Id.GetStringId());
                handler.Info(message);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("OnActivateAsync", error));
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

        public async Task<TicketOrder> GetOrder()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "GetOrder", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<TicketOrder>(ActorStatePropertyName);
                return state;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            finally
            {
                handler.Stop(error);
                if (!string.IsNullOrEmpty(error))
                {
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("GetOrder", error));
                }
            }
        }

        public async Task ProcessOrder(TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "ProcessOrder", GetActorProperties());

            try
            {
                if (order == null)
                {
                    handler.Info("Process order is null");
                    return;
                }

                var state = await this.StateManager.GetStateAsync<TicketOrder>(ActorStatePropertyName);
                state = order;

                // Validate the order
                bool isValid = true;
                try
                {
                    TicketOrder.Validate(state);
                }
                catch (Exception e)
                {
                    state.Note = e.Message;
                    isValid = false;
                }

                if (isValid)
                {
                    // Validate the event 
                    TicketEvent tEvent = await this.DataStoreService.GetEventById(order.EventId);
                    if (tEvent == null)
                    {
                        state.FulfillDate = null;
                        state.IsFulfilled = false;
                        state.Note = "The Event ID is not valid!";
                        //TODO: Exercise 6 - Task 1 
                        //state.Status = OrderStatuses.Invalid;
                    }
                    else
                    {
                        state.PricePerTicket = tEvent.PricePerTicket;
                        state.Currency = tEvent.Currency;
                        state.Price = state.Tickets * state.PricePerTicket;

                        // Locate the event actor
                        IEventActor eventActor = this.ActorLocationService.Create<IEventActor>(new ActorId(order.EventId), Constants.ContosoEventsApplicationName);
                        bool isAvailable = await eventActor.ReserveTickets(order);
                        if (isAvailable || !SettingService.IsTicketAvailabilityCheck())
                        {
                            // Charge credit card
                            string confirmationNumber = await this.PaymentProcessorService.Authorize(order);
                            if (!string.IsNullOrEmpty(confirmationNumber))
                            {
                                state.PaymentProcessorConfirmation = confirmationNumber;
                                state.FulfillDate = DateTime.Now;
                                state.IsFulfilled = true;
                                //TODO: Exercise 6 - Task 1 
                                //state.Status = OrderStatuses.Fufilled;
                            }
                            else
                            {
                                state.FulfillDate = null;
                                state.IsFulfilled = false;
                                state.Note = "Credit card failed to authorize!";
                                await eventActor.FailTickets(order);
                                //TODO: Exercise 6 - Task 1 
                                //state.Status = OrderStatuses.CreditCardDenied;
                            }
                        }
                        else
                        {
                            state.FulfillDate = null;
                            state.IsFulfilled = false;
                            state.Note = "Event Tickets are exhausted!";
                            //TODO: Exercise 6 - Task 1 
                            //state.Status = OrderStatuses.TicketsExhausted;
                        }
                    }
                }
                else
                {
                    state.FulfillDate = null;
                    state.IsFulfilled = false;
                    //TODO: Exercise 6 - Task 1 
                    //state.Status = OrderStatuses.Invalid;
                }

                // Make sure the state is saved
                await SetEntityStateAsync(state);

                // Externalize the state
                await this.ExternalizationService.Externalize(state);

                // Notify the user
                await this.NotificationService.Notify(state);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("ProcessOrder", error));
                }
            }
        }

        public async Task CancelOrder()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(SettingService, LoggerService);
            handler.Start(LOG_TAG, "CancelOrder", GetActorProperties());

            try
            {
                var state = await this.StateManager.GetStateAsync<TicketOrder>(ActorStatePropertyName);
                if (state == null)
                {
                    handler.Info("Cancel order is null");
                    return;
                }

                // Locate the event actor
                IEventActor eventActor = this.ActorLocationService.Create<IEventActor>(new ActorId(state.EventId), Constants.ContosoEventsApplicationName);
                await eventActor.CancelTickets(state);
                // Refund credit card
                await this.PaymentProcessorService.Refund(state);
                state.CancellationDate = DateTime.Now;
                state.IsFulfilled = false;
                state.IsCancelled = true;
                //TODO: Exercise 6 - Task 1 
                //state.Status = OrderStatuses.Cancelled;

                // Make sure the state is saved
                await SetEntityStateAsync(state);

                // Externalize the state
                await this.ExternalizationService.Externalize(state);

                // Notify the user
                await this.NotificationService.Notify(state);
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
                    this.HealthReporterService.SendReportForService(HealthState.Error, GetHealthErrorMessage("CancelOrder", error));
                }
            }
        }

        public async Task UpdateHealthState(HealthState state, string message)
        {
            this.HealthReporterService.SendReportForService(state, message);
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

        private async Task<TicketOrder> GetEntityStateAsync()
        {
            ConditionalValue<TicketOrder> stateResult = await this.StateManager.TryGetStateAsync<TicketOrder>(ActorStatePropertyName);
            if (stateResult.HasValue)
            {
                return stateResult.Value;
            }
            else
            {
                return null;
            }
        }

        private async Task SetEntityStateAsync(TicketOrder state)
        {
            await this.StateManager.SetStateAsync<TicketOrder>(ActorStatePropertyName, state);
            // Just to make sure though it is probably not needed
            await this.SaveStateAsync();
        }
    }
}

