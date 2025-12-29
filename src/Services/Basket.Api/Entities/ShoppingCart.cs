namespace Basket.Api.Entities;

public class ShoppingCart
{
    public string UserName { get; set; } = string.Empty;
    public List<ShoppingCartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
    
    public ShoppingCart()
    {
    }
    
    public ShoppingCart(string userName)
    {
        UserName = userName;
    }
}

public class ShoppingCartItem
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}
