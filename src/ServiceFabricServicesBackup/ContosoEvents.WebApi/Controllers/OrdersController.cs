using ContosoEvents.Models;
using ContosoEvents.Shared.Contracts;
using ContosoEvents.Shared.Handlers;
using ContosoEvents.Shared.Helpers;
using ContosoEvents.Shared.Services;
using Microsoft.ServiceFabric.Actors;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoEvents.WebApi.Controllers
{
    public class OrdersController : GenericApiController
    {
        protected const string LOG_TAG = "OrdersController";
        private static FabricClient _fabricClient = new FabricClient();

        public OrdersController() :
            base()
        {
        }

        [SwaggerOperation("GetOrdersByUserName")]
        [SwaggerResponse(HttpStatusCode.OK, "Ticket Orders By User Name", typeof(List<TicketOrder>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/orders/user/{username}", Name = "GetOrdersByUserName")]
        public async Task<IHttpActionResult> GetOrdersByUserName(string username)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetOrdersByUserName", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                return Ok(await dataService.GetOrdersByUserName(username));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [SwaggerOperation("GetHistoricalOrdersByEventId")]
        [SwaggerResponse(HttpStatusCode.OK, "Historical Ticket Orders By Event Id", typeof(List<TicketOrder>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/orders/event/{eventid}", Name = "GetHistoricalOrdersByEventId")]
        public async Task<IHttpActionResult> GetHistricalOrdersByEventId(string eventid)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetEventStatsById", GetServiceProperties());

            try
            {
                IActorLocationService locator = ServiceFactory.GetInstance().GetActorLocationService();
                IEventActor eventActor = locator.Create<IEventActor>(new ActorId(eventid), Constants.ContosoEventsApplicationName);
                return Ok(await eventActor.GetHistoricalOrders());
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [SwaggerOperation("GetOrderById")]
        [SwaggerResponse(HttpStatusCode.OK, "Ticket Orders By Id", typeof(TicketOrder))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/orders/{id}", Name = "GetOrderById")]
        public async Task<IHttpActionResult> GetOrderById(string id)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetOrderById", GetServiceProperties());

            try
            {
                IActorLocationService locator = ServiceFactory.GetInstance().GetActorLocationService();
                ITicketOrderActor orderActor = locator.Create<ITicketOrderActor>(new ActorId(id), Constants.ContosoEventsApplicationName);
                return Ok(await orderActor.GetOrder());
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [SwaggerOperation("GetOrderStats")]
        [SwaggerResponse(HttpStatusCode.OK, "Get Order Stats", typeof(List<TicketOrderStats>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/orders/stats", Name = "GetOrderStats")]
        public async Task<IHttpActionResult> GetOrderStats()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetOrderStats", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                return Ok(await dataService.GetOrderStats());
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [SwaggerOperation("PostNewOrder")]
        [SwaggerResponse(HttpStatusCode.OK, "Create a new Ticket Order", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        [Route("api/orders", Name = "PostNewOrder")]
        public async Task<IHttpActionResult> PostOrder([FromBody]TicketOrder order)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "PostOrder", GetServiceProperties());

            try
            {
                TicketOrder.Validate(order);

                ServiceLocationService locator = new ServiceLocationService();
                UriBuilderService builder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, Constants.ContosoEventsTicketOrderServiceName);
                ITicketOrderService dispenderService = locator.Create<ITicketOrderService>(builder.ToUri());
                return Ok(await dispenderService.EnqueueOrder(order));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [SwaggerOperation("CancelOrder")]
        [SwaggerResponse(HttpStatusCode.OK, "Cancel an order by id", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        [Route("api/orders/cancel/{id}", Name = "CancelOrder")]
        public async Task<IHttpActionResult> CancelOrder(string id)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "CancelOrder", GetServiceProperties());

            try
            {
                IActorLocationService locator = ServiceFactory.GetInstance().GetActorLocationService();
                ITicketOrderActor orderActor = locator.Create<ITicketOrderActor>(new ActorId(id), Constants.ContosoEventsApplicationName);
                await orderActor.CancelOrder();
                return Ok("Ok");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }
    }
}
