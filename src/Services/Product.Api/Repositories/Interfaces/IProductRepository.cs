using Infrastructure.Repositories;
using Product.Api.DTOs;

namespace Product.Api.Repositories.Interfaces;

public interface IProductRepository : IRepository<Entities.Product>
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(long id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<bool> UpdateProductAsync(long id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(long id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(long categoryId);
}
