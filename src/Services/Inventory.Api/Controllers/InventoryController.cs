using Inventory.Api.DTOs;
using Inventory.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryRepository inventoryRepository, ILogger<InventoryController> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetAll()
    {
        _logger.LogInformation("Getting all inventory");
        var inventories = await _inventoryRepository.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<InventoryDto>>.SuccessResult(inventories, "Inventory retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetById(string id)
    {
        _logger.LogInformation("Getting inventory by id: {Id}", id);
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        
        if (inventory == null)
        {
            return NotFound(ApiResponse<InventoryDto>.FailureResult($"Inventory with id {id} not found"));
        }

        return Ok(ApiResponse<InventoryDto>.SuccessResult(inventory, "Inventory retrieved successfully"));
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetByProductId(long productId)
    {
        _logger.LogInformation("Getting inventory for product: {ProductId}", productId);
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        
        if (inventory == null)
        {
            return NotFound(ApiResponse<InventoryDto>.FailureResult($"Inventory for product {productId} not found"));
        }

        return Ok(ApiResponse<InventoryDto>.SuccessResult(inventory, "Inventory retrieved successfully"));
    }

    [HttpGet("lowstock")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetLowStock()
    {
        _logger.LogInformation("Getting low stock products");
        var inventories = await _inventoryRepository.GetLowStockProductsAsync();
        return Ok(ApiResponse<IEnumerable<InventoryDto>>.SuccessResult(inventories, "Low stock products retrieved"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Create([FromBody] CreateInventoryDto dto)
    {
        _logger.LogInformation("Creating inventory for product: {ProductId}", dto.ProductId);
        var inventory = await _inventoryRepository.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, 
            ApiResponse<InventoryDto>.SuccessResult(inventory, "Inventory created successfully"));
    }

    [HttpPut("product/{productId}/stock")]
    public async Task<ActionResult<ApiResponse>> UpdateStock(long productId, [FromBody] UpdateStockDto dto)
    {
        _logger.LogInformation("Updating stock for product {ProductId}: {Quantity}", productId, dto.Quantity);
        var result = await _inventoryRepository.UpdateStockAsync(productId, dto.Quantity);
        
        if (!result)
        {
            return NotFound(ApiResponse.FailureResult($"Inventory for product {productId} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Stock updated successfully"));
    }

    [HttpPost("product/{productId}/reserve")]
    public async Task<ActionResult<ApiResponse>> ReserveStock(long productId, [FromBody] ReserveStockDto dto)
    {
        _logger.LogInformation("Reserving {Quantity} units of product {ProductId} for order {OrderId}",
            dto.Quantity, productId, dto.OrderId);
        
        var result = await _inventoryRepository.ReserveStockAsync(productId, dto);
        
        if (!result)
        {
            return BadRequest(ApiResponse.FailureResult("Failed to reserve stock - insufficient quantity or product not found"));
        }

        return Ok(ApiResponse.SuccessResult("Stock reserved successfully"));
    }

    [HttpPost("product/{productId}/release")]
    public async Task<ActionResult<ApiResponse>> ReleaseStock(long productId, [FromBody] ReleaseStockDto dto)
    {
        _logger.LogInformation("Releasing {Quantity} units of product {ProductId} from order {OrderId}",
            dto.Quantity, productId, dto.OrderId);
        
        var result = await _inventoryRepository.ReleaseStockAsync(productId, dto);
        
        if (!result)
        {
            return BadRequest(ApiResponse.FailureResult("Failed to release stock - insufficient reserved quantity or product not found"));
        }

        return Ok(ApiResponse.SuccessResult("Stock released successfully"));
    }

    [HttpGet("product/{productId}/movements")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockMovementDto>>>> GetStockMovements(long productId)
    {
        _logger.LogInformation("Getting stock movements for product: {ProductId}", productId);
        var movements = await _inventoryRepository.GetStockMovementsAsync(productId);
        return Ok(ApiResponse<IEnumerable<StockMovementDto>>.SuccessResult(movements, "Stock movements retrieved"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(string id)
    {
        _logger.LogInformation("Deleting inventory: {Id}", id);
        var result = await _inventoryRepository.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.FailureResult($"Inventory with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Inventory deleted successfully"));
    }
}
