using Customer.Api.DTOs;
using Infrastructure.Repositories;
using Shared.DTOs;

namespace Customer.Api.Repositories.Interfaces;

public interface ICustomerRepository : IRepository<Entities.Customer>
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<PaginatedResult<CustomerDto>> GetCustomersPagedAsync(int pageIndex, int pageSize);
    Task<CustomerDto?> GetCustomerByIdAsync(long id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<bool> UpdateCustomerAsync(long id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(long id);
}
