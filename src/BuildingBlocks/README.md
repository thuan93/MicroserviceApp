# BuildingBlocks — Thư viện dùng chung cho toàn bộ Microservices

## 1. Tổng quan về BuildingBlocks

**BuildingBlocks** là tập hợp các thư viện dùng chung (shared libraries) được tái sử dụng bởi tất cả microservices trong hệ thống. Mục tiêu:

- **Tái sử dụng code** — tuân thủ nguyên tắc DRY, tránh trùng lặp logic.
- **Nhất quán** — đảm bảo các service có chung cách triển khai repository, logging, response format.
- **Tăng tốc phát triển** — không cần viết lại các pattern phổ biến (Repository, Unit of Work, Exception Middleware).
- **Dễ bảo trì** — thay đổi một nơi, áp dụng cho tất cả service.

## 2. Danh sách các BuildingBlocks

### 2.1. Infrastructure (`src/BuildingBlocks/Infrastructure`)

**Mục đích:** Cung cấp tầng truy xuất dữ liệu (Data Access Layer) với các design pattern phổ biến.

**Nội dung:**
- `IRepository<TEntity>` — interface generic định nghĩa các thao tác CRUD.
- `RepositoryBase<TEntity, TContext>` — lớp base implement generic repository pattern, sử dụng EF Core.
- `UnitOfWork` — quản lý transaction cho nhiều repository.

**Dependencies:** `Microsoft.EntityFrameworkCore`

**Service sử dụng:** Product.Api, Customer.Api, Ordering.Api, Inventory.Api.

---

### 2.2. Shared (`src/BuildingBlocks/Shared`)

**Mục đích:** Chứa các tiện ích, helper, DTO dùng chung không phụ thuộc vào tầng nào.

**Nội dung:**
- `ApiResponse<T>` / `ApiResponse` — response wrapper chuẩn hóa.
- `PaginatedResult<T>` — hỗ trợ phân trang.
- `ApiConstants` — hằng số API (route, message).
- `CustomExceptions` — các exception tùy chỉnh (`NotFoundException`, `ValidationException`, `BadRequestException`, `ForbiddenException`).
- Extension methods: `StringExtensions`, `DateTimeExtensions`.

**Service sử dụng:** Tất cả service.

---

### 2.3. Contracts (`src/BuildingBlocks/Contracts`)

**Mục đích:** Định nghĩa DTO và interface chung giữa các service, đảm bảo kiểu dữ liệu nhất quán khi giao tiếp.

**Nội dung:**
- `ProductDto`, `CreateProductDto`, `UpdateProductDto`
- `CategoryDto`, `SupplierDto`
- Base entity interfaces (nếu có)

**Service sử dụng:** Các service cần trao đổi dữ liệu với nhau.

---

### 2.4. EventBus.Messages (`src/BuildingBlocks/EventBus.Messages`)

**Mục đích:** Định nghĩa message contracts cho giao tiếp bất đồng bộ qua Event Bus (MassTransit / RabbitMQ).

**Nội dung:**
- `IntegrationBaseEvent` — lớp cơ sở chứa `Id` (Guid) và `CreationDate`.
- `ProductCreatedEvent`, `ProductUpdatedEvent`, `ProductDeletedEvent`, `ProductStockUpdatedEvent`
- Các event tương tự cho Customer, Inventory, Order.

**Service sử dụng:** Service có dùng MassTransit/RabbitMQ.

---

### 2.5. Common.Logging (`src/BuildingBlocks/Common.Logging`)

**Mục đích:** Cấu hình logging tập trung dùng Serilog.

**Nội dung:**
- `Serilogger.ConfigureLogger` — Action delegate cấu sẵn Serilog với Console, Debug output.
- Enrichers: MachineName, Environment, Application.

**Service sử dụng:** Tất cả service.

---

### 2.6. AspNetCore.Extensions (`src/BuildingBlocks/AspNetCore.Extensions`)

**Mục đích:** Các extension dùng chung cho tầng ASP.NET Core.

