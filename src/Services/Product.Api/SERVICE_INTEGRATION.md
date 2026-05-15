# Kết Nối Giữa Product.Api Và Các Services Khác

## Tổng Quan

**Product.Api** giao tiếp với các microservices khác thông qua ba cơ chế chính: **API Gateway (Ocelot)**, **Event Bus (MassTransit/RabbitMQ)** và **Health Checks / OpenTelemetry**.

---

## 1. API Gateway (Ocelot)

Ocelot hoạt động như cổng duy nhất cho tất cả request từ client đến Product.Api.

### Cấu Hình Routing

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5001 }
      ],
      "UpstreamPathTemplate": "/products/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ]
}
```

### Authentication & Authorization
- Ocelot xác thực JWT token trước khi forward request.
- Claims từ token được truyền xuống Product.Api để kiểm tra quyền.
- Hỗ trợ rate limiting và load balancing.

---

## 2. Event Bus (MassTransit / RabbitMQ)

Product.Api publish các domain events qua RabbitMQ để các service khác xử lý bất đồng bộ.

### Các Event Được Publish

| Event | Khi Nào Publish | Consumer |
|-------|----------------|----------|
| **ProductCreated** | Tạo sản phẩm mới thành công | Notification.Api, Search.Api |
| **ProductUpdated** | Cập nhật thông tin sản phẩm | Search.Api, Cart.Api |
| **ProductDeleted** | Xóa sản phẩm | Cart.Api, Order.Api |
| **ProductStockUpdated** | Thay đổi số lượng tồn kho | Order.Api, Inventory.Api |

### Cấu Hình MassTransit

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});
```

### Flow Xử Lý Event

1. Controller gọi Service → Service ghi DB → Service publish event qua MassTransit.
2. RabbitMQ nhận event → Message broker gửi đến các consumer.
3. Consumer xử lý logic tương ứng (gửi email, cập nhật search index, v.v.).

---

## 3. Health Checks

Product.Api expose endpoint `/health` để kiểm tra trạng thái hoạt động.

### Các Thành Phần Được Kiểm Tra

- **Database**: Kiểm tra kết nối SQL Server.
- **RabbitMQ**: Kiểm tra kết nối message broker.
- **Redis**: Kiểm tra cache (nếu có).
- **External APIs**: Kiểm tra các dependency bên ngoài.

### Cấu Hình

```csharp
services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>()
    .AddRabbitMQ(rabbitConnectionString)
    .AddRedis(redisConnectionString);
```

---

## 4. OpenTelemetry Tracing

OpenTelemetry thu thập và xuất trace, metrics và logs.

### Cấu Hình

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracer => tracer
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddJaegerExporter())
    .WithMetrics(meter => meter
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());
```

### Dữ Liệu Được Thu Thập

- **Trace**: Distributed tracing qua Jaeger/Zipkin.
- **Metrics**: Request rate, error rate, latency (qua Prometheus).
- **Logs**: Structured logging với Serilog, gửi đến Elasticsearch.

---

## Sơ Đồ Luồng Dữ Liệu

```
[Client] → [Ocelot Gateway] → [Product.Api]
                                  ├── [SQL Server Database]
                                  ├── [RabbitMQ / MassTransit]
                                  │     └── [Notification.Api, Search.Api, ...]
                                  └── [OpenTelemetry / Jaeger / Prometheus]
```
