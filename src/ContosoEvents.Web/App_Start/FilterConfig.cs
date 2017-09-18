using System.Web;
using System.Web.Mvc;

namespace ContosoEvents.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            // enforce authorization on all routes
            filters.Add(new Policies.PolicyAuthorize { Policy = Startup.SignInPolicyId });
        }
    }
}
