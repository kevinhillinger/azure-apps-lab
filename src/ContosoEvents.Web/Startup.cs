using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ContosoEvents.Web.Startup))]
namespace ContosoEvents.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //initialize the authentication middleware
            ConfigureAuth(app);
        }
    }
}