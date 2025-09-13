namespace Shared.Logging.Abstraction.Models;

public enum LogMessageTag
{
    Input = 1,
    ExternalService = 2,
    Db = 3,
    Internal = 4,
    EventBus = 5
}