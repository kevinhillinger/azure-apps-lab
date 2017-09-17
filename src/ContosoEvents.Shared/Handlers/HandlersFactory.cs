using ContosoEvents.Shared.Services;
using Shared.Services;

namespace ContosoEvents.Shared.Handlers
{
    public class HandlersFactory
    {
        public static IProfilerHandler GetProfilerHandler(ISettingService setting, ILoggerService logger)
        {
            return new ProfilerHandler(setting, logger);
        }
    }
}
