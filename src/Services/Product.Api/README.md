# Product.Api — Kiến trúc Microservice

## 1. Cấu trúc thư mục

```
Product.Api/
├── Controllers/
│   ├── ProductsController.cs      # API endpoints cho Product
│   └── HealthController.cs         # Health check endpoints
├── DTOs/
│   └── ProductDto.cs               # CreateProductDto, UpdateProductDto, ProductDto
├── Entities/
│   ├── Product.cs                  # Domain model chính
│   ├── Category.cs                 # Danh mục sản phẩm
│   └── Supplier.cs                 # Nhà cung cấp
├── Extensions/
│   └── ServiceExtensions.cs        # DI Registration, MassTransit, Health Checks
├── Persistence/
│   ├── ProductContext.cs           # DbContext (EF Core)
│   └── Configurations/
│       ├── ProductConfiguration.cs # Fluent API config cho Product
│       ├── CategoryConfiguration.cs
│       └── SupplierConfiguration.cs
├── Repositories/
│   ├── Interfaces/
│   │   └── IProductRepository.cs   # Interface repository
│   └── ProductRepository.cs        # Implementation repository
├── Validators/
│   └── CreateProductDtoValidator.cs # FluentValidation rules
├── Migrations/                     # EF Core migrations
├── Program.cs                      # Entry point
├── appsettings.json                # Cấu hình chính
├── appsettings.Development.json    # Cấu hình development
├── Dockerfile                      # Docker build
└── Product.Api.csproj              # Project file
```

## 2. Kết nối với các thành phần khác

### 2.1 BuildingBlocks (Shared Libraries)

Product.Api tham chiếu 6 thư viện dùng chung từ thư mục `BuildingBlocks/`:

| Thư viện | Mục đích |
|----------|----------|
| `AspNetCore.Extensions` | CORS, JWT, Swagger, Exception Handler |
| `Common.Logging` | Cấu hình Serilog toàn cục |
| `Shared` | `ApiResponse<T>`, `PaginatedResult<T>`, utilities |
| `Infrastructure` | `RepositoryBase<T, TContext>`, `IRepository<T>` |
| `Contracts` | `IEntityBase`, `IAuditableEntity` |
| `EventBus.Messages` | Message contracts cho RabbitMQ (`ProductCreatedEvent`, ...) |

### 2.2 Database — MySQL (Port 3307)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3307;Database=ProductDb;Uid=root;Pwd=Passw0rd!;AllowPublicKeyRetrieval=true;"
  }
}
```

Kết nối bằng DBeaver / MySQL Workbench:
- Host: `localhost` — Port: `3307`
- Database: `ProductDb` — User: `root` — Password: `Passw0rd!`

Entity Relationship:

```
┌───────────┐       ┌──────────────┐       ┌────────────┐
│ Category  │──1:N──│   Product    │──N:1──│  Supplier  │
│ Id        │       │ Id           │       │ Id         │
│ Name      │       │ Name         │       │ Name       │
└───────────┘       │ Price        │       └────────────┘
                    │ StockQuantity│
                    │ CategoryId   │
                    │ SupplierId   │
                    │ CreatedDate  │
                    │ UpdatedDate  │
                    └──────────────┘
```

### 2.3 API Gateway (Ocelot)

Product.Api được expose qua Ocelot API Gateway:

```
http://localhost:<gateway-port>/products → http://localhost:<product-port>/api/products
```

Gateway chịu trách nhiệm: routing, authentication, rate limiting, load balancing.

### 2.4 RabbitMQ (Event Bus)

- Host: `localhost:5672` — Management UI: `http://localhost:15672`
- Username/Password: `guest` / `guest`

Product.Api publish các **domain events** khi dữ liệu thay đổi:

| Sự kiện | Khi nào | Consumer tiềm năng |
|----------|---------|-------------------|
| `ProductCreatedEvent` | `POST /api/products` | Search, Notification |
| `ProductUpdatedEvent` | `PUT /api/products/{id}` | Search, Inventory |
| `ProductStockUpdatedEvent` | Khi StockQuantity thay đổi | Inventory, Ordering |
| `ProductDeletedEvent` | `DELETE /api/products/{id}` | Search, Cart |

```csharp
// Ví dụ: Publish event qua MassTransit
await _publishEndpoint.Publish(new ProductCreatedEvent
{
    ProductId = product.Id,
    Name = product.Name,
    Price = product.Price
});
```

