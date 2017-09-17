using ContosoEvents.Web.Helpers;
using ContosoEvents.Web.Models.Api;
using ContosoEvents.Web.Policies;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;

namespace ContosoEvents.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var events = ContosoEventsApi.GetEvents();
            Event ticketEvent = null;
            if (events.Any())
            {
                var randomEventNumber = new Random().Next(events.Count);
                ticketEvent = events[randomEventNumber];
            }

            return View(ticketEvent);
        }

        // Secure this action with a sign-in policy
        // You can use the PolicyAuthorize decorator to execute a certain policy if the user is not already signed in the app.
        [PolicyAuthorize(Policy = "B2C_1_Signin")]
        public ActionResult Claims()
        {
            // Extract user information from the claims in the ClaimsPrincipal.
            var displayName = ClaimsPrincipal.Current.FindFirst(ClaimsPrincipal.Current.Identities.First().NameClaimType);
            ViewBag.DisplayName = displayName != null ? displayName.Value : string.Empty;
            return View();
        }

        public ActionResult Error(string message)
        {
            ViewBag.Message = message;

            return View("Error");
        }
    }
}