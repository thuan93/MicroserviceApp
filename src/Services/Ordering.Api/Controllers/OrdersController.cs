using Microsoft.AspNetCore.Mvc;
using Ordering.Api.DTOs;
using Ordering.Api.Repositories.Interfaces;
using Shared.DTOs;

namespace Ordering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetAll()
    {
        _logger.LogInformation("Getting all orders");
        var orders = await _orderRepository.GetAllOrdersAsync();
        return Ok(ApiResponse<IEnumerable<OrderDto>>.SuccessResult(orders, "Orders retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(long id)
    {
        _logger.LogInformation("Getting order by id: {Id}", id);
        var order = await _orderRepository.GetOrderByIdAsync(id);
        
        if (order == null)
        {
            _logger.LogWarning("Order with id {Id} not found", id);
            return NotFound(ApiResponse<OrderDto>.FailureResult($"Order with id {id} not found"));
        }

        return Ok(ApiResponse<OrderDto>.SuccessResult(order, "Order retrieved successfully"));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetByCustomer(long customerId)
    {
        _logger.LogInformation("Getting orders for customer: {CustomerId}", customerId);
        var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId);
        return Ok(ApiResponse<IEnumerable<OrderDto>>.SuccessResult(orders, "Orders retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
    {
        _logger.LogInformation("Creating new order for customer: {CustomerId}", dto.CustomerId);
        
        try
        {
            var order = await _orderRepository.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, 
                ApiResponse<OrderDto>.SuccessResult(order, "Order created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(ApiResponse<OrderDto>.FailureResult(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse>> UpdateStatus(long id, [FromBody] UpdateOrderStatusDto dto)
    {
        _logger.LogInformation("Updating order {Id} status to {Status}", id, dto.Status);
        var result = await _orderRepository.UpdateOrderStatusAsync(id, dto);
        
        if (!result)
        {
            _logger.LogWarning("Order with id {Id} not found for status update", id);
            return NotFound(ApiResponse.FailureResult($"Order with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Order status updated successfully"));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse>> Cancel(long id)
    {
        _logger.LogInformation("Cancelling order: {Id}", id);
        var result = await _orderRepository.CancelOrderAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Order with id {Id} not found for cancellation", id);
            return NotFound(ApiResponse.FailureResult($"Order with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Order cancelled successfully"));
    }
}