### 2.5 Health Checks

```
GET /health      → Chi tiết từng dependency (DB, RabbitMQ, ...)
GET /api/health  → JSON report với từng entry
GET /api/health/ping → Kiểm tra alive đơn giản
```

Các health check đã đăng ký:
- **mysql**: Kiểm tra kết nối MySQL

Tích hợp với **WebHealthStatus** dashboard để monitor tổng thể.

### 2.6 Logging (Serilog)

- Ghi log có cấu trúc (structured logging) ra Console
- Hỗ trợ Elasticsearch (có thể bật qua cấu hình)
- Mỗi request được ghi log tự động qua `UseSerilogRequestLogging()`
- Log level cấu hình trong `appsettings.json`

### 2.7 OpenTelemetry (Distributed Tracing)

```json
{
  "OpenTelemetry": {
    "Enabled": true,
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

Tự động collect traces và metrics, gửi đến OTLP collector (Jaeger, Grafana Tempo, ...).

## 3. Cách chạy Project

### 3.1 Khởi động Database

```bash
# Từ thư mục gốc của solution
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d productdb rabbitmq
```

### 3.2 Tạo / Cập nhật Migration

```bash
cd src/Services/Product.Api

# Tạo migration mới
dotnet ef migrations add TenMigration

# Áp dụng migration vào database
dotnet ef database update

# Xem các migration đã có
dotnet ef migrations list
```

### 3.3 Chạy Application

```bash
# Development
cd src/Services/Product.Api
dotnet run --environment Development

# Hoặc F5 trong Visual Studio / Rider
```

### 3.4 Kiểm tra API

**Swagger UI:** `http://localhost:<port>/swagger`

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/api/products?pageIndex=1&pageSize=20` | Danh sách phân trang |
| `GET` | `/api/products/{id}` | Chi tiết sản phẩm |
| `POST` | `/api/products` | Tạo sản phẩm mới |
| `PUT` | `/api/products/{id}` | Cập nhật sản phẩm |
| `DELETE` | `/api/products/{id}` | Xoá sản phẩm |
| `GET` | `/api/products/category/{categoryId}` | Sản phẩm theo danh mục |
| `GET` | `/health` | Health checks |
| `GET` | `/api/health/ping` | Ping alive |

### 3.5 Chạy với Docker

```bash
docker build -t product-api .
docker run -p 5000:8080 product-api
```

## 4. Packages được sử dụng (.NET 10)

| Package | Version | Mục đích |
|---------|---------|----------|
| `Microsoft.EntityFrameworkCore` | 9.0.0 | ORM |
| `Pomelo.EntityFrameworkCore.MySql` | 9.0.0 | MySQL provider |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 12.0.1 | Mapping Entity ↔ DTO |
| `FluentValidation.AspNetCore` | 11.3.0 | Input validation |
| `MediatR` | 12.4.1 | CQRS / In-process messaging |
| `MassTransit` | 8.3.5 | Message bus abstraction |
| `MassTransit.RabbitMQ` | 8.3.5 | RabbitMQ transport |
| `Swashbuckle.AspNetCore` | 7.2.0 | Swagger / OpenAPI |
| `AspNetCore.HealthChecks.MySql` | 8.0.1 | MySQL health check |
| `AspNetCore.HealthChecks.UI.Client` | 8.0.1 | Health check UI writer |
| `Serilog` | *(qua Common.Logging)* | Structured logging |

## 5. Kiến trúc Pattern

### 5.1 Clean Architecture (Onion)

```
┌──────────────────────────────────────┐
│        Controllers (API Layer)        │  → Xử lý HTTP request/response
├──────────────────────────────────────┤
│       Repositories (Application)      │  → Business logic, orchestration
├──────────────────────────────────────┤
│  Infrastructure / Persistence (Data)  │  → EF Core, DbContext
├──────────────────────────────────────┤
│      Entities / Domain (Core)         │  → Domain models thuần
└──────────────────────────────────────┘
```

- **Outer layer** (Controllers) phụ thuộc vào **inner layer** (Repositories), không phụ thuộc vào Infrastructure
- **Domain layer** không biết gì về database, HTTP, hay bất kỳ framework nào

### 5.2 Repository Pattern

```
Controller → IProductRepository (interface)
                  ↓
          ProductRepository (implementation)
                  ↓
         RepositoryBase<T> (generic, Infrastructure)
                  ↓
            ProductContext (EF Core)
                  ↓
                MySQL
