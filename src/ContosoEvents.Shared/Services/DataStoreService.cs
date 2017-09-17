using ContosoEvents.Models;
using ContosoEvents.Shared.Handlers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    /// <summary>
    /// Data Store Service that communicates with DocDB.
    /// </summary>
    public class DataStoreService : IDataStoreService
    {
        const string LOG_TAG = "DataStoreService";

        private string _endpointUri;
        private string _primaryKey;
        private string _databaseName;
        private string _eventsCollectionName;
        private string _ordersCollectionName;
        private string _logMessagesCollectionName;

        private DocumentClient _docDbClient;

        private List<TicketEvent> _events = new List<TicketEvent>();

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public DataStoreService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;

            _endpointUri = _settingService.GetDataStoreEndpointUri();
            _primaryKey = _settingService.GetDataStorePrimaryKey();
            _databaseName = _settingService.GetDataStoreDatabaseName();
            _eventsCollectionName = _settingService.GetDataStoreEventsCollectionName();
            _ordersCollectionName = _settingService.GetDataStoreOrdersCollectionName();
            _logMessagesCollectionName = _settingService.GetDataStoreLogMessagesCollectionName();

            // Initialize Data Store
            Initialize().Wait();
        }

        public async Task<List<TicketEvent>> GetEvents()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "GetEvents", null);

            try
            {
                if (this._docDbClient == null)
                    return new List<TicketEvent>();

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                IQueryable<TicketEvent> eventsQuery = this._docDbClient.CreateDocumentQuery<TicketEvent>(
                                UriFactory.CreateDocumentCollectionUri(_databaseName, _eventsCollectionName), queryOptions);

                return eventsQuery.ToList();
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

        public async Task<TicketEvent> GetEventById(string eventId)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "GetEventById", null);

            try
            {
                if (this._docDbClient == null)
                    return null;

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                return this._docDbClient.CreateDocumentQuery<TicketEvent>(
                                UriFactory.CreateDocumentCollectionUri(_databaseName, _eventsCollectionName), queryOptions)
                                .Where(e => e.Id == eventId).AsEnumerable().FirstOrDefault();
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

        public async Task<List<TicketOrder>> GetOrdersByUserName(string userName)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "GetOrdersByUserName", null);

            try
            {
                if (this._docDbClient == null)
                    return new List<TicketOrder>();

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                IQueryable<TicketOrder> ordersQuery = this._docDbClient.CreateDocumentQuery<TicketOrder>(
                                UriFactory.CreateDocumentCollectionUri(_databaseName, _ordersCollectionName), queryOptions)
                                .Where(o => o.UserName.ToLower() == userName)
                                .OrderByDescending(o => o.OrderDate);

                return ordersQuery.ToList();
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

        public async Task<List<TicketOrderStats>> GetOrderStats()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "GetOrderStats", null);

            try
            {
                if (this._docDbClient == null)
                    return new List<TicketOrderStats>();

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                IQueryable<TicketOrder> ordersQuery = this._docDbClient.CreateDocumentQuery<TicketOrder>(
                                UriFactory.CreateDocumentCollectionUri(_databaseName, _ordersCollectionName), queryOptions).
                                Where(o => o.FulfillDate != null);

                var orders = ordersQuery.ToList();
                return orders.GroupBy(o => new { ID = o.Tag }).Select(g => new TicketOrderStats { Tag = g.Key.ID, Count = g.Count(), SumSeconds = g.Sum(x => ((DateTime)x.FulfillDate - x.OrderDate).TotalSeconds), AverageSeconds = g.Average(x => ((DateTime)x.FulfillDate - x.OrderDate).TotalSeconds) }).ToList();
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

        public async Task<string> CreateEvent(TicketEvent tEvent)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "CreateEvent", null);
            var id = tEvent.Id;

            try
            {
                if (this._docDbClient == null)
                    return "";

                await this._docDbClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, _eventsCollectionName), tEvent);
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

            return id;
        }

        public async Task<TicketEventStats> GetEventStats(string eventId)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "GetEventStats", null);

            try
            {
                if (this._docDbClient == null)
                    return new TicketEventStats();

                TicketEvent tEvent = await GetEventById(eventId);
                if (tEvent == null)
                    return new TicketEventStats();

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                IQueryable<TicketOrder> ordersQuery = this._docDbClient.CreateDocumentQuery<TicketOrder>(
                                UriFactory.CreateDocumentCollectionUri(_databaseName, _ordersCollectionName), queryOptions).
                                Where(o => o.EventId.ToLower() == eventId.ToLower());

                var orders = ordersQuery.ToList();
                var requestedCount = orders.Sum(o => o.Tickets);
                var failedCount = orders.Where(o => o.IsFulfilled == false && o.IsCancelled == false).Sum(o => o.Tickets);
                var canxCount = orders.Where(o => o.IsFulfilled == false && o.IsCancelled == true).Sum(o => o.Tickets);
                return new TicketEventStats()
                {
                    Tickets = tEvent.TotalTickets,
                    RequestedTickets = requestedCount,
                    FailedTickets = failedCount,
                    CancelledTickets = canxCount,
                    Orders = orders.Count
                };
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

        public async Task DeleteAllEvents()
        {
            await DeleteCollectionIems(_eventsCollectionName);
        }

        public async Task DeleteAllOrders()
        {
            await DeleteCollectionIems(_ordersCollectionName);
        }

        public async Task DeleteAllLogMessages()
        {
            await DeleteCollectionIems(_logMessagesCollectionName);
        }

        // PRIVATE
        private async Task DeleteCollectionIems(string collectionName)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "DeleteCollectionIems", null);

            try
            {
                if (this._docDbClient == null)
                    return;

                var db = this._docDbClient.CreateDatabaseQuery().ToList().First();
                var collection = this._docDbClient.CreateDocumentCollectionQuery(db.CollectionsLink).Where(c => c.Id == collectionName).ToList().FirstOrDefault();
                if (collection != null)
                {
                    var docs = this._docDbClient.CreateDocumentQuery(collection.DocumentsLink);
                    foreach (var doc in docs)
                    {
                        await this._docDbClient.DeleteDocumentAsync(doc.SelfLink);
                    }
                }
                else
                    throw new Exception("Unable to get the collection [" + collectionName + "]");
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

        private async Task Initialize()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(_settingService, _loggerService);
            handler.Start(LOG_TAG, "Initialize", null);

            try
            {
                _events = new List<TicketEvent>();

                var event1 = new TicketEvent()
                {
                    Id = "EVENT1-ID-00001",
                    Name = "Seattle Rock and Rollers",
                    Summary = "Seattle Rock and Rollers Summary",
                    Description = "Seattle Rock and Rollers Description",
                    ImageUrl = "http://www.loremimages.com/gen.php?size=300x200&bg=0000&fg=fff&format=png",
                    Latitude = 0,
                    Longitude = 0,
                    StartDate = DateTime.Now.AddDays(-60),
                    EndDate = DateTime.Now.AddDays(60),
                    TotalTickets = 100000,
                    PricePerTicket = 25,
                    Currency = "USD",
                    PaymentProcessorUrl = "",
                    PaymentProcessorAccount = "",
                    PaymentProcessorPassword = "",
                    SuccessEmailTemplate = @"
                    <h1> Ticket Purchase Notification </h1>
                    Dear @Model.UserName, 
                    <p>
                    Congratulations! Your have @Model.Tickets tickets guranateed to Seattle Rock and Rollers.
                    </p>
                    ",
                    FailedEmailTemplate = @"
                    <h1> Ticket Purchase Failure Notification </h1>
                    Dear @Model.UserName, 
                    <p>
                    Sorry! Your @Model.Tickets tickets could not be purchased to Seattle Rock and Rollers.
                    </p>
                    ",
                    SuccessSmsTemplate = @"
                    <h1> Ticket Purchase Notification </h1>
                    Dear @Model.UserName, 
                    <p>
                    Congratulations! Your have @Model.Tickets tickets guranateed to Seattle Rock and Rollers.
                    </p>
                    ",
                    FailedSmsTemplate = @"
                    <h1> Ticket Purchase Failure Notification </h1>
                    Dear @Model.UserName, 
                    <p>
                    Sorry! Your @Model.Tickets tickets could not be purchased to Seattle Rock and Rollers.
                    </p>
                    "
                };

                _events.Add(event1);

                if (string.IsNullOrEmpty(_endpointUri) ||
                    string.IsNullOrEmpty(_primaryKey)
                    )
                {
                    return; // Exit gracefully
                }

                // Seed events
                _docDbClient = new DocumentClient(new Uri(_endpointUri), _primaryKey);

                try
                {
                    await this._docDbClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(_databaseName, _eventsCollectionName, event1.Id));
                }
                catch (DocumentClientException de)
                {
                    if (de.StatusCode == HttpStatusCode.NotFound)
                    {
                        await this._docDbClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, _eventsCollectionName), event1);
                    }
                    else
                    {
                        throw;
                    }
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
    }
}
