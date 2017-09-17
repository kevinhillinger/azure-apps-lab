using ContosoEvents.Models;
using System;

namespace ContosoEvents.Shared.Services.LogSources
{
    public interface ILogMessageEventSource
    {
        event EventHandler<LogMessage> LogMessageReceived;
    }
}
