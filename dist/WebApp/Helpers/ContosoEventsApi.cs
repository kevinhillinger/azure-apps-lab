using ContosoEvents.Web.Models.Api;
using RestSharp;
using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace ContosoEvents.Web.Helpers
{
    public static class ContosoEventsApi
    {
        public const string UserIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        // Api Management settings
        public static string BaseUrl = ConfigurationManager.AppSettings["apimng:BaseUrl"];
        private static string key = ConfigurationManager.AppSettings["apimng:SubscriptionKey"];

        private static IRestResponse Execute(IRestRequest request)
        {
            var client = new RestClient(BaseUrl);
            client.AddDefaultHeader("Ocp-Apim-Subscription-Key", key);

            return client.Execute(request);
        }

        private static T Execute<T>(IRestRequest request) where T : new()
        {
            var client = new RestClient(BaseUrl);
            client.AddDefaultHeader("Ocp-Apim-Subscription-Key", key);

            return client.Execute<T>(request).Data;
        }

        public static IList<Event> GetEvents()
        {
            var request = new RestRequest("api/events", Method.GET);

            return Execute<List<Event>>(request) ?? new List<Event>();
        }

        public static Event GetEvent(string id)
        {
            var request = new RestRequest("api/events/" + id, Method.GET);

            return Execute<Event>(request);
        }

        public static Order GetOrder(string id)
        {
            var request = new RestRequest("api/orders/" + id, Method.GET);

            return Execute<Order>(request);
        }

        public static IList<Order> GetUserOrders(string username)
        {
            var request = new RestRequest("api/orders/user/" + username, Method.GET);

            return Execute<List<Order>>(request);
        }

        public static string PlaceUserOrder(OrderRequest order)
        {
            var request = new RestRequest("api/orders", Method.POST);
            request.AddJsonBody(order);

            var result = Execute(request);
            return result.StatusCode == HttpStatusCode.OK && string.IsNullOrWhiteSpace(result.Content) == false
                ? result.Content
                : null;
        }

        public static bool CancelUserOrder(string id)
        {
            var request = new RestRequest("api/orders/cancel/" + id, Method.PUT);

            return Execute(request).StatusCode == HttpStatusCode.OK;
        }

        public static bool RequestLoadSimulation(LoadSimulationRequest simulation)
        {
            var request = new RestRequest("api/admin/simulate/orders", Method.POST);
            request.AddJsonBody(simulation);

            return Execute(request).StatusCode == HttpStatusCode.OK;
        }

        public static IList<LoadSimulationStatus> GetLoadSimulationStatus()
        {
            var request = new RestRequest("api/admin/partitions", Method.GET);

            return Execute<List<LoadSimulationStatus>>(request);
        }
    }
}