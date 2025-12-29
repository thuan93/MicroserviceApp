using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Inventory.Api.Entities;

public class ProductInventory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("productId")]
    public long ProductId { get; set; }
    
    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;
    
    [BsonElement("availableStock")]
    public int AvailableStock { get; set; }
    
    [BsonElement("reservedStock")]
    public int ReservedStock { get; set; }
    
    [BsonElement("minimumStock")]
    public int MinimumStock { get; set; }
    
    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }
    
    [BsonElement("updatedDate")]
    public DateTime? UpdatedDate { get; set; }
    
    public int TotalStock => AvailableStock + ReservedStock;
    public bool IsLowStock => AvailableStock <= MinimumStock;
}
