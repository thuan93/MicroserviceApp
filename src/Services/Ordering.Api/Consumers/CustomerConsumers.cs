using EventBus.Messages.Events.Customer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Persistence;

namespace Ordering.Api.Consumers;

public class CustomerCreatedConsumer : IConsumer<CustomerCreatedEvent>
{
    private readonly OrderingContext _context;
    private readonly ILogger<CustomerCreatedConsumer> _logger;

    public CustomerCreatedConsumer(OrderingContext context, ILogger<CustomerCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received CustomerCreatedEvent: {CustomerId} - {Name}", message.CustomerId, message.Name);

        // Check if customer already exists
        var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == message.CustomerId);
        if (existingCustomer != null)
        {
            _logger.LogWarning("Customer {CustomerId} already exists", message.CustomerId);
            return;
        }

        // Create customer in Ordering database
        var customer = new Entities.Customer
        {
            Id = message.CustomerId,
            Name = message.Name,
            Email = message.Email
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created customer {CustomerId} in Ordering database", message.CustomerId);
    }
}

public class CustomerUpdatedConsumer : IConsumer<CustomerUpdatedEvent>
{
    private readonly OrderingContext _context;
    private readonly ILogger<CustomerUpdatedConsumer> _logger;

    public CustomerUpdatedConsumer(OrderingContext context, ILogger<CustomerUpdatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received CustomerUpdatedEvent: {CustomerId}", message.CustomerId);

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == message.CustomerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", message.CustomerId);
            return;
        }

        customer.Name = message.Name;
        customer.Email = message.Email;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated customer {CustomerId} in Ordering database", message.CustomerId);
    }
}
