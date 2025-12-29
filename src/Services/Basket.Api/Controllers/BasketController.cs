using Basket.Api.DTOs;
using Basket.Api.Entities;
using Basket.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Basket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketRepository basketRepository, ILogger<BasketController> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<ApiResponse<ShoppingCartDto>>> GetBasket(string userName)
    {
        _logger.LogInformation("Getting basket for user: {UserName}", userName);
        var basket = await _basketRepository.GetBasketAsync(userName);

        if (basket == null)
        {
            // Return empty basket if not found
            basket = new ShoppingCart(userName);
        }

        var dto = MapToDto(basket);
        return Ok(ApiResponse<ShoppingCartDto>.SuccessResult(dto, "Basket retrieved successfully"));
    }

    [HttpPost("{userName}/items")]
    public async Task<ActionResult<ApiResponse<ShoppingCartDto>>> AddItem(string userName, [FromBody] AddItemDto dto)
    {
        _logger.LogInformation("Adding item to basket for user: {UserName}", userName);
        
        var basket = await _basketRepository.GetBasketAsync(userName) ?? new ShoppingCart(userName);
        
        var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        
        if (existingItem != null)
        {
            // Update quantity if item already exists
            existingItem.Quantity += dto.Quantity;
        }
        else
        {
            // Add new item
            basket.Items.Add(new ShoppingCartItem
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl
            });
        }

        var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);
        var result = MapToDto(updatedBasket);
        
        return Ok(ApiResponse<ShoppingCartDto>.SuccessResult(result, "Item added to basket successfully"));
    }

    [HttpPut("{userName}/items/{productId}")]
    public async Task<ActionResult<ApiResponse<ShoppingCartDto>>> UpdateItemQuantity(
        string userName, 
        long productId, 
        [FromBody] UpdateItemQuantityDto dto)
    {
        _logger.LogInformation("Updating item quantity in basket for user: {UserName}", userName);
        
        var basket = await _basketRepository.GetBasketAsync(userName);
        
        if (basket == null)
        {
            return NotFound(ApiResponse<ShoppingCartDto>.FailureResult($"Basket not found for user {userName}"));
        }

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        
        if (item == null)
        {
            return NotFound(ApiResponse<ShoppingCartDto>.FailureResult($"Item {productId} not found in basket"));
        }

        if (dto.Quantity <= 0)
        {
            // Remove item if quantity is 0 or negative
            basket.Items.Remove(item);
        }
        else
        {
            item.Quantity = dto.Quantity;
        }

        var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);
        var result = MapToDto(updatedBasket);
        
        return Ok(ApiResponse<ShoppingCartDto>.SuccessResult(result, "Item quantity updated successfully"));
    }

    [HttpDelete("{userName}/items/{productId}")]
    public async Task<ActionResult<ApiResponse<ShoppingCartDto>>> RemoveItem(string userName, long productId)
    {
        _logger.LogInformation("Removing item from basket for user: {UserName}", userName);
        
        var basket = await _basketRepository.GetBasketAsync(userName);
        
        if (basket == null)
        {
            return NotFound(ApiResponse<ShoppingCartDto>.FailureResult($"Basket not found for user {userName}"));
        }

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        
        if (item == null)
        {
            return NotFound(ApiResponse<ShoppingCartDto>.FailureResult($"Item {productId} not found in basket"));
        }

        basket.Items.Remove(item);
        var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);
        var result = MapToDto(updatedBasket);
        
        return Ok(ApiResponse<ShoppingCartDto>.SuccessResult(result, "Item removed from basket successfully"));
    }

    [HttpDelete("{userName}")]
    public async Task<ActionResult<ApiResponse>> ClearBasket(string userName)
    {
        _logger.LogInformation("Clearing basket for user: {UserName}", userName);
        var deleted = await _basketRepository.DeleteBasketAsync(userName);
        
        if (!deleted)
        {
            return NotFound(ApiResponse.FailureResult($"Basket not found for user {userName}"));
        }

        return Ok(ApiResponse.SuccessResult("Basket cleared successfully"));
    }

    [HttpPost("{userName}/checkout")]
    public async Task<ActionResult<ApiResponse>> Checkout(string userName, [FromBody] CheckoutDto dto)
    {
        _logger.LogInformation("Checking out basket for user: {UserName}", userName);
        
        var basket = await _basketRepository.GetBasketAsync(userName);
        
        if (basket == null || !basket.Items.Any())
        {
            return BadRequest(ApiResponse.FailureResult("Basket is empty"));
        }

        // In a real system, you would:
        // 1. Create an order in Ordering.Api
        // 2. Publish OrderCreatedEvent
        // 3. Clear the basket
        // For now, just clear the basket
        
        await _basketRepository.DeleteBasketAsync(userName);
        
        return Ok(ApiResponse.SuccessResult("Checkout completed successfully"));
    }

    private static ShoppingCartDto MapToDto(ShoppingCart basket)
    {
        return new ShoppingCartDto
        {
            UserName = basket.UserName,
            TotalPrice = basket.TotalPrice,
            Items = basket.Items.Select(i => new ShoppingCartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }
}
