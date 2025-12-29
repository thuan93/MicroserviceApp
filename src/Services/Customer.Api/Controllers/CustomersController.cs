using Microsoft.AspNetCore.Mvc;
using Customer.Api.DTOs;
using Customer.Api.Repositories.Interfaces;
using Shared.DTOs;

namespace Customer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerRepository customerRepository, ILogger<CustomersController> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetAll()
    {
        _logger.LogInformation("Getting all customers");
        var customers = await _customerRepository.GetAllCustomersAsync();
        return Ok(ApiResponse<IEnumerable<CustomerDto>>.SuccessResult(customers, "Customers retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(long id)
    {
        _logger.LogInformation("Getting customer by id: {Id}", id);
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer with id {Id} not found", id);
            return NotFound(ApiResponse<CustomerDto>.FailureResult($"Customer with id {id} not found"));
        }

        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer, "Customer retrieved successfully"));
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetByEmail(string email)
    {
        _logger.LogInformation("Getting customer by email: {Email}", email);
        var customer = await _customerRepository.GetCustomerByEmailAsync(email);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer with email {Email} not found", email);
            return NotFound(ApiResponse<CustomerDto>.FailureResult($"Customer with email {email} not found"));
        }

        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer, "Customer retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create([FromBody] CreateCustomerDto dto)
    {
        _logger.LogInformation("Creating new customer: {Email}", dto.Email);
        var customer = await _customerRepository.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, 
            ApiResponse<CustomerDto>.SuccessResult(customer, "Customer created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse>> Update(long id, [FromBody] UpdateCustomerDto dto)
    {
        _logger.LogInformation("Updating customer with id: {Id}", id);
        var result = await _customerRepository.UpdateCustomerAsync(id, dto);
        
        if (!result)
        {
            _logger.LogWarning("Customer with id {Id} not found for update", id);
            return NotFound(ApiResponse.FailureResult($"Customer with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Customer updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(long id)
    {
        _logger.LogInformation("Deleting customer with id: {Id}", id);
        var result = await _customerRepository.DeleteCustomerAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Customer with id {Id} not found for deletion", id);
            return NotFound(ApiResponse.FailureResult($"Customer with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Customer deleted successfully"));
    }
}
