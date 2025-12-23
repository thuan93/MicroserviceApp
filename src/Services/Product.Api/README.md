# Product.Api - Microservice Architecture

## ?? C?u trúc Folder

```
Product.Api/
??? Controllers/             # API Endpoints
?   ??? ProductsController.cs
??? DTOs/                    # Data Transfer Objects
?   ??? ProductDto.cs
??? Entities/                # Domain Models
?   ??? Product.cs
?   ??? Category.cs
??? Extensions/              # Service Extensions
?   ??? ServiceExtensions.cs
??? Persistence/             # Database Context & Configurations
?   ??? ProductContext.cs
?   ??? Configurations/
?       ??? ProductConfiguration.cs
?       ??? CategoryConfiguration.cs
??? Repositories/            # Data Access Layer
?   ??? Interfaces/
?   ?   ??? IProductRepository.cs
?   ??? ProductRepository.cs
??? appsettings.json
??? appsettings.Development.json
??? Program.cs
```

## ?? K?t n?i v?i các thành ph?n

### 1. **BuildingBlocks**
Product.Api s? d?ng các shared libraries:

- **Common.Logging**: Serilog configuration
- **Shared**: Common utilities & helpers
- **Infrastructure**: Repository patterns & base classes
- **Contracts**: DTOs & interfaces
- **EventBus.Messages**: RabbitMQ message contracts

### 2. **Database - MySQL (Port 3307)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3307;Database=ProductDb;Uid=root;Pwd=Passw0rd!;"
  }
}
```

**DBeaver Connection:**
- Host: `localhost`
- Port: `3307`
- Database: `ProductDb`
- Username: `root`
- Password: `Passw0rd!`

### 3. **API Gateway (Ocelot)**
Product.Api s? ???c expose thông qua OcelotApiGw:
```
http://localhost:<gateway-port>/products
```

### 4. **RabbitMQ (Event Bus)**
- Host: `localhost:5672`
- Management UI: `http://localhost:15672`
- Publish events khi Product thay ??i (Create/Update/Delete)

### 5. **Health Checks**
```
GET http://localhost:<port>/health
```
K?t n?i v?i WebHealthStatus dashboard

### 6. **Logging**
- Console output
- Debug window (Visual Studio)
- Elasticsearch (future integration)

## ?? Ch?y Project

### 1. **Kh?i ??ng Database**
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d productdb
```

### 2. **T?o Database Migration**
```bash
cd src/Services/Product.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. **Ch?y Application**
```bash
dotnet run
```

ho?c F5 trong Visual Studio

### 4. **Test API**

**Swagger UI:**
```
http://localhost:<port>/swagger
```

**Endpoints:**
- GET    `/api/products` - Get all products
- GET    `/api/products/{id}` - Get product by id
- POST   `/api/products` - Create product
- PUT    `/api/products/{id}` - Update product
- DELETE `/api/products/{id}` - Delete product

## ?? Packages ???c s? d?ng

### Database
- `Microsoft.EntityFrameworkCore` (9.0.0)
- `Pomelo.EntityFrameworkCore.MySql` (9.0.0)

### Utilities
- `AutoMapper` - Object mapping
- `FluentValidation` - Input validation
- `MediatR` - CQRS pattern

### Messaging
- `MassTransit` - Message bus abstraction
- `MassTransit.RabbitMQ` - RabbitMQ integration

### Monitoring
- `Serilog` - Structured logging
- `AspNetCore.HealthChecks.MySql` - Health checks

### Documentation
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI

## ??? Ki?n trúc Pattern

### Clean Architecture (Onion Architecture)
```
???????????????????????????????????
?      Controllers (API)          ?
???????????????????????????????????
?      Services (Business)        ?
???????????????????????????????????
?     Repositories (Data)         ?
???????????????????????????????????
?    Entities (Domain Models)     ?
???????????????????????????????????
```

### Repository Pattern
- Tách bi?t logic truy v?n database
- D? test và mock
- Có th? thay ??i database provider

### Dependency Injection
- T?t c? dependencies ???c inject qua constructor
- Register trong `ServiceExtensions.cs`

## ?? Workflow

1. **Request** ? Controller
2. **Controller** ? Repository (ho?c Service n?u có business logic)
3. **Repository** ? Database (via EF Core)
4. **Response** ? DTO (không expose Entity)

## ?? Ghi chú

### T?i sao dùng Repository Pattern?
- ? Tách bi?t data access logic
- ? D? test v?i mock repositories
- ? Có th? thay ??i database mà không ?nh h??ng business logic
- ? Reusable queries

### T?i sao dùng DTO thay vì Entity?
- ? Không expose internal structure
- ? Ki?m soát data ???c tr? v?
- ? Versioning API d? dàng h?n
- ? Security (không tr? v? sensitive data)

### Next Steps
1. ? Thêm FluentValidation cho input validation
2. ? Implement CQRS v?i MediatR
3. ? Thêm Unit Tests & Integration Tests
4. ? Implement caching v?i Redis
5. ? Add authentication/authorization
6. ? Implement event publishing v?i RabbitMQ
