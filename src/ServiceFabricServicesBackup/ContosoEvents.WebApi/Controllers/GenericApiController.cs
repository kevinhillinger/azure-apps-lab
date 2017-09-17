using ContosoEvents.Shared.Helpers;
using ContosoEvents.Shared.Services;
using Shared.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace ContosoEvents.WebApi.Controllers
{
    public class GenericApiController : ApiController
    {
        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public GenericApiController()
        {
            // This is really neat! The FabricRuntime.GetActivationContext() gives me access to the config context!!! :-)
            _settingService = ServiceFactory.GetInstance().GetSettingService();
            _loggerService = ServiceFactory.GetInstance().GetLoggerService();
        }

        public ISettingService TheSettingService
        {
            get { return _settingService; }
            set { _settingService = value; }
        }

        public ILoggerService TheLoggerService
        {
            get { return _loggerService; }
            set { _loggerService = value; }
        }

        // If there is a way to access the SF context from the controller, we will change this
        protected Dictionary<string, string> GetServiceProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, "WebApi.WebApi");
            props.Add(Constants.ServicePropId, "API");
            props.Add(Constants.ServicePropApplicationType, "ContosoEventsAppType");
            props.Add(Constants.ServicePropApplicationName, "ContosoEventsApp");
            props.Add(Constants.ServicePropServiceType, "WebApiType");
            props.Add(Constants.ServicePropServiceName, "WebApi");
            props.Add(Constants.ServicePropPartitionId, "Web API");
            props.Add(Constants.ServicePropReplicationId, "Web API");
            props.Add(Constants.ServicePropNode, "Web API");
            return props;
        }
    }
}
