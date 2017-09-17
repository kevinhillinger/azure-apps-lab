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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoEvents.WebApi.Controllers
{
    public class EventsController : GenericApiController
    {
        protected const string LOG_TAG = "EventsController";
        private static FabricClient _fabricClient = new FabricClient();

        public EventsController() :
            base()
        {
        }

        [SwaggerOperation("GetEvents")]
        [SwaggerResponse(HttpStatusCode.OK, "Ticket Events", typeof(List<TicketEvent>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/events", Name = "AllEvents")]
        public async Task<IHttpActionResult> GetEvents()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetEvents", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                var itemsList = await dataService.GetEvents();
                return Ok(itemsList.OrderBy(p => p.EndDate).ToList());
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

        [SwaggerOperation("GetEventById")]
        [SwaggerResponse(HttpStatusCode.OK, "Ticket Event By Id", typeof(TicketEvent))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/events/{id}", Name = "GetEventById")]
        public async Task<IHttpActionResult> GetEventById(string id)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetEventById", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                return Ok(await dataService.GetEventById(id));
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

        [SwaggerOperation("GetEventStatsById")]
        [SwaggerResponse(HttpStatusCode.OK, "Ticket Event Stats By Id", typeof(TicketEventStats))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/events/stats/{id}", Name = "GetEventStatsById")]
        public async Task<IHttpActionResult> GetEventStatsById(string id)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetEventStatsById", GetServiceProperties());

            try
            {
                IActorLocationService locator = ServiceFactory.GetInstance().GetActorLocationService();
                IEventActor eventActor = locator.Create<IEventActor>(new ActorId(id), Constants.ContosoEventsApplicationName);
                return Ok(await eventActor.GetStats());
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

        [SwaggerOperation("PostNewEvent")]
        [SwaggerResponse(HttpStatusCode.OK, "Create a new Ticket Event", typeof(TicketEvent))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        [Route("api/events", Name = "PostNewEvent")]
        public async Task<IHttpActionResult> PostEvent([FromBody]TicketEvent tEvent)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "PostEvent", GetServiceProperties());

            try
            {
                TicketEvent.Validate(tEvent);

                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                var id = await dataService.CreateEvent(tEvent);
                return Ok(await dataService.GetEventById(id));
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

        [SwaggerOperation("ReloadEvent")]
        [SwaggerResponse(HttpStatusCode.OK, "Reload a Ticket Event", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        [Route("api/events/{eventid}", Name = "ReloadEvent")]
        public async Task<IHttpActionResult> ReloadEvent(string eventid)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "ReloadEvent", GetServiceProperties());

            try
            {
                IActorLocationService locator = ServiceFactory.GetInstance().GetActorLocationService();
                IEventActor eventActor = locator.Create<IEventActor>(new ActorId(eventid), Constants.ContosoEventsApplicationName);
                await eventActor.Reload();
                return Ok("Done");
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
