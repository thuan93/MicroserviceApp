using Inventory.Api.DTOs;

namespace Inventory.Api.Repositories.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryDto>> GetAllAsync();
    Task<InventoryDto?> GetByIdAsync(string id);
    Task<InventoryDto?> GetByProductIdAsync(long productId);
    Task<InventoryDto> CreateAsync(CreateInventoryDto dto);
    Task<bool> UpdateStockAsync(long productId, int quantity);
    Task<bool> ReserveStockAsync(long productId, ReserveStockDto dto);
    Task<bool> ReleaseStockAsync(long productId, ReleaseStockDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(long productId);
    Task<IEnumerable<InventoryDto>> GetLowStockProductsAsync();
}
