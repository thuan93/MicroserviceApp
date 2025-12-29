using Infrastructure.Repositories;
using Customer.Api.DTOs;

namespace Customer.Api.Repositories.Interfaces;

public interface ICustomerRepository : IRepository<Entities.Customer>
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(long id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<bool> UpdateCustomerAsync(long id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(long id);
}
