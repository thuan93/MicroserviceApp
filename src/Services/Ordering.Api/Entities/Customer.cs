using Contracts.Common;

namespace Ordering.Api.Entities;

public class Customer : IEntityBase
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
