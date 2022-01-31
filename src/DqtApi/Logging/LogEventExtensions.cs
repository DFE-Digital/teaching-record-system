using Serilog.Events;

namespace DqtApi.Logging
{
    public static class LogEventExtensions
    {
        public static T GetScalarPropertyValue<T>(this LogEvent logEvent, string propertyKey)
        {
            var property = logEvent.Properties[propertyKey];
            return (T)(((ScalarValue)property).Value);
        }
    }
}
