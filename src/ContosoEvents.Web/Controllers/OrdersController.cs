using ContosoEvents.Web.Helpers;
using ContosoEvents.Web.Models;
using ContosoEvents.Web.Models.Api;
using System;
using System.Security.Claims;
using System.Web.Mvc;

namespace ContosoEvents.Web.Controllers
{
    public class OrdersController : Controller
    {
        public ActionResult Index()
        {
            var orders = ContosoEventsApi.GetUserOrders(ClaimsPrincipal.Current.GetUserId());

            return View(orders);
        }

        [HttpPost]
        public ActionResult PlaceOrder(string id, NewOrder order)
        {
            var result = true;
            string orderId = null;

            var eventData = ContosoEventsApi.GetEvent(id);
            if (eventData == null)
            {
                result = false;
            }
            else
            {
                var data = new OrderRequest
                {
                    OrderDate = DateTime.UtcNow,
                    UserName = ClaimsPrincipal.Current?.GetUserId(),
                    Email = ClaimsPrincipal.Current?.Identity?.Name,
                    EventId = eventData.Id,
                    PaymentProcessorTokenId = order.PaymentProcessorToken,
                    Tickets = order.TicketCount
                };
                if (string.IsNullOrWhiteSpace(data.Email)) data.Email = order.DeliveryEmail;
                if (string.IsNullOrWhiteSpace(data.UserName)) data.UserName = order.DeliveryEmail;
                if (string.IsNullOrWhiteSpace(data.FirstName)) data.FirstName = order.FirstName;
                if (string.IsNullOrWhiteSpace(data.LastName)) data.LastName = order.LastName;

                orderId = ContosoEventsApi.PlaceUserOrder(data);
                result = orderId != null;
            }

            return View("OrderResult", new Tuple<bool, string>(result, orderId));
        }

        public ActionResult Details(string id)
        {
            var data = ContosoEventsApi.GetOrder(id);

            return View(data);
        }

        public ActionResult Cancel(string id)
        {
            ContosoEventsApi.CancelUserOrder(id);

            return RedirectToAction("Index");
        }
    }
}