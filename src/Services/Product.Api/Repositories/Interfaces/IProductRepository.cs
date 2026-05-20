using Infrastructure.Repositories;
using Product.Api.DTOs;
using Shared.DTOs;

namespace Product.Api.Repositories.Interfaces;

public interface IProductRepository : IRepository<Entities.Product>
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<PaginatedResult<ProductDto>> GetProductsPagedAsync(int pageIndex, int pageSize);
    Task<ProductDto?> GetProductByIdAsync(long id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<bool> UpdateProductAsync(long id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(long id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(long categoryId);
}
