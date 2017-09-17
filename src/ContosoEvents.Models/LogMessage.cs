using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ContosoEvents.Models
{
    public class LogMessage
    {
        public LogMessage(LogLevels level, string correlationId, string tag, string method, string message, LogTypes type, Dictionary<string, string> properties = null, double duration = 0)
        {
            Id = Guid.NewGuid().ToString();
            Level = level;
            CorrelationId = correlationId;
            Tag = tag;
            Method = method;
            Message = message;
            Type = type;
            Properties = properties;
            Duration = duration;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public LogLevels Level { get; set; }
        public string CorrelationId { get; set; }
        public string Tag { get; set; }
        public string Method { get; set; }
        public string Message { get; set; }
        public LogTypes Type { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public double Duration { get; set; }
    }

    public enum LogLevels
    {
        Debug,
        Info,
        Medium,
        Severe
    }

    public enum LogTypes
    {
        Start,
        Info,
        Error,
        Stop,
        AbnormalStop
    }
}
