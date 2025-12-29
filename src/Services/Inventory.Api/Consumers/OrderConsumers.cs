using EventBus.Messages.Events.Order;
using Inventory.Api.DTOs;
using Inventory.Api.Repositories.Interfaces;
using MassTransit;

namespace Inventory.Api.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IInventoryRepository inventoryRepository, ILogger<OrderCreatedConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received OrderCreatedEvent: {OrderId}", message.OrderId);

        // Reserve stock for each item in the order
        foreach (var item in message.Items)
        {
            var reserveDto = new ReserveStockDto
            {
                OrderId = message.OrderId,
                Quantity = item.Quantity
            };

            var success = await _inventoryRepository.ReserveStockAsync(item.ProductId, reserveDto);
            
            if (!success)
            {
                _logger.LogError("Failed to reserve stock for Product {ProductId} in Order {OrderId}",
                    item.ProductId, message.OrderId);
                // In a real system, you'd want to implement compensation logic here
            }
        }

        _logger.LogInformation("Reserved stock for Order {OrderId}", message.OrderId);
    }
}

public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(IInventoryRepository inventoryRepository, ILogger<OrderCancelledConsumer> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received OrderCancelledEvent: {OrderId}", message.OrderId);

        // Note: In a real system, you'd need to get order items from somewhere
        // For now, this is a placeholder - you might need to store order reservations
        
        _logger.LogInformation("Order {OrderId} cancelled, stock should be released", message.OrderId);
    }
}
