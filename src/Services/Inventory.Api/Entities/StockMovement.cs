using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Inventory.Api.Entities;

public class StockMovement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("productId")]
    public long ProductId { get; set; }
    
    [BsonElement("movementType")]
    public StockMovementType MovementType { get; set; }
    
    [BsonElement("quantity")]
    public int Quantity { get; set; }
    
    [BsonElement("orderId")]
    public long? OrderId { get; set; }
    
    [BsonElement("reason")]
    public string? Reason { get; set; }
    
    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }
}

public enum StockMovementType
{
    StockIn = 0,
    StockOut = 1,
    Reserved = 2,
    Released = 3,
    Adjustment = 4
}