**Nội dung:**
- `ExceptionMiddleware` + `UseGlobalExceptionHandler()` — global exception handler trả về `ApiResponse` chuẩn.
- `AddMicroserviceTelemetry()` — cấu hình OpenTelemetry tracing + OTLP export.
- `CorsExtensions`, `JwtAuthenticationExtensions`, `SwaggerGenJwtExtensions`, `HttpResilienceExtensions`.

**Dependencies:** OpenTelemetry, Swashbuckle, JwtBearer.

**Service sử dụng:** Tất cả service có API endpoint.

## 3. Kiến trúc phụ thuộc (ASCII Diagram)

```
┌──────────────────────────────────────────────────────────────┐
│                      Microservices                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  Product.Api  │  │ Customer.Api │  │  Ordering.Api    │   │
│  │               │  │              │  │                  │   │
│  └───────┬───────┘  └──────┬───────┘  └────────┬─────────┘   │
│  ┌───────┴───────┐  ┌──────┴───────┐  ┌────────┴─────────┐   │
│  │ Inventory.Api  │  │  Basket.Api  │  │  OcelotApiGw     │   │
│  └───────┬───────┘  └──────┬───────┘  └────────┬─────────┘   │
└──────────┼──────────────────┼───────────────────┼─────────────┘
           │                  │                   │
           ▼                  ▼                   ▼
┌──────────────────────────────────────────────────────────────┐
│                    BuildingBlocks Layer                        │
│  ┌────────────────┐  ┌────────────────┐  ┌──────────────┐    │
│  │  Infrastructure│  │    Shared      │  │  Contracts   │    │
│  │  (Repository)  │  │  (ApiResponse) │  │   (DTOs)     │    │
│  └────────────────┘  └────────────────┘  └──────────────┘    │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐   │
│  │ EventBus.Msgs  │  │ Common.Logging │  │AspNetCore.Ext  │   │
│  │  (Events)      │  │  (Serilog)     │  │(Middleware)    │   │
│  └────────────────┘  └────────────────┘  └────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

**Mối quan hệ giữa các BuildingBlocks:**

- `Infrastructure` → độc lập, chỉ dùng EF Core.
- `Shared` → độc lập, không phụ thuộc BuildingBlocks khác.
- `Contracts` → độc lập.
- `EventBus.Messages` → độc lập, dùng `IntegrationBaseEvent` nội bộ.
- `Common.Logging` → độc lập, chỉ dùng Serilog.
- `AspNetCore.Extensions` → phụ thuộc `Shared` (dùng `ApiResponse` và `CustomExceptions`).

## 4. Cách sử dụng

### 4.1. Thêm Project Reference

Trong `.csproj` của service, thêm các `ProjectReference` cần thiết:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\BuildingBlocks\Infrastructure\Infrastructure.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\Shared\Shared.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\Contracts\Contracts.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\EventBus.Messages\EventBus.Messages.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\Common.Logging\Common.Logging.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\AspNetCore.Extensions\AspNetCore.Extensions.csproj" />
</ItemGroup>
```

### 4.2. Sử dụng Infrastructure (Repository + UnitOfWork)

```csharp
using Infrastructure.Repositories;

// Định nghĩa repository riêng
public interface IProductRepository : IRepository<Product> { }

public class ProductRepository : RepositoryBase<Product, ProductDbContext>, IProductRepository
{
    public ProductRepository(ProductDbContext context) : base(context) { }

    // Có thể override hoặc thêm method riêng
    public async Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId)
        => await FindAsync(p => p.CategoryId == categoryId);
}

// Sử dụng trong Service Layer
public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Product> CreateAsync(Product product)
    {
        await _repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }
}
```

### 4.3. Sử dụng Shared (ApiResponse, CustomException)

```csharp
using Shared.DTOs;
using Shared.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(long id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException($"Product with id {id} not found.");

        return Ok(ApiResponse<ProductDto>.SuccessResult(product));
    }
}
```

### 4.4. Sử dụng Contracts (DTO dùng chung)

```csharp
using Contracts.DTOs.Product;

public async Task<ProductDto> GetProductAsync(long id)
{
    var product = await _repository.GetByIdAsync(id);
    return new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price
    };
}
```

