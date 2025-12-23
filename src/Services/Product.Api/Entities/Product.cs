namespace Product.Api.Entities;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public long CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
