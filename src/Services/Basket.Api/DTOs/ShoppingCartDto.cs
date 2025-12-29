namespace Basket.Api.DTOs;

public record ShoppingCartDto
{
    public string UserName { get; set; } = string.Empty;
    public List<ShoppingCartItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
}

public record ShoppingCartItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

public record AddItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

public record UpdateItemQuantityDto
{
    public int Quantity { get; set; }
}

public record CheckoutDto
{
    public string UserName { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingCountry { get; set; }
}
