# Contracts BuildingBlock

## Mục đích
Chứa các DTOs và interfaces dùng chung giữa các microservices nhằm bảo toàn tính nhất quán.

## Cấu trúc

```
Contracts/
    Common/
        Interfaces           # Base interfaces (IEntityBase, IAuditableEntity, ISoftDelete)
    DTOs/
        Product/
            ProductDtos      # Product DTOs
        Category/
            CategoryDtos     # Category DTOs
        Supplier/
            SupplierDtos     # Supplier DTOs
```

## Cách sử dụng

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
// Trong Ordering.Api cũng có thể sử dụng
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

## Lợi ích

? **Consistency**: Đảm bảo DTOs giống nhau giữa các services  
? **Reusability**: Tái sử dụng DTOs thay vì duplicate  
? **Type Safety**: Compile-time checking khi giao tiếp giữa services  
? **Maintainability**: Thay đổi 1 chỗ, apply cho tất cả services  

## Khi nào dùng?

- ? DTOs được share giữa nhiều services
- ? Common interfaces cho entities
- ? Validation rules dùng chung
- ? Business logic (nên để trong service layer)
- ? Infrastructure concerns

## Dependencies

Không có external dependencies - Pure .NET 10
