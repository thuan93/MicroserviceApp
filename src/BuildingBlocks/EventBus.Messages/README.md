# EventBus.Messages BuildingBlock

## M?c ?ích
Ch?a các event contracts (message contracts) cho RabbitMQ event-driven communication gi?a các microservices.

## C?u trúc

```
EventBus.Messages/
??? Common/
?   ??? IntegrationBaseEvent      # Base class cho t?t c? events
??? Events/
    ??? Product/
    ?   ??? ProductEvents         # Product-related events
    ??? Order/
    ?   ??? OrderEvents           # Order-related events
    ??? Inventory/
    ?   ??? InventoryEvents       # Inventory-related events
    ??? Customer/
        ??? CustomerEvents        # Customer-related events
```

## Event Flow Example

```
Product.Api              RabbitMQ              Inventory.Api
    |                       |                       |
    | -- ProductCreated --> |                       |
    |                       | -- Subscribe ----->   |
    |                       |                       |
    |                       |   <-- Process Event   |
    |                       |                       |
    | <-- InventoryReserved |   <-- Publish Event   |
```

## Cách s? d?ng

### 1. Publish Event (Product.Api)

```csharp
using EventBus.Messages.Events.Product;

public class ProductService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task CreateProduct(CreateProductDto dto)
    {
        // Save product to database
        var product = await _repository.AddAsync(newProduct);
        
        // Publish event
        await _publishEndpoint.Publish(new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId
        });
    }
}
```

### 2. Consume Event (Inventory.Api)

```csharp
using EventBus.Messages.Events.Product;
using MassTransit;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation($"Product Created: {message.ProductId} - {message.Name}");
        
        // Update inventory
        await UpdateInventoryAsync(message.ProductId, message.StockQuantity);
    }
}
```

### 3. MassTransit Configuration

```csharp
// Program.cs or ServiceExtensions
services.AddMassTransit(config =>
{
    // Register consumers
    config.AddConsumer<ProductCreatedConsumer>();
    
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});
```

## Available Events

### Product Events
- ? `ProductCreatedEvent` - Khi t?o product m?i
- ? `ProductUpdatedEvent` - Khi update product
- ? `ProductDeletedEvent` - Khi xóa product
- ? `ProductStockUpdatedEvent` - Khi stock thay ??i

### Order Events
- ? `OrderCreatedEvent` - Khi t?o order
- ? `OrderUpdatedEvent` - Khi update order status
- ? `OrderCancelledEvent` - Khi cancel order

### Inventory Events
- ? `InventoryReservedEvent` - Khi reserve stock cho order
- ? `InventoryReleasedEvent` - Khi release stock (cancel order)
- ? `InventoryLowStockEvent` - Khi stock th?p h?n minimum

### Customer Events
- ? `CustomerCreatedEvent` - Khi t?o customer
- ? `CustomerUpdatedEvent` - Khi update customer
- ? `CustomerDeletedEvent` - Khi xóa customer

## Event Properties

M?i event ??u k? th?a `IntegrationBaseEvent` và có:
- `Id` (Guid) - Unique event identifier
- `CreationDate` (DateTime) - UTC timestamp

## Best Practices

? Events are immutable (use records)  
? Include all necessary data (avoid additional lookups)  
? Use past tense naming (ProductCreated, not CreateProduct)  
? Keep events small and focused  
? Version events when schema changes  

## Dependencies

Không có external dependencies - Pure .NET 10
