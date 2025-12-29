using EventBus.Messages.Events.Product;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Product.Api.DTOs;
using Product.Api.Persistence;
using Product.Api.Repositories.Interfaces;

namespace Product.Api.Repositories;

public class ProductRepository : RepositoryBase<Entities.Product, ProductContext>, IProductRepository
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ProductContext context, IPublishEndpoint publishEndpoint, ILogger<ProductRepository> logger) 
        : base(context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                SupplierId = p.SupplierId
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(long id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                SupplierId = p.SupplierId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Entities.Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            CreatedDate = DateTime.UtcNow
        };

        await AddAsync(product);

        // Publish ProductCreatedEvent
        var productCreatedEvent = new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId
        };

        await _publishEndpoint.Publish(productCreatedEvent);
        _logger.LogInformation("Published ProductCreatedEvent for Product {ProductId}", product.Id);

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<bool> UpdateProductAsync(long id, UpdateProductDto dto)
    {
        var product = await GetByIdAsync(id);
        if (product == null) return false;

        var oldQuantity = product.StockQuantity;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;
        product.UpdatedDate = DateTime.UtcNow;

        await UpdateAsync(product);

        // Publish ProductUpdatedEvent
        var productUpdatedEvent = new ProductUpdatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId
        };

        await _publishEndpoint.Publish(productUpdatedEvent);
        _logger.LogInformation("Published ProductUpdatedEvent for Product {ProductId}", product.Id);

        // If stock changed, publish StockUpdatedEvent
        if (oldQuantity != product.StockQuantity)
        {
            var stockUpdatedEvent = new ProductStockUpdatedEvent
            {
                ProductId = product.Id,
                OldQuantity = oldQuantity,
                NewQuantity = product.StockQuantity
            };

            await _publishEndpoint.Publish(stockUpdatedEvent);
            _logger.LogInformation("Published ProductStockUpdatedEvent for Product {ProductId}", product.Id);
        }

        return true;
    }

    public async Task<bool> DeleteProductAsync(long id)
    {
        var product = await GetByIdAsync(id);
        if (product == null) return false;

        await DeleteAsync(product);

        // Publish ProductDeletedEvent
        var productDeletedEvent = new ProductDeletedEvent
        {
            ProductId = id
        };

        await _publishEndpoint.Publish(productDeletedEvent);
        _logger.LogInformation("Published ProductDeletedEvent for Product {ProductId}", id);

        return true;
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(long categoryId)
    {
        var products = await FindAsync(p => p.CategoryId == categoryId);
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            SupplierId = p.SupplierId
        });
    }
}