### 4.5. Sử dụng EventBus.Messages (Publish Event)

```csharp
using EventBus.Messages.Events.Product;
using MassTransit;

public class ProductService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var product = await _repository.AddAsync(MapToEntity(dto));

        await _publishEndpoint.Publish(new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId
        });

        return product;
    }
}
```

### 4.6. Sử dụng Common.Logging (Serilog)

```csharp
using Common.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Serilogger.ConfigureLogger);
```

### 4.7. Sử dụng AspNetCore.Extensions (Middleware + Telemetry)

```csharp
using AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddMicroserviceTelemetry("Product.Api");

var app = builder.Build();
app.UseGlobalExceptionHandler();
```

## 5. Best Practices

### ✅ NÊN làm:
- Sử dụng BuildingBlocks cho mọi chức năng dùng chung.
- Giữ code generic, tái sử dụng, không hardcode.
- Ghi lại thay đổi trong README tương ứng.
- Luôn kiểm tra backward compatibility trước khi sửa.

### ❌ KHÔNG nên làm:
- Đặt business logic vào BuildingBlocks.
- Viết code đặc thù cho một service duy nhất.
- Tạo tight coupling giữa các BuildingBlocks với nhau.
- Sao chép code đã có sẵn trong BuildingBlocks.

## 6. Dependency Matrix

| Service            | Infrastructure | Shared | Contracts | EventBus | Logging | AspNetCore.Ext |
|--------------------|:--------------:|:------:|:---------:|:--------:|:-------:|:--------------:|
| Product.Api        | ✅             | ✅     | ✅        | ✅       | ✅      | ✅             |
| Customer.Api       | ✅             | ✅     | ✅        | ✅       | ✅      | ✅             |
| Ordering.Api       | ✅             | ✅     | ✅        | ✅       | ✅      | ✅             |
| Inventory.Api      | ✅             | ✅     | ✅        | ✅       | ✅      | ✅             |
| Basket.Api         | ❌             | ✅     | ✅        | ✅       | ✅      | ✅             |
| OcelotApiGw        | ❌             | ✅     | ❌        | ❌       | ✅      | ✅             |

## 7. Versioning

**Phiên bản hiện tại:** `1.0.0`

Mỗi BuildingBlock có thể được version riêng trong `.csproj`:

```xml
<PropertyGroup>
  <Version>1.1.0</Version>
  <AssemblyVersion>1.1.0.0</AssemblyVersion>
  <FileVersion>1.1.0.0</FileVersion>
</PropertyGroup>
```

Quy tắc versioning:
- **Major** — thay đổi phá vỡ backward compatibility.
- **Minor** — thêm tính năng mới, vẫn tương thích.
- **Patch** — sửa lỗi, không thay đổi contract.

## 8. Contributing Guidelines

Khi thêm hoặc sửa BuildingBlocks:

1. **Cập nhật README** — ghi rõ mục đích, lớp, namespace của thay đổi.
2. **Kiểm tra backward compatibility** — không phá vỡ contract hiện tại nếu chưa có major version bump.
3. **Viết unit test** — cho mọi logic phức tạp (exception handling, extension methods).
4. **Build toàn bộ solution** — đảm bảo không lỗi ở service nào:
   ```powershell
   dotnet build src/MicroserviceApp.sln
   ```
5. **Tạo Pull Request** — mô tả rõ thay đổi, lý do, ảnh hưởng.

## 9. Roadmap

- [ ] Thêm FluentValidation base validators cho common validation rules
- [ ] Middleware xử lý exception hoàn chỉnh (dùng `AspNetCore.Extensions`)
- [ ] Hỗ trợ API versioning qua `AspNetCore.Extensions`
- [ ] Distributed caching abstraction (Redis interface)
- [ ] Authentication/Authorization helpers (Policy-based)
- [ ] Health check extensions dùng chung
- [ ] Tự động generate OpenAPI specs từ Contracts
- [ ] Source Generators cho Repository boilerplate
