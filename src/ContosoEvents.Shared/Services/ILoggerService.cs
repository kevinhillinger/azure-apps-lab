using ContosoEvents.Models;
using ContosoEvents.Shared.Services.LogSources;
using System.Collections.Generic;

namespace ContosoEvents.Shared.Services
{
    public interface ILoggerService : ILogMessageEventSource
    {
        void Log(LogLevels level, string correlationId, string tag, string method, string message, LogTypes type, Dictionary<string, string> properties = null, double duration = 0);
    }
}
