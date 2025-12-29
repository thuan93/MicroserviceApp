using EventBus.Messages.Events.Product;
using Inventory.Api.DTOs;
using Inventory.Api.Repositories.Interfaces;
using MassTransit;

namespace Inventory.Api.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(IInventoryRepository inventoryRepository, ILogger<ProductCreatedConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProductCreatedEvent: {ProductId} - {Name}", message.ProductId, message.Name);

        // Check if inventory already exists
        var existing = await _inventoryRepository.GetByProductIdAsync(message.ProductId);
        if (existing != null)
        {
            _logger.LogWarning("Inventory for Product {ProductId} already exists", message.ProductId);
            return;
        }

        // Create inventory
        var dto = new CreateInventoryDto
        {
            ProductId = message.ProductId,
            ProductName = message.Name,
            InitialStock = message.StockQuantity,
            MinimumStock = 10
        };

        await _inventoryRepository.CreateAsync(dto);
        _logger.LogInformation("Created inventory for Product {ProductId}", message.ProductId);
    }
}

public class ProductUpdatedConsumer : IConsumer<ProductUpdatedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<ProductUpdatedConsumer> _logger;

    public ProductUpdatedConsumer(IInventoryRepository inventoryRepository, ILogger<ProductUpdatedConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProductUpdatedEvent: {ProductId}", message.ProductId);

        // Update inventory if product exists
        var existing = await _inventoryRepository.GetByProductIdAsync(message.ProductId);
        if (existing == null)
        {
            _logger.LogWarning("Inventory for Product {ProductId} not found", message.ProductId);
            return;
        }

        _logger.LogInformation("Product {ProductId} updated, inventory unchanged", message.ProductId);
    }
}

public class ProductStockUpdatedConsumer : IConsumer<ProductStockUpdatedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<ProductStockUpdatedConsumer> _logger;

    public ProductStockUpdatedConsumer(IInventoryRepository inventoryRepository, ILogger<ProductStockUpdatedConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductStockUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProductStockUpdatedEvent: {ProductId} {OldQty} ? {NewQty}",
            message.ProductId, message.OldQuantity, message.NewQuantity);

        var difference = message.NewQuantity - message.OldQuantity;
        await _inventoryRepository.UpdateStockAsync(message.ProductId, difference);

        _logger.LogInformation("Updated inventory for Product {ProductId}", message.ProductId);
    }
}

public class ProductDeletedConsumer : IConsumer<ProductDeletedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<ProductDeletedConsumer> _logger;

    public ProductDeletedConsumer(IInventoryRepository inventoryRepository, ILogger<ProductDeletedConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProductDeletedEvent: {ProductId}", message.ProductId);

        var inventory = await _inventoryRepository.GetByProductIdAsync(message.ProductId);
        if (inventory != null)
        {
            await _inventoryRepository.DeleteAsync(inventory.Id);
            _logger.LogInformation("Deleted inventory for Product {ProductId}", message.ProductId);
        }
    }
}
