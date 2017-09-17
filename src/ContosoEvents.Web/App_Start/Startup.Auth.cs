using ContosoEvents.Web.Policies;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoEvents.Web
{
    public partial class Startup
    {
        // The ACR claim is used to indicate which policy was executed
        public const string AcrClaimType = "http://schemas.microsoft.com/claims/authnclassreference";
        public const string PolicyKey = "b2cpolicy";
        public const string OIDCMetadataSuffix = "/.well-known/openid-configuration";

        // App config settings
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        // B2C policy identifiers
        public static string SignUpPolicyId = ConfigurationManager.AppSettings["ida:SignUpPolicyId"];
        public static string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
        public static string ProfilePolicyId = ConfigurationManager.AppSettings["ida:UserProfilePolicyId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the OWIN OpenID Connect authentication pipeline
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            var options = new OpenIdConnectAuthenticationOptions
            {
                // These are standard OpenID Connect parameters, with values pulled from web.config
                ClientId = clientId,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                    RedirectToIdentityProvider = OnRedirectToIdentityProvider,
                },
                Scope = "openid",
                ResponseType = "id_token",

                // The PolicyConfigurationManager takes care of getting the correct Azure AD authentication
                // endpoints from the OpenID Connect metadata endpoint. It is included in the PolicyAuthHelpers folder.
                // The first parameter is the metadata URL of your B2C directory.
                // The second parameter is an array of the policies that your app will use.
                ConfigurationManager = new PolicyConfigurationManager(
                    string.Format(CultureInfo.InvariantCulture, aadInstance, tenant, "/v2.0", OIDCMetadataSuffix),
                    new string[] { SignUpPolicyId, SignInPolicyId, ProfilePolicyId }),

                // This piece is optional. It is used to display the user's name in the navigation bar.
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "emails",
                },
            };

            app.UseOpenIdConnectAuthentication(options);
        }

        // This notification can be used to manipulate the OIDC request before it is sent.  Here we use it to send the correct policy.
        private async Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            // Use the AuthenticationProperties to choose the right policy to execute
            var policyManager = notification.Options.ConfigurationManager as PolicyConfigurationManager;
            if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            {
                var config = await policyManager.GetConfigurationByPolicyAsync(CancellationToken.None, notification.OwinContext.Authentication.AuthenticationResponseRevoke.Properties.Dictionary[Startup.PolicyKey]);
                notification.ProtocolMessage.IssuerAddress = config.EndSessionEndpoint;
            }
            else
            {
                var config = await policyManager.GetConfigurationByPolicyAsync(CancellationToken.None, notification.OwinContext.Authentication.AuthenticationResponseChallenge.Properties.Dictionary[Startup.PolicyKey]);
                notification.ProtocolMessage.IssuerAddress = config.AuthorizationEndpoint;
            }
        }

        // Used for avoiding yellow-screen-of-death
        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}