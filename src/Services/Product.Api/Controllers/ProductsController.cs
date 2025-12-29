using Microsoft.AspNetCore.Mvc;
using Product.Api.DTOs;
using Product.Api.Repositories.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Product.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        var products = await _productRepository.GetAllProductsAsync();
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products, "Products retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(long id)
    {
        _logger.LogInformation("Getting product by id: {Id}", id);
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product with id {Id} not found", id);
            return NotFound(ApiResponse<ProductDto>.FailureResult($"Product with id {id} not found"));
        }

        return Ok(ApiResponse<ProductDto>.SuccessResult(product, "Product retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating new product: {Name}", dto.Name);
        var product = await _productRepository.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, 
            ApiResponse<ProductDto>.SuccessResult(product, "Product created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse>> Update(long id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with id: {Id}", id);
        var result = await _productRepository.UpdateProductAsync(id, dto);

        if (!result)
        {
            _logger.LogWarning("Product with id {Id} not found for update", id);
            return NotFound(ApiResponse.FailureResult($"Product with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Product updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(long id)
    {
        _logger.LogInformation("Deleting product with id: {Id}", id);
        var result = await _productRepository.DeleteProductAsync(id);

        if (!result)
        {
            _logger.LogWarning("Product with id {Id} not found for deletion", id);
            return NotFound(ApiResponse.FailureResult($"Product with id {id} not found"));
        }

        return Ok(ApiResponse.SuccessResult("Product deleted successfully"));
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(long categoryId)
    {
        _logger.LogInformation("Getting products by category: {CategoryId}", categoryId);
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products, "Products retrieved successfully"));
    }
}
