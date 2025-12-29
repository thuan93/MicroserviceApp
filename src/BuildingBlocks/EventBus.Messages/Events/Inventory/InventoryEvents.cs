using EventBus.Messages.Common;

namespace EventBus.Messages.Events.Inventory;

public class InventoryReservedEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public long OrderId { get; set; }
}

public class InventoryReleasedEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public long OrderId { get; set; }
}

public class InventoryLowStockEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
}
