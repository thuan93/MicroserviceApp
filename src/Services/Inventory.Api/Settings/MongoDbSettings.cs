namespace Inventory.Api.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string InventoryCollectionName { get; set; } = "ProductInventory";
    public string StockMovementCollectionName { get; set; } = "StockMovements";
}
