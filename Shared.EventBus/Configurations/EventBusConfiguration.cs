namespace Shared.EventBus.Configurations
{
    public class EventBusConfiguration
    {
        public string EventsPrefix { get; set; } = "BaseTemplate"; //?
        public string HostUrl { get; set; }
        public int DefaultEventFetchInterval { get; set; }

    }
}
