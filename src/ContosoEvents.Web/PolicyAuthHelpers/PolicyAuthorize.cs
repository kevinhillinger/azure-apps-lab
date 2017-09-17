using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace ContosoEvents.Web.Policies
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PolicyAuthorize : AuthorizeAttribute
    {
        public string Policy { get; set; }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.HttpContext.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties(new Dictionary<string, string> { { Startup.PolicyKey, Policy } })
                {
                    RedirectUri = "/",
                },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}