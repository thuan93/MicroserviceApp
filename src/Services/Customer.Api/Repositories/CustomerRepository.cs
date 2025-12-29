using EventBus.Messages.Events.Customer;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Customer.Api.DTOs;
using Customer.Api.Persistence;
using Customer.Api.Repositories.Interfaces;

namespace Customer.Api.Repositories;

public class CustomerRepository : RepositoryBase<Entities.Customer, CustomerContext>, ICustomerRepository
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(CustomerContext context, IPublishEndpoint publishEndpoint, ILogger<CustomerRepository> logger) 
        : base(context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                CreatedDate = c.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(long id)
    {
        return await _context.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                CreatedDate = c.CreatedDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        return await _context.Customers
            .Where(c => c.Email == email)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                CreatedDate = c.CreatedDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var customer = new Entities.Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            CreatedDate = DateTime.UtcNow
        };

        await AddAsync(customer);

        // Publish CustomerCreatedEvent
        var customerCreatedEvent = new CustomerCreatedEvent
        {
            CustomerId = customer.Id,
            Name = $"{customer.FirstName} {customer.LastName}",
            Email = customer.Email
        };

        await _publishEndpoint.Publish(customerCreatedEvent);
        _logger.LogInformation("Published CustomerCreatedEvent for Customer {CustomerId}", customer.Id);

        return (await GetCustomerByIdAsync(customer.Id))!;
    }

    public async Task<bool> UpdateCustomerAsync(long id, UpdateCustomerDto dto)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null) return false;

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.City = dto.City;
        customer.Country = dto.Country;
        customer.UpdatedDate = DateTime.UtcNow;

        await UpdateAsync(customer);

        // Publish CustomerUpdatedEvent
        var customerUpdatedEvent = new CustomerUpdatedEvent
        {
            CustomerId = customer.Id,
            Name = $"{customer.FirstName} {customer.LastName}",
            Email = customer.Email
        };

        await _publishEndpoint.Publish(customerUpdatedEvent);
        _logger.LogInformation("Published CustomerUpdatedEvent for Customer {CustomerId}", customer.Id);

        return true;
    }

    public async Task<bool> DeleteCustomerAsync(long id)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null) return false;

        await DeleteAsync(customer);

        // Publish CustomerDeletedEvent
        var customerDeletedEvent = new CustomerDeletedEvent
        {
            CustomerId = id
        };

        await _publishEndpoint.Publish(customerDeletedEvent);
        _logger.LogInformation("Published CustomerDeletedEvent for Customer {CustomerId}", id);

        return true;
    }
}
