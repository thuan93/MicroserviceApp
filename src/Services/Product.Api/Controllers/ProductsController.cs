using Microsoft.AspNetCore.Mvc;
using Product.Api.DTOs;
using Product.Api.Repositories.Interfaces;

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
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        var products = await _productRepository.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        _logger.LogInformation("Getting product by id: {Id}", id);
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
        {
            _logger.LogWarning("Product with id {Id} not found", id);
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating new product: {Name}", dto.Name);
        var product = await _productRepository.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product with id: {Id}", id);
        var result = await _productRepository.UpdateAsync(id, dto);
        
        if (!result)
        {
            _logger.LogWarning("Product with id {Id} not found for update", id);
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        _logger.LogInformation("Deleting product with id: {Id}", id);
        var result = await _productRepository.DeleteAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Product with id {Id} not found for deletion", id);
            return NotFound();
        }

        return NoContent();
    }
}
