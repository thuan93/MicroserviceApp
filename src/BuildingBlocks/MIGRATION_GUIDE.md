# Migration Guide - Refactor Product.Api to use BuildingBlocks

## ?? T?ng quan

Document này h??ng d?n refactor Product.Api ?? s? d?ng các BuildingBlocks ?ã implement.

## ?? M?c tiêu

- ? S? d?ng `Infrastructure.Repositories.RepositoryBase` thay vì custom repository
- ? S? d?ng `Shared.DTOs.ApiResponse` cho consistent API responses
- ? S? d?ng `Contracts.DTOs` thay vì local DTOs
- ? S? d?ng `EventBus.Messages` ?? publish events
- ? S? d?ng `Shared.Exceptions` cho error handling

## ?? Step-by-Step Migration

### Step 1: Update DTOs to use Contracts

**Before:**
```csharp
// Product.Api/DTOs/ProductDto.cs
namespace Product.Api.DTOs;

public record ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // ...
}
```

**After:**
```csharp
// Use from Contracts
using Contracts.DTOs.Product;
// Remove local DTOs folder
```

### Step 2: Update Repository to use Infrastructure

**Before:**
```csharp
// Product.Api/Repositories/IProductRepository.cs
public interface IProductRepository
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(long id);
    // ...
}
```

**After:**
```csharp
using Infrastructure.Repositories;
using Contracts.DTOs.Product;

public interface IProductRepository : IRepository<Entities.Product>
{
    // Only keep product-specific methods
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(long id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(long categoryId);
}

public class ProductRepository : RepositoryBase<Entities.Product, ProductContext>, IProductRepository
{
    public ProductRepository(ProductContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await GetAllAsync();
        // Map to DTOs
    }
    
    // Implement other methods...
}
```

### Step 3: Update Controller to use ApiResponse

**Before:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(long id)
{
    var product = await _repository.GetByIdAsync(id);
    if (product == null)
        return NotFound();
    return Ok(product);
}
```

**After:**
```csharp
using Shared.DTOs;
using Shared.Exceptions;
using Contracts.DTOs.Product;

[HttpGet("{id}")]
public async Task<IActionResult> GetById(long id)
{
    try
    {
        var product = await _repository.GetProductByIdAsync(id);
        if (product == null)
            throw new NotFoundException(nameof(Product), id);
            
        return Ok(ApiResponse<ProductDto>.SuccessResult(product));
    }
    catch (NotFoundException ex)
    {
        return NotFound(ApiResponse<ProductDto>.FailureResult(ex.Message));
    }
}
```

### Step 4: Add Event Publishing

**Install MassTransit:**
```bash
dotnet add package MassTransit
dotnet add package MassTransit.RabbitMQ
```

**Update ServiceExtensions.cs:**
```csharp
using MassTransit;

public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing code ...
    
    // MassTransit RabbitMQ
    services.AddMassTransit(config =>
    {
        config.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(configuration["RabbitMQ:Password"] ?? "guest");
            });
        });
    });

    return services;
}
```

**Update appsettings.json:**
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

**Publish events in repository/service:**
```csharp
using EventBus.Messages.Events.Product;
using MassTransit;

public class ProductRepository : RepositoryBase<Entities.Product, ProductContext>, IProductRepository
{
    private readonly IPublishEndpoint _publishEndpoint;

    public ProductRepository(ProductContext context, IPublishEndpoint publishEndpoint) 
        : base(context)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = // ... create product
        var result = await AddAsync(product);
        
        // Publish event
        await _publishEndpoint.Publish(new ProductCreatedEvent
        {
            ProductId = result.Id,
            Name = result.Name,
            Price = result.Price,
            StockQuantity = result.StockQuantity,
            CategoryId = result.CategoryId,
            SupplierId = result.SupplierId
        });
        
        return MapToDto(result);
    }
}
```

### Step 5: Add Global Exception Handler (Optional)

**Create Middleware:**
```csharp
using Shared.Exceptions;
using Shared.DTOs;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode, List<string>? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.FailureResult(exception.Message, errors);
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

**Register in Program.cs:**
```csharp
app.UseMiddleware<ExceptionHandlerMiddleware>();
```

### Step 6: Update Entity to implement Contracts interfaces

**Before:**
```csharp
public class Product
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    // ...
}
```

**After:**
```csharp
using Contracts.Common;

public class Product : IEntityBase, IAuditableEntity
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    // ...
}
```

## ? Verification Checklist

After migration:
- [ ] All DTOs use Contracts namespace
- [ ] Repository extends RepositoryBase
- [ ] Controllers return ApiResponse<T>
- [ ] Events are published on Create/Update/Delete
- [ ] Exception handling uses Shared.Exceptions
- [ ] Build succeeds
- [ ] Tests pass (if any)
- [ ] Swagger UI works correctly

## ?? Benefits After Migration

? Less code to maintain  
? Consistent responses across services  
? Event-driven architecture  
? Standardized error handling  
? Reusable patterns  
? Better separation of concerns  

## ?? Next Steps

1. Refactor Product.Api (this guide)
2. Apply same patterns to Customer.Api
3. Apply to Ordering.Api
4. Apply to Inventory.Api
5. Implement event consumers
6. Add integration tests

## ?? Notes

- Migration không c?n làm 1 lúc - có th? làm t?ng step
- Test thoroughly sau m?i step
- Keep backward compatibility n?u có consumers
- Document any breaking changes
