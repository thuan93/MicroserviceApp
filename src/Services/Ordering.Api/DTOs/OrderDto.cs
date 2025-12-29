using Ordering.Api.Entities;

namespace Ordering.Api.DTOs;

public record OrderDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingCountry { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public record OrderItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public record CreateOrderDto
{
    public long CustomerId { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingCountry { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public record CreateOrderItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public record UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
