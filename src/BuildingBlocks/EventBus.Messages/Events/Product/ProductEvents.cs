using EventBus.Messages.Common;

namespace EventBus.Messages.Events.Product;

public class ProductCreatedEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public long CategoryId { get; set; }
    public long? SupplierId { get; set; }
}

public class ProductUpdatedEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public long CategoryId { get; set; }
    public long? SupplierId { get; set; }
}

public class ProductDeletedEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
}

public class ProductStockUpdatedEvent : IntegrationBaseEvent
{
    public long ProductId { get; set; }
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
}
