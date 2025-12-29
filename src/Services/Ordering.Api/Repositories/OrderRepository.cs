using EventBus.Messages.Events.Order;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.DTOs;
using Ordering.Api.Entities;
using Ordering.Api.Persistence;
using Ordering.Api.Repositories.Interfaces;

namespace Ordering.Api.Repositories;

public class OrderRepository : RepositoryBase<Order, OrderingContext>, IOrderRepository
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(OrderingContext context, IPublishEndpoint publishEndpoint, ILogger<OrderRepository> logger) 
        : base(context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(long id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(long customerId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        // Get or create customer
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId);
        if (customer == null)
        {
            throw new Exception($"Customer with id {dto.CustomerId} not found");
        }

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            CustomerId = dto.CustomerId,
            Status = OrderStatus.Pending,
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            ShippingCountry = dto.ShippingCountry,
            CreatedDate = DateTime.UtcNow,
            OrderItems = dto.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);

        await AddAsync(order);

        // Publish OrderCreatedEvent
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(oi => new EventBus.Messages.Events.Order.OrderItemDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                Price = oi.UnitPrice
            }).ToList()
        };

        await _publishEndpoint.Publish(orderCreatedEvent);
        _logger.LogInformation("Published OrderCreatedEvent for Order {OrderId}", order.Id);

        return (await GetOrderByIdAsync(order.Id))!;
    }

    public async Task<bool> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto)
    {
        var order = await GetByIdAsync(id);
        if (order == null) return false;

        order.Status = dto.Status;
        order.UpdatedDate = DateTime.UtcNow;

        await UpdateAsync(order);

        // Publish OrderUpdatedEvent
        var orderUpdatedEvent = new OrderUpdatedEvent
        {
            OrderId = order.Id,
            Status = order.Status.ToString()
        };

        await _publishEndpoint.Publish(orderUpdatedEvent);
        _logger.LogInformation("Published OrderUpdatedEvent for Order {OrderId}", order.Id);

        return true;
    }

    public async Task<bool> CancelOrderAsync(long id)
    {
        var order = await GetByIdAsync(id);
        if (order == null) return false;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedDate = DateTime.UtcNow;

        await UpdateAsync(order);

        // Publish OrderCancelledEvent
        var orderCancelledEvent = new OrderCancelledEvent
        {
            OrderId = order.Id
        };

        await _publishEndpoint.Publish(orderCancelledEvent);
        _logger.LogInformation("Published OrderCancelledEvent for Order {OrderId}", order.Id);

        return true;
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"ORD{date:yyyyMMdd}";
        
        var lastOrder = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Name,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingCountry = order.ShippingCountry,
            CreatedDate = order.CreatedDate,
            Items = order.OrderItems.Select(oi => new DTOs.OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}
