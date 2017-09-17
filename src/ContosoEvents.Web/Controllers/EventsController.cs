using ContosoEvents.Web.Helpers;
using ContosoEvents.Web.Models;
using System.Security.Claims;
using System.Web.Mvc;

namespace ContosoEvents.Web.Controllers
{
    public class EventsController : Controller
    {
        public ActionResult Index()
        {
            var events = ContosoEventsApi.GetEvents();

            return View(events);
        }

        public ActionResult Details(string id)
        {
            var data = ContosoEventsApi.GetEvent(id);

            return View(data);
        }

        public ActionResult Order(string id)
        {
            var principal = ClaimsPrincipal.Current;
            var eventData = ContosoEventsApi.GetEvent(id);

            var data = new NewOrder
            {
                Event = eventData,
                TicketCount = 1,
                FirstName = principal?.FindFirst(ClaimTypes.GivenName)?.Value,
                LastName = principal?.FindFirst(ClaimTypes.Surname)?.Value,
                Address = "1 Microsoft Way",
                City = "Redmond",
                PostalCode = "WA 98052",
                Country = "US",

                DeliveryEmail = principal?.FindFirst("emails")?.Value,
                PhoneNumber = "425 123 4567",
            };

            return View(data);
        }
    }
}