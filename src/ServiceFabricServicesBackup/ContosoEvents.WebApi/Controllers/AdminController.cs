using ContosoEvents.Models;
using ContosoEvents.Shared.Contracts;
using ContosoEvents.Shared.Handlers;
using ContosoEvents.Shared.Helpers;
using ContosoEvents.Shared.Services;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Query;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ContosoEvents.WebApi.Controllers
{
    public class AdminController : GenericApiController
    {
        protected const string LOG_TAG = "AdminController";
        private static FabricClient _fabricClient = new FabricClient();

        public AdminController() :
            base()
        {
        }

        [SwaggerOperation("GetTicketOrderPartitions")]
        [SwaggerResponse(HttpStatusCode.OK, "Ticket Order Partitions", typeof(List<TicketOrderServiceInfo>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/admin/partitions", Name = "AllTicketOrderPartitions")]
        public async Task<IHttpActionResult> GetTicketOrderPartitions()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetTicketOrderPartitions", GetServiceProperties());
            List<TicketOrderServiceInfo> infos = new List<TicketOrderServiceInfo>();

            try
            {
                ServiceLocationService locator = new ServiceLocationService();
                UriBuilderService builder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, Constants.ContosoEventsTicketOrderServiceName);
                ServicePartitionList partitions = await _fabricClient.QueryManager.GetPartitionListAsync(builder.ToUri());
                foreach (Partition p in partitions)
                {
                    long minKey = (p.PartitionInformation as Int64RangePartitionInformation).LowKey;
                    ITicketOrderService dispenderService = locator.Create<ITicketOrderService>(minKey, builder.ToUri());
                    infos.Add(new TicketOrderServiceInfo()
                    {
                        PartitionId = p.PartitionInformation.Id.ToString(),
                        PartitionKind = p.PartitionInformation.Kind.ToString(),
                        PartitionStatus = p.PartitionStatus.ToString(),
                        NodeName = await dispenderService.GetNodeName(),
                        HealthState = p.HealthState.ToString(),
                        ServiceKind = p.ServiceKind.ToString(),
                        ItemsInQueue = await dispenderService.GetOrdersCounter(CancellationToken.None)
                    });
                }

                return Ok(infos);
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

        [SwaggerOperation("GetApplicationHealth")]
        [SwaggerResponse(HttpStatusCode.OK, "Get Application Health", typeof(ApplicationHealth))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/admin/applicationhealth", Name = "GetApplicationHealth")]
        public async Task<IHttpActionResult> GetApplicationHealth()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetApplicationHealth", GetServiceProperties());

            try
            {
                return Ok(await _fabricClient.HealthManager.GetApplicationHealthAsync(new Uri(Constants.ContosoEventsApplicationName)));
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

        [SwaggerOperation("GetServiceHealth")]
        [SwaggerResponse(HttpStatusCode.OK, "Get Service Health", typeof(ApplicationHealth))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/admin/servicehealth/{servicename}", Name = "GetServiceHealth")]
        public async Task<IHttpActionResult> GetServiceHealth(string servicename)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetServiceHealth", GetServiceProperties());

            try
            {
                ServiceLocationService locator = new ServiceLocationService();
                UriBuilderService builder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, servicename);
                return Ok(await _fabricClient.HealthManager.GetServiceHealthAsync(builder.ToUri()));
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

        /// <summary>
        /// The Actor Service Name is the name of the actor plus the Service appended. 
        /// Examples: TicketOrderActorService and EventActorService
        /// </summary>
        /// <param name="actorname"></param>
        /// <returns></returns>
        [SwaggerOperation("GetActorHealth")]
        [SwaggerResponse(HttpStatusCode.OK, "Get Actor Health", typeof(ApplicationHealth))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/admin/actorhealth/{actorname}", Name = "GetActorHealth")]
        public async Task<IHttpActionResult> GetActorHealth(string actorname)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "GetActorHealth", GetServiceProperties());

            try
            {
                ServiceLocationService locator = new ServiceLocationService();
                UriBuilderService builder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, actorname);
                return Ok(await _fabricClient.HealthManager.GetServiceHealthAsync(builder.ToUri()));
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

        [SwaggerOperation("PostTicketOrdersSimulation")]
        [SwaggerResponse(HttpStatusCode.OK, "Simulate Ticket Orders", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        [Route("api/admin/simulate/orders", Name = "PostTicketOrderSimulation")]
        public async Task<IHttpActionResult> PostTicketOrdersSimulation([FromBody]TicketOrderSimulationRequest request)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "PostOrder", GetServiceProperties());

            try
            {
                TicketOrderSimulationRequest.Validate(request);

                IExternalizationService extService = ServiceFactory.GetInstance().GetExternalizationService(TheSettingService, TheLoggerService);
                await extService.Simulate(request);
                return Ok("Running");
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

        [SwaggerOperation("UpdateServiceHealthState")]
        [SwaggerResponse(HttpStatusCode.OK, "Update Service Health State", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        [Route("api/admin/health/service", Name = "UpdateServiceHealthState")]
        public async Task<IHttpActionResult> UpdateServiceHealthState(UpdateHealthRequest request)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "UpdateServiceHealthState", GetServiceProperties());

            try
            {
                UpdateHealthRequest.Validate(request);

                int ticketOrderServices = 0;
                int ticketOrderActors = 0;
                int eventActors = 0;
                ServiceLocationService locator = new ServiceLocationService();
                UriBuilderService builder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, Constants.ContosoEventsTicketOrderServiceName);
                ServicePartitionList partitions = await _fabricClient.QueryManager.GetPartitionListAsync(builder.ToUri());
                foreach (Partition p in partitions)
                {
                    long minKey = (p.PartitionInformation as Int64RangePartitionInformation).LowKey;
                    ITicketOrderService dispenderService = locator.Create<ITicketOrderService>(builder.ToUri());
                    await dispenderService.UpdateHealthState(GetHealthStateFromString(request.State), request.Message);
                    ticketOrderServices++;
                }

                ActorLocationService actorLocator = new ActorLocationService();
                UriBuilderService orderActorBuilder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, Constants.ContosoEventsTicketOrderActorName);
                ServicePartitionList orderActorPartitions = await _fabricClient.QueryManager.GetPartitionListAsync(orderActorBuilder.ToUri());
                foreach (Partition p in orderActorPartitions)
                {
                    string minKey = (p.PartitionInformation as Int64RangePartitionInformation).Id.ToString();
                    ITicketOrderActor actor = actorLocator.Create<ITicketOrderActor>(new ActorId(minKey), Constants.ContosoEventsApplicationName);
                    await actor.UpdateHealthState(GetHealthStateFromString(request.State), request.Message);
                    ticketOrderActors++;
                    // May require contiunuation
                }

                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                List<TicketEvent> events = await dataService.GetEvents();
                UriBuilderService eventActorBuilder = new UriBuilderService(Constants.ContosoEventsApplicationInstance, Constants.ContosoEventsEventActorName);
                foreach (var tEvent in events)
                {
                    IEventActor actor = actorLocator.Create<IEventActor>(new ActorId(tEvent.Id), Constants.ContosoEventsApplicationName);
                    await actor.UpdateHealthState(GetHealthStateFromString(request.State), request.Message);
                    eventActors++;
                    // May require contiunuation
                }

                return Ok("Done: " + ticketOrderServices + "|" + ticketOrderActors + "|" + eventActors);
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

        [SwaggerOperation("DeleteAllEvents")]
        [SwaggerResponse(HttpStatusCode.OK, "Delete all events", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        [Route("api/admin/events", Name = "DeleteAllEvents")]
        public async Task<IHttpActionResult> DeleteAllEvents()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "DeleteAllEvents", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                await dataService.DeleteAllEvents();
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

        [SwaggerOperation("DeleteAllOrders")]
        [SwaggerResponse(HttpStatusCode.OK, "Delete all orders", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        [Route("api/admin/orders", Name = "DeleteAllOrders")]
        public async Task<IHttpActionResult> DeleteAllOrders()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "DeleteAllOrders", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                await dataService.DeleteAllOrders();
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

        [SwaggerOperation("DeleteAllLogMessges")]
        [SwaggerResponse(HttpStatusCode.OK, "Delete all log messages", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        [Route("api/admin/logmessages", Name = "DeleteAllLogMessges")]
        public async Task<IHttpActionResult> DeleteAllLogMessges()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService, TheLoggerService);
            handler.Start(LOG_TAG, "DeleteAllLogMessges", GetServiceProperties());

            try
            {
                IDataStoreService dataService = ServiceFactory.GetInstance().GetDataStoreService(TheSettingService, TheLoggerService);
                await dataService.DeleteAllLogMessages();
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

        // private
        private HealthState GetHealthStateFromString(string state)
        {
            if (state.ToLower() == "ok")
                return HealthState.Ok;
            else if (state.ToLower() == "warning")
                return HealthState.Warning;
            else if (state.ToLower() == "error")
                return HealthState.Error;
            else if (state.ToLower() == "invalid")
                return HealthState.Invalid;
            else
                return HealthState.Warning;
        }
    }
}
