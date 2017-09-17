using ContosoEvents.Web.Helpers;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            var claim = ClaimsPrincipal.Current.FindFirst(ContosoEventsApi.UserIdentifierClaimType);

            return claim != null ? claim.Value : null;
        }
    }
}