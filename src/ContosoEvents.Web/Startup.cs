using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ContosoEvents.Web.Startup))]
namespace ContosoEvents.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //TODO Exercise 10 - Task 3 - uncomment the following line to initialize the authentication middleware
            //ConfigureAuth(app);
        }
    }
}