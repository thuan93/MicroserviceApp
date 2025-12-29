using EventBus.Messages.Common;

namespace EventBus.Messages.Events.Customer;

public class CustomerCreatedEvent : IntegrationBaseEvent
{
    public long CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CustomerUpdatedEvent : IntegrationBaseEvent
{
    public long CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CustomerDeletedEvent : IntegrationBaseEvent
{
    public long CustomerId { get; set; }
}
