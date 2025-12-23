using Product.Api.DTOs;

namespace Product.Api.Repositories.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(long id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<bool> UpdateAsync(long id, UpdateProductDto dto);
    Task<bool> DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
}
