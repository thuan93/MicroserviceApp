using EventBus.Messages.Events.Inventory;
using Inventory.Api.DTOs;
using Inventory.Api.Entities;
using Inventory.Api.Repositories.Interfaces;
using Inventory.Api.Settings;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Inventory.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<ProductInventory> _inventoryCollection;
    private readonly IMongoCollection<StockMovement> _stockMovementCollection;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<InventoryRepository> _logger;

    public InventoryRepository(
        IOptions<MongoDbSettings> settings,
        IPublishEndpoint publishEndpoint,
        ILogger<InventoryRepository> logger)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        
        _inventoryCollection = database.GetCollection<ProductInventory>(settings.Value.InventoryCollectionName);
        _stockMovementCollection = database.GetCollection<StockMovement>(settings.Value.StockMovementCollectionName);
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllAsync()
    {
        var inventories = await _inventoryCollection.Find(_ => true).ToListAsync();
        return inventories.Select(MapToDto);
    }

    public async Task<InventoryDto?> GetByIdAsync(string id)
    {
        var inventory = await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        return inventory == null ? null : MapToDto(inventory);
    }

    public async Task<InventoryDto?> GetByProductIdAsync(long productId)
    {
        var inventory = await _inventoryCollection.Find(i => i.ProductId == productId).FirstOrDefaultAsync();
        return inventory == null ? null : MapToDto(inventory);
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto)
    {
        var inventory = new ProductInventory
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            AvailableStock = dto.InitialStock,
            ReservedStock = 0,
            MinimumStock = dto.MinimumStock,
            CreatedDate = DateTime.UtcNow
        };

        await _inventoryCollection.InsertOneAsync(inventory);

        // Create stock movement record
        await CreateStockMovement(dto.ProductId, StockMovementType.StockIn, dto.InitialStock, null, "Initial stock");

        _logger.LogInformation("Created inventory for Product {ProductId}", dto.ProductId);

        return MapToDto(inventory);
    }

    public async Task<bool> UpdateStockAsync(long productId, int quantity)
    {
        var inventory = await _inventoryCollection.Find(i => i.ProductId == productId).FirstOrDefaultAsync();
        if (inventory == null) return false;

        var oldStock = inventory.AvailableStock;
        inventory.AvailableStock += quantity;
        inventory.UpdatedDate = DateTime.UtcNow;

        await _inventoryCollection.ReplaceOneAsync(i => i.ProductId == productId, inventory);

        // Create stock movement
        var movementType = quantity > 0 ? StockMovementType.StockIn : StockMovementType.StockOut;
        await CreateStockMovement(productId, movementType, Math.Abs(quantity), null, "Stock adjustment");

        // Check for low stock
        if (inventory.IsLowStock)
        {
            await _publishEndpoint.Publish(new InventoryLowStockEvent
            {
                ProductId = productId,
                CurrentStock = inventory.AvailableStock,
                MinimumStock = inventory.MinimumStock
            });
            _logger.LogWarning("Low stock alert for Product {ProductId}", productId);
        }

        _logger.LogInformation("Updated stock for Product {ProductId}: {OldStock} ? {NewStock}", 
            productId, oldStock, inventory.AvailableStock);

        return true;
    }

    public async Task<bool> ReserveStockAsync(long productId, ReserveStockDto dto)
    {
        var inventory = await _inventoryCollection.Find(i => i.ProductId == productId).FirstOrDefaultAsync();
        if (inventory == null) return false;

        if (inventory.AvailableStock < dto.Quantity)
        {
            _logger.LogWarning("Insufficient stock for Product {ProductId}. Available: {Available}, Requested: {Requested}",
                productId, inventory.AvailableStock, dto.Quantity);
            return false;
        }

        inventory.AvailableStock -= dto.Quantity;
        inventory.ReservedStock += dto.Quantity;
        inventory.UpdatedDate = DateTime.UtcNow;

        await _inventoryCollection.ReplaceOneAsync(i => i.ProductId == productId, inventory);

        // Create stock movement
        await CreateStockMovement(productId, StockMovementType.Reserved, dto.Quantity, dto.OrderId, "Reserved for order");

        // Publish event
        await _publishEndpoint.Publish(new InventoryReservedEvent
        {
            ProductId = productId,
            Quantity = dto.Quantity,
            OrderId = dto.OrderId
        });

        _logger.LogInformation("Reserved {Quantity} units of Product {ProductId} for Order {OrderId}",
            dto.Quantity, productId, dto.OrderId);

        return true;
    }

    public async Task<bool> ReleaseStockAsync(long productId, ReleaseStockDto dto)
    {
        var inventory = await _inventoryCollection.Find(i => i.ProductId == productId).FirstOrDefaultAsync();
        if (inventory == null) return false;

        if (inventory.ReservedStock < dto.Quantity)
        {
            _logger.LogWarning("Insufficient reserved stock for Product {ProductId}. Reserved: {Reserved}, Requested: {Requested}",
                productId, inventory.ReservedStock, dto.Quantity);
            return false;
        }

        inventory.ReservedStock -= dto.Quantity;
        inventory.AvailableStock += dto.Quantity;
        inventory.UpdatedDate = DateTime.UtcNow;

        await _inventoryCollection.ReplaceOneAsync(i => i.ProductId == productId, inventory);

        // Create stock movement
        await CreateStockMovement(productId, StockMovementType.Released, dto.Quantity, dto.OrderId, "Released from order");

        // Publish event
        await _publishEndpoint.Publish(new InventoryReleasedEvent
        {
            ProductId = productId,
            Quantity = dto.Quantity,
            OrderId = dto.OrderId
        });

        _logger.LogInformation("Released {Quantity} units of Product {ProductId} from Order {OrderId}",
            dto.Quantity, productId, dto.OrderId);

        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _inventoryCollection.DeleteOneAsync(i => i.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(long productId)
    {
        var movements = await _stockMovementCollection
            .Find(m => m.ProductId == productId)
            .SortByDescending(m => m.CreatedDate)
            .Limit(50)
            .ToListAsync();

        return movements.Select(m => new StockMovementDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            MovementType = m.MovementType.ToString(),
            Quantity = m.Quantity,
            OrderId = m.OrderId,
            Reason = m.Reason,
            CreatedDate = m.CreatedDate
        });
    }

    public async Task<IEnumerable<InventoryDto>> GetLowStockProductsAsync()
    {
        var inventories = await _inventoryCollection
            .Find(i => i.AvailableStock <= i.MinimumStock)
            .ToListAsync();

        return inventories.Select(MapToDto);
    }

    private async Task CreateStockMovement(long productId, StockMovementType type, int quantity, long? orderId, string? reason)
    {
        var movement = new StockMovement
        {
            ProductId = productId,
            MovementType = type,
            Quantity = quantity,
            OrderId = orderId,
            Reason = reason,
            CreatedDate = DateTime.UtcNow
        };

        await _stockMovementCollection.InsertOneAsync(movement);
    }

    private static InventoryDto MapToDto(ProductInventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductName = inventory.ProductName,
            AvailableStock = inventory.AvailableStock,
            ReservedStock = inventory.ReservedStock,
            TotalStock = inventory.TotalStock,
            MinimumStock = inventory.MinimumStock,
            IsLowStock = inventory.IsLowStock,
            CreatedDate = inventory.CreatedDate
        };
    }
}
