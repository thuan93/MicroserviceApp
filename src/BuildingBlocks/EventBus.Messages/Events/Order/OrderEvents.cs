using EventBus.Messages.Common;

namespace EventBus.Messages.Events.Order;

public class OrderCreatedEvent : IntegrationBaseEvent
{
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderUpdatedEvent : IntegrationBaseEvent
{
    public long OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OrderCancelledEvent : IntegrationBaseEvent
{
    public long OrderId { get; set; }
}

public class OrderItemDto
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
