using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace ContosoEvents.Web.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public void SignIn()
        {
            // Execute a sign in policy
            if (!Request.IsAuthenticated)
            {
                // To execute a policy, you simply need to trigger an OWIN challenge.
                // You can indicate which policy to use by adding it to the AuthenticationProperties by using the PolicyKey provided.
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties(new Dictionary<string, string> { { Startup.PolicyKey, Startup.SignInPolicyId } })
                    {
                        RedirectUri = "/",
                    },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        [AllowAnonymous]
        public void SignUp()
        {
            // Execute a sign up policy
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties(new Dictionary<string, string> { { Startup.PolicyKey, Startup.SignUpPolicyId } })
                    {
                        RedirectUri = "/",
                    },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public new void Profile()
        {
            // Execute an edit profile policy
            if (Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties(new Dictionary<string, string> { { Startup.PolicyKey, Startup.ProfilePolicyId } })
                    {
                        RedirectUri = "/",
                    },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            // Sign the user out using OWIN.
            if (Request.IsAuthenticated)
            {
                // To sign out the user, you should issue an OpenID Connect sign-out request by using the last policy that the user executed.
                // This is as easy as looking up the current value of the ACR claim, adding it to the AuthenticationProperties, and making an OWIN SignOut call.

                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties(new Dictionary<string, string> { { Startup.PolicyKey, ClaimsPrincipal.Current.FindFirst(Startup.AcrClaimType).Value } }),
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType);
            }
        }
    }
}