```

- Interface được định nghĩa trong `Interfaces/`, implementation trong `Repositories/`
- Base repository (`RepositoryBase`) cung cấp sẵn `AddAsync`, `UpdateAsync`, `DeleteAsync`, `FindAsync`, `GetByIdAsync`
- Repository chịu trách nhiệm mapping Entity → DTO trước khi trả về Controller

### 5.3 Dependency Injection

Tất cả dependencies được đăng ký tập trung trong `ServiceExtensions.cs`:

```csharp
services.AddScoped<IProductRepository, ProductRepository>();
services.AddDbContext<ProductContext>(...);
services.AddMassTransit(config => { config.UsingRabbitMq(...); });
services.AddHealthChecks().AddMySql(...);
services.AddValidatorsFromAssembly(...);
```

Controller nhận dependencies qua constructor injection:

```csharp
public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
```

## 6. Workflow (Luồng xử lý request)

```
Client (HTTP)
    │
    ▼
┌──────────────────┐
│  API Gateway      │  → Authentication, Routing
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  ProductsController │  → Validate input (FluentValidation)
│  POST /api/products │  → Call IProductRepository
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ ProductRepository  │  → Map CreateProductDto → Entity
│                    │  → Save to DB (EF Core)
│                    │  → Publish ProductCreatedEvent (RabbitMQ)
│                    │  → Map Entity → ProductDto
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Response (JSON)   │  → ApiResponse<ProductDto>
└──────────────────┘
```

Ví dụ cụ thể với tạo sản phẩm:

1. **Client** gửi `POST /api/products` với JSON body
2. **FluentValidation** kiểm tra dữ liệu đầu vào (Name không rỗng, Price > 0, ...)
3. **ProductsController.Create()** gọi `_productRepository.CreateProductAsync(dto)`
4. **ProductRepository** map DTO → Entity, gọi `AddAsync(product)` → EF Core → MySQL
5. **ProductRepository** publish `ProductCreatedEvent` qua MassTransit → RabbitMQ
6. **ProductRepository** query lại và trả về `ProductDto`
7. **Controller** wrap trong `ApiResponse` và trả về `201 Created`

## 7. Ghi chú

### Tại sao dùng Repository Pattern?

- **Tách biệt data access logic**: Controller không cần biết là MySQL, SQL Server, hay In-Memory
- **Dễ test**: Có thể mock `IProductRepository` mà không cần database thật
- **Reusable queries**: Các truy vấn phức tạp (phân trang, include, filter) được đóng gói trong repository
- **Dễ thay đổi ORM**: Nếu chuyển từ EF Core sang Dapper, chỉ cần thay đổi implementation

### Tại sao dùng DTO thay vì trả Entity trực tiếp?

- **Không expose internal structure**: Entity có thể chứa navigation properties, shadow properties
- **Kiểm soát dữ liệu trả về**: Chỉ trả đúng field cần thiết, tránh over-fetching
- **Circular reference**: Tránh lỗi JSON serialization do quan hệ vòng giữa các Entity
- **API Versioning**: Dễ thay đổi response format mà không ảnh hưởng Domain Model
- **Security**: Không vô tình expose sensitive data (ví dụ: internal notes, audit fields)

### Tại sao publish event trong Repository?

- Repository là nơi transaction được quản lý, đảm bảo **tính nhất quán** giữa database và message
- Có thể wrap trong **Outbox Pattern** sau này để đảm bảo exactly-once delivery
- Tránh trường hợp lưu DB thành công nhưng quên publish event

## 8. Next Steps

1. [x] FluentValidation cho input validation (đã implement `CreateProductDtoValidator`)
2. [ ] Triển khai Outbox Pattern cho event publishing (Transactional Outbox)
3. [ ] Viết Unit Tests (xUnit + Moq) và Integration Tests (Testcontainers)
4. [ ] Thêm Redis caching cho GET endpoints
5. [ ] Implement CQRS đầy đủ với MediatR (Commands / Queries riêng)
6. [ ] Rate limiting qua API Gateway hoặc middleware
7. [ ] CI/CD pipeline (GitHub Actions / Azure DevOps)
8. [ ] Monitoring & Alerting (Grafana dashboards)
