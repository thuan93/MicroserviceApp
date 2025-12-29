namespace Inventory.Api.DTOs;

public record InventoryDto
{
    public string Id { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int AvailableStock { get; set; }
    public int ReservedStock { get; set; }
    public int TotalStock { get; set; }
    public int MinimumStock { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateInventoryDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int InitialStock { get; set; }
    public int MinimumStock { get; set; } = 10;
}

public record UpdateStockDto
{
    public int Quantity { get; set; }
}

public record ReserveStockDto
{
    public long OrderId { get; set; }
    public int Quantity { get; set; }
}

public record ReleaseStockDto
{
    public long OrderId { get; set; }
    public int Quantity { get; set; }
}

public record StockMovementDto
{
    public string Id { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public long? OrderId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedDate { get; set; }
}
