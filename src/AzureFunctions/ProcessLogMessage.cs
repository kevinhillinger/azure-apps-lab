using System;

public static void Run(object logMessageItem, out object logMessageDocument, TraceWriter log)
{
    log.Info($"logMessage received: {logMessageItem}");
    log.Info(GetEnvironmentVariable("sfshackathon_DOCUMENTDB"));

    // Store in DocDB so we can query it for log messages
    logMessageDocument = logMessageItem;
}

public static string GetEnvironmentVariable(string name)
{
    return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}

