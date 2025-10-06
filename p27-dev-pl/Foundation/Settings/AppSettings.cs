namespace Foundation.Settings

{
    public class AppSettings
    {
        public string MongoUrl { get; set; }
        public DistributedTracingOptions DistributedTracing { get; set; }
        public Jwt Jwt { get; set; }
        public MessageBroker MessageBroker { get; set; }
    }

    public class DistributedTracingOptions
    {
        public bool IsEnabled { get; set; }
        public JaegerOptions Jaeger { get; set; }
    }

    public class JaegerOptions
    {
        public string ServiceName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public record Jwt(string Issuer, string Audience, string Key);

    public record MessageBroker(string Host, string User, string Password);
}
