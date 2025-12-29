# Shared BuildingBlock

## M?c ?ích
Ch?a các utilities, extensions, constants, và exceptions dùng chung cho t?t c? microservices.

## C?u trúc

```
Shared/
??? Constants/
?   ??? ApiConstants          # API constants
??? DTOs/
?   ??? ApiResponse<T>        # Standardized API response
?   ??? PaginatedResult<T>    # Pagination wrapper
??? Extensions/
?   ??? StringExtensions      # String helper methods
?   ??? DateTimeExtensions    # DateTime helper methods
??? Exceptions/
    ??? CustomExceptions      # Custom exception classes
```

## Cách s? d?ng

### 1. ApiResponse - Standardized Response

```csharp
// Success response
return Ok(ApiResponse<ProductDto>.SuccessResult(product, "Product retrieved successfully"));

// Failure response
return BadRequest(ApiResponse<ProductDto>.FailureResult("Product not found", new List<string> { "Id is invalid" }));
```

### 2. PaginatedResult - Pagination

```csharp
var products = await _context.Products
    .Skip((pageIndex - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

var totalCount = await _context.Products.CountAsync();

var result = new PaginatedResult<Product>(products, totalCount, pageIndex, pageSize);
```

### 3. Custom Exceptions

```csharp
// Not Found
throw new NotFoundException(nameof(Product), productId);

// Validation Error
throw new ValidationException(new List<string> { "Name is required", "Price must be positive" });

// Bad Request
throw new BadRequestException("Invalid product data");
```

### 4. String Extensions

```csharp
var slug = productName.ToSlug(); // "Gaming Mouse" -> "gaming-mouse"
var truncated = description.Truncate(100);
if (name.IsNullOrEmpty()) { }
```

### 5. DateTime Extensions

```csharp
var friendly = createdDate.ToFriendlyDate(); // "2 hours ago"
if (orderDate.IsToday()) { }
```

## Features

? Standardized API response format  
? Pagination support  
? Custom exceptions  
? Common string extensions  
? DateTime utilities  
? Reusable constants  

## Dependencies

Không có external dependencies - Pure .NET 10
