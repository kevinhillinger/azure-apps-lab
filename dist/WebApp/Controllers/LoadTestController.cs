using ContosoEvents.Web.Helpers;
using ContosoEvents.Web.Models.Api;
using System.Security.Claims;
using System.Web.Mvc;

namespace ContosoEvents.Web.Controllers
{
    public class LoadTestController : Controller
    {
        public ActionResult Index()
        {
            var events = ContosoEventsApi.GetEvents();

            return View(events);
        }

        public ActionResult Status()
        {
            var status = ContosoEventsApi.GetLoadSimulationStatus();

            return View(status);
        }

        [HttpPost]
        public ActionResult RunTest(string eventId, string tag, int requestsCount)
        {
            var userName = ClaimsPrincipal.Current.GetUserId();
            var email = ClaimsPrincipal.Current.Identity.Name;

            var simulation = new LoadSimulationRequest
            {
                BaseUrl = ContosoEventsApi.BaseUrl,
                EventId = eventId,
                UserName = string.IsNullOrWhiteSpace(userName) == false ? userName : "no_username",
                Email = string.IsNullOrWhiteSpace(email) == false ? email : "no_email",
                Iterations = requestsCount,
                Tag = tag
            };

            ContosoEventsApi.RequestLoadSimulation(simulation);

            return RedirectToAction("Index");
        }
    }
}