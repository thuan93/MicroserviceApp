# Contracts BuildingBlock

## M?c ?ích
Ch?a các DTOs và interfaces dùng chung gi?a các microservices ?? ??m b?o tính nh?t quán.

## C?u trúc

```
Contracts/
??? Common/
?   ??? Interfaces            # Base interfaces (IEntityBase, IAuditableEntity, ISoftDelete)
??? DTOs/
    ??? Product/
    ?   ??? ProductDtos      # Product DTOs
    ??? Category/
    ?   ??? CategoryDtos     # Category DTOs
    ??? Supplier/
        ??? SupplierDtos     # Supplier DTOs
```

## Cách s? d?ng

### 1. Base Interfaces

```csharp
public class Product : IEntityBase, IAuditableEntity
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    // ... other properties
}
```

### 2. Shared DTOs

```csharp
// Trong Product.Api
using Contracts.DTOs.Product;

public async Task<ProductDto> GetProductAsync(long id)
{
    var product = await _repository.GetByIdAsync(id);
    return _mapper.Map<ProductDto>(product);
}
```

```csharp
// Trong Ordering.Api c?ng có th? s? d?ng
using Contracts.DTOs.Product;

public class OrderService
{
    public async Task ValidateProduct(ProductDto product)
    {
        // Both services use same DTO contract
    }
}
```

### 3. Soft Delete Support

```csharp
public class Category : IEntityBase, ISoftDelete
{
    public long Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
}
```

## L?i ích

? **Consistency**: ??m b?o DTOs gi?ng nhau gi?a các services  
? **Reusability**: Tái s? d?ng DTOs thay vì duplicate  
? **Type Safety**: Compile-time checking khi giao ti?p gi?a services  
? **Maintainability**: Thay ??i 1 ch?, apply cho t?t c? services  

## Khi nào dùng?

- ? DTOs ???c share gi?a nhi?u services
- ? Common interfaces cho entities
- ? Validation rules dùng chung
- ? Business logic (nên ?? trong service layer)
- ? Infrastructure concerns

## Dependencies

Không có external dependencies - Pure .NET 10
