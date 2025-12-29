# Infrastructure BuildingBlock

## M?c ?ích
Cung c?p các base classes và interfaces cho Data Access Layer, áp d?ng Repository Pattern và Unit of Work Pattern.

## C?u trúc

```
Infrastructure/
??? Repositories/
?   ??? IRepository<T>           # Generic repository interface
?   ??? RepositoryBase<T>        # Base repository implementation
?   ??? UnitOfWork               # Unit of Work pattern
??? Specifications/
    ??? ISpecification<T>        # Specification interface
    ??? BaseSpecification<T>     # Base specification implementation
```

## Cách s? d?ng

### 1. Repository Pattern

**T?o repository c? th?:**
```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId);
}

public class ProductRepository : RepositoryBase<Product, ProductContext>, IProductRepository
{
    public ProductRepository(ProductContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId)
    {
        return await FindAsync(p => p.CategoryId == categoryId);
    }
}
```

### 2. Unit of Work Pattern

**??ng ký trong DI:**
```csharp
services.AddScoped<IUnitOfWork, UnitOfWork<ProductContext>>();
```

**S? d?ng:**
```csharp
public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task UpdateProductWithTransaction(Product product)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _productRepository.UpdateAsync(product);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### 3. Specification Pattern

**T?o specification:**
```csharp
public class ProductsByCategorySpec : BaseSpecification<Product>
{
    public ProductsByCategorySpec(long categoryId) 
        : base(p => p.CategoryId == categoryId)
    {
        AddInclude(p => p.Category);
        AddInclude(p => p.Supplier);
        ApplyOrderBy(p => p.Name);
    }
}
```

## Features

? Generic repository v?i CRUD operations  
? Unit of Work cho transaction management  
? Specification pattern cho complex queries  
? Support Include/ThenInclude cho eager loading  
? Pagination support  
? Async/await throughout  

## Dependencies

- Microsoft.EntityFrameworkCore (9.0.0)
