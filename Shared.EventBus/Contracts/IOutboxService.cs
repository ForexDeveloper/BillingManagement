namespace Shared.EventBus.Contracts
{
    public interface IOutboxService
    {
        public void AddNewEvent<T>(T message) where T : IEventSign;

    }
}
