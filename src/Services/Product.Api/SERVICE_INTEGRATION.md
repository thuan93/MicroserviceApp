# K?t n?i gi?a Product.Api và các Services khác

## ?? S? ?? k?t n?i t?ng quan

```
                    ????????????????
                    ?  API Gateway ?
                    ?   (Ocelot)   ?
                    ????????????????
                           ?
        ???????????????????????????????????????
        ?                  ?                  ?
        ?                  ?                  ?
    ??????????       ????????????      ????????????
    ?Customer?       ? Product  ?      ?  Order   ?
    ?  Api   ?????????   Api    ????????   Api    ?
    ??????????       ????????????      ????????????
                           ?
        ???????????????????????????????????????
        ?                  ?                  ?
        ?                  ?                  ?
    ??????????       ????????????      ????????????
    ?Inventory?      ?  Basket  ?      ?Schedule  ?
    ?  Api   ?       ?   Api    ?      ?   Job    ?
    ??????????       ????????????      ????????????
         ?                  ?                 ?
         ??????????????????????????????????????
                            ?
                    ????????????????
                    ?   RabbitMQ   ?
                    ?  Event Bus   ?
                    ????????????????
```

## 1?? Product.Api ?? Order.Api

### K?ch b?n: Khách hàng ??t hàng

**Flow:**
```
1. Order.Api nh?n request t?o order
2. Order.Api call Product.Api ?? check product exists & stock
3. Product.Api tr? v? product info
4. Order.Api t?o order
5. Order.Api publish OrderCreatedEvent
6. Product.Api consume event và update stock
```

**Product.Api cung c?p:**
```csharp
// API Endpoints for Order.Api
GET /api/products/{id}              // Get product details
GET /api/products/check-stock/{id}  // Check stock availability
POST /api/products/reserve-stock    // Reserve stock for order
```

**Events:**
```csharp
// Product.Api publishes
ProductStockUpdatedEvent
{
    ProductId,
    OldStock,
    NewStock,
    UpdatedBy: "OrderService"
}

// Product.Api consumes
OrderCreatedEvent -> Decrease stock
OrderCancelledEvent -> Restore stock
```

---

## 2?? Product.Api ?? Inventory.Api

### K?ch b?n: Qu?n lý t?n kho

**Flow:**
```
1. Product.Api update product stock
2. Publish ProductStockChangedEvent
3. Inventory.Api consume event
4. Inventory.Api sync stock data
5. Inventory.Api có th? publish LowStockAlert
6. Product.Api consume và mark product as low stock
```

**Events:**
```csharp
// Product.Api publishes
ProductCreatedEvent
ProductStockChangedEvent
ProductDeletedEvent

// Product.Api consumes
InventoryUpdatedEvent -> Sync stock from inventory system
LowStockAlertEvent -> Update product status
```

**Shared Data Models:**
```csharp
// In EventBus.Messages project
public class ProductStockSyncEvent
{
    public long ProductId { get; set; }
    public int AvailableStock { get; set; }
    public int ReservedStock { get; set; }
    public DateTime SyncedAt { get; set; }
}
```

---

## 3?? Product.Api ?? Basket.Api

### K?ch b?n: Thêm s?n ph?m vào gi? hàng

**Flow:**
```
1. User add product to basket via Basket.Api
2. Basket.Api call Product.Api to get product info
3. Product.Api return product details & price
4. Basket.Api calculate total and store in Redis
```

**Product.Api cung c?p:**
```csharp
GET /api/products/{id}                    // Get product info
POST /api/products/batch                  // Get multiple products
GET /api/products/{id}/current-price      // Get current price
```

**Redis Cache Structure:**
```json
{
  "basket:user123": {
    "items": [
      {
        "productId": 1,
        "productName": "Laptop",
        "price": 999.99,
        "quantity": 1,
        "cachedAt": "2024-01-15T10:30:00Z"
      }
    ]
  }
}
```

**Events:**
```csharp
// Product.Api publishes
ProductPriceChangedEvent
{
    ProductId,
    OldPrice,
    NewPrice,
    ChangedAt
}

// Basket.Api consumes và update price in all baskets
```

---

## 4?? Product.Api ?? Customer.Api

### K?ch b?n: Customer preferences & recommendations

**Flow:**
```
1. Customer.Api track customer behavior
2. Customer view product -> publish ProductViewedEvent
3. Product.Api consume ?? update view count
4. Product.Api có th? recommend similar products
```

**Events:**
```csharp
// Customer.Api publishes
CustomerViewedProductEvent
{
    CustomerId,
    ProductId,
    ViewedAt,
    Duration
}

// Product.Api consumes
- Update product view count
- Generate recommendations
```

---

## 5?? Product.Api ?? ScheduleJob

### K?ch b?n: Background tasks & maintenance

