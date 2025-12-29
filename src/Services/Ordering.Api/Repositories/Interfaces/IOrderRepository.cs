using Infrastructure.Repositories;
using Ordering.Api.DTOs;

namespace Ordering.Api.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Entities.Order>
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(long id);
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(long customerId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<bool> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto);
    Task<bool> CancelOrderAsync(long id);
    Task<string> GenerateOrderNumberAsync();
}
