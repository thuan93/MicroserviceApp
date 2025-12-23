# Product.Api - Implementation Checklist

## ? ?ã hoàn thành

- [x] C?u trúc folder c? b?n
- [x] Entity models (Product, Category)
- [x] DTOs (ProductDto, CreateProductDto, UpdateProductDto)
- [x] DbContext & Entity Configurations
- [x] Repository Pattern (IProductRepository, ProductRepository)
- [x] Controllers (ProductsController)
- [x] Dependency Injection setup
- [x] Serilog logging configuration
- [x] Health Checks integration
- [x] Swagger documentation
- [x] MySQL connection string

## ?? C?n làm ti?p

### Phase 1: Database & Basic Functionality
- [ ] Ch?y EF Core Migration
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```
- [ ] Seed initial data (Categories & Products)
- [ ] Test CRUD operations qua Swagger
- [ ] Verify logging trong Output window

### Phase 2: Validation & Error Handling
- [ ] Implement FluentValidation
  - [ ] CreateProductDtoValidator
  - [ ] UpdateProductDtoValidator
- [ ] Global Exception Handler middleware
- [ ] Model validation responses
- [ ] Custom error response format

### Phase 3: Business Logic Layer
- [ ] T?o Services layer
  - [ ] IProductService
  - [ ] ProductService (business logic)
- [ ] Implement business rules
  - [ ] Check stock availability
  - [ ] Price validation
  - [ ] Category existence check

### Phase 4: Event-Driven Architecture
- [ ] Implement RabbitMQ integration
  - [ ] ProductCreatedEvent
  - [ ] ProductUpdatedEvent
  - [ ] ProductDeletedEvent
- [ ] Event Publishers
- [ ] Event Consumers (n?u c?n)

### Phase 5: Caching
- [ ] Redis integration
- [ ] Cache products list
- [ ] Cache single product by ID
- [ ] Cache invalidation strategy

### Phase 6: Advanced Features
- [ ] Implement CQRS v?i MediatR
  - [ ] Commands (Create, Update, Delete)
  - [ ] Queries (GetAll, GetById)
  - [ ] Handlers
- [ ] Pagination support
- [ ] Filtering & Sorting
- [ ] Search functionality

### Phase 7: Security
- [ ] JWT Authentication
- [ ] Authorization policies
- [ ] API Key authentication
- [ ] Rate limiting

### Phase 8: Testing
- [ ] Unit Tests
  - [ ] Repository tests
  - [ ] Service tests
  - [ ] Validator tests
- [ ] Integration Tests
  - [ ] Controller tests
  - [ ] Database tests
- [ ] Performance Tests

### Phase 9: Monitoring & Observability
- [ ] Elasticsearch integration
- [ ] Kibana dashboards
- [ ] Application Insights (optional)
- [ ] Distributed tracing

### Phase 10: Docker & Deployment
- [ ] Create Dockerfile
- [ ] Update docker-compose.yml
- [ ] Environment-specific configs
- [ ] CI/CD pipeline

## ?? Chi ti?t t?ng b??c

### 1. Ch?y Migration & Seed Data

**Migration:**
```bash
cd src/Services/Product.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Seed Data (t?o file):**
```csharp
// Persistence/ProductContextSeed.cs
public static class ProductContextSeed
{
    public static async Task SeedAsync(ProductContext context)
    {
        if (!context.Categories.Any())
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "Electronics", Description = "Electronic devices" },
                new Category { Name = "Clothing", Description = "Fashion items" },
                new Category { Name = "Books", Description = "Books and magazines" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Products.Any())
        {
            var electronics = await context.Categories.FirstAsync(c => c.Name == "Electronics");
            await context.Products.AddRangeAsync(
                new Entities.Product
                {
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 999.99M,
                    StockQuantity = 50,
                    CategoryId = electronics.Id,
                    CreatedDate = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }
    }
}
```

**Update Program.cs:**
```csharp
// After app.Build()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
    await ProductContextSeed.SeedAsync(context);
}
```

### 2. FluentValidation Implementation

**CreateProductDtoValidator:**
```csharp
// Validators/CreateProductDtoValidator.cs
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required");
    }
}
```

**Register in ServiceExtensions:**
```csharp
services.AddFluentValidationAutoValidation();
services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
```

### 3. Global Exception Handler

**Middleware/ExceptionMiddleware.cs:**
```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error",
            Detailed = exception.Message
        });
    }
}
```

### 4. RabbitMQ Event Publishing

**Events/ProductCreatedEvent.cs:**
```csharp
public record ProductCreatedEvent
{
    public long ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

**Update Repository sau khi Create:**
```csharp
// Publish event
await _publishEndpoint.Publish(new ProductCreatedEvent
{
    ProductId = product.Id,
    ProductName = product.Name,
    Price = product.Price,
    CreatedAt = DateTime.UtcNow
});
```

### 5. Redis Caching

**Install package:**
```xml
<PackageReference Include="StackExchange.Redis" Version="2.7.20" />
```

**Caching Service:**
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}
```

## ?? Priority Order

1. **High Priority** (Làm ngay)
   - Migration & Seed Data
   - Test CRUD operations
   - FluentValidation
   - Error handling

2. **Medium Priority** (Tu?n t?i)
   - Services layer
   - RabbitMQ events
   - Basic caching
   - Unit tests

3. **Low Priority** (Khi có th?i gian)
   - CQRS pattern
   - Advanced features
   - Performance optimization
   - Monitoring

## ?? Resources

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [MassTransit](https://masstransit.io/)
- [Redis Caching](https://redis.io/docs/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