**Schedule Job tasks:**
```csharp
// Daily tasks
- Clear expired product cache
- Generate daily product reports
- Sync product data with external systems
- Check and alert for low stock products

// Hourly tasks
- Refresh product cache
- Update product rankings

// Weekly tasks
- Clean up deleted products
- Archive old product data
```

**Events:**
```csharp
// ScheduleJob publishes
CacheClearRequestedEvent
DataSyncRequestedEvent

// Product.Api consumes and executes
```

---

## ?? Connection Strings & Endpoints

### Internal Service URLs (Docker Network)

```yaml
# docker-compose.override.yml
services:
  product.api:
    environment:
      - ServiceUrls__OrderApi=http://ordering.api:80
      - ServiceUrls__InventoryApi=http://inventory.api:80
      - ServiceUrls__BasketApi=http://basket.api:80
      - ServiceUrls__CustomerApi=http://customer.api:80
```

### appsettings.json Configuration

```json
{
  "ServiceUrls": {
    "OrderApi": "http://localhost:5002",
    "InventoryApi": "http://localhost:5005",
    "BasketApi": "http://localhost:5003",
    "CustomerApi": "http://localhost:5004"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "ProductApi_"
  }
}
```

---

## ?? Event Bus Messages

### EventBus.Messages Project Structure

```
EventBus.Messages/
??? Events/
?   ??? Product/
?   ?   ??? ProductCreatedEvent.cs
?   ?   ??? ProductUpdatedEvent.cs
?   ?   ??? ProductDeletedEvent.cs
?   ?   ??? ProductStockChangedEvent.cs
?   ?   ??? ProductPriceChangedEvent.cs
?   ??? Order/
?   ?   ??? OrderCreatedEvent.cs
?   ?   ??? OrderCancelledEvent.cs
?   ?   ??? OrderCompletedEvent.cs
?   ??? Inventory/
?       ??? InventoryUpdatedEvent.cs
?       ??? LowStockAlertEvent.cs
```

### Example Event Implementation

```csharp
// EventBus.Messages/Events/Product/ProductCreatedEvent.cs
namespace EventBus.Messages.Events.Product;

public record ProductCreatedEvent
{
    public long ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public long CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = "System";
}
```

---

## ?? Service-to-Service Authentication

### Option 1: API Key (Simple)

```csharp
// Product.Api -> Order.Api
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "shared-secret-key");
var response = await client.GetAsync("http://order.api/api/orders");
```

### Option 2: JWT Token (Recommended)

```csharp
// Get token from Identity Server
var tokenResponse = await identityClient.RequestClientCredentialsTokenAsync(
    new ClientCredentialsTokenRequest
    {
        Address = "http://identity.server/connect/token",
        ClientId = "product.api",
        ClientSecret = "secret",
        Scope = "order.api"
    });

// Use token to call Order.Api
client.SetBearerToken(tokenResponse.AccessToken);
var response = await client.GetAsync("http://order.api/api/orders");
```

---

## ?? Testing Service Integration

### Integration Test Example

```csharp
public class ProductOrderIntegrationTests
{
    [Fact]
    public async Task CreateOrder_WithValidProduct_ShouldDecreaseStock()
    {
        // Arrange
        var productId = await CreateTestProduct(stockQuantity: 10);
        
        // Act
        var orderResult = await OrderApiClient.CreateOrder(new CreateOrderDto
        {
            Items = new[] { new OrderItemDto { ProductId = productId, Quantity = 2 } }
        });
        
        // Assert
        var product = await ProductApiClient.GetProduct(productId);
        Assert.Equal(8, product.StockQuantity); // 10 - 2 = 8
    }
}
```

---

## ?? Monitoring Cross-Service Communication

### Health Check Dependencies

```csharp
// Product.Api/Extensions/ServiceExtensions.cs
services.AddHealthChecks()
    .AddMySql(connectionString)
    .AddRabbitMQ(rabbitConnectionString)
    .AddRedis(redisConnectionString)
    .AddUrlGroup(new Uri("http://inventory.api/health"), "inventory-api")
    .AddUrlGroup(new Uri("http://order.api/health"), "order-api");
```

### Distributed Tracing

```csharp
// Add OpenTelemetry for distributed tracing
services.AddOpenTelemetryTracing(builder =>
{
    builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource("Product.Api")
        .AddJaegerExporter();
});
```

---

## ?? Summary

**Product.Api ho?t ??ng nh? m?t hub service:**

1. **Provides product data** cho t?t c? services khác
2. **Consumes events** t? Order, Inventory, Customer services
3. **Publishes events** khi có thay ??i product data
4. **Caches frequently accessed data** trong Redis
5. **Communicates via RabbitMQ** cho async operations
6. **Exposes REST APIs** cho sync operations qua API Gateway

**Key Dependencies:**
- ? MySQL (data storage)
- ? RabbitMQ (event bus)
- ? Redis (caching)
- ? API Gateway (routing)
- ? EventBus.Messages (shared contracts)
