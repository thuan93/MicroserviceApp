using System.Text.Json;
using Basket.Api.Entities;
using Basket.Api.Repositories.Interfaces;
using StackExchange.Redis;

namespace Basket.Api.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _redisDb;
    private readonly ILogger<BasketRepository> _logger;

    public BasketRepository(IConnectionMultiplexer redis, ILogger<BasketRepository> logger)
    {
        _redisDb = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        var basket = await _redisDb.StringGetAsync(userName);

        if (basket.IsNullOrEmpty)
        {
            return null;
        }

        var cart = JsonSerializer.Deserialize<ShoppingCart>(basket.ToString());
        _logger.LogInformation("Retrieved basket for user: {UserName}", userName);

        return cart;
    }

    public async Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket)
    {
        var json = JsonSerializer.Serialize(basket);
        var created = await _redisDb.StringSetAsync(basket.UserName, json);

        if (!created)
        {
            _logger.LogError("Failed to update basket for user: {UserName}", basket.UserName);
            throw new Exception("Failed to update basket");
        }

        _logger.LogInformation("Updated basket for user: {UserName}", basket.UserName);
        return await GetBasketAsync(basket.UserName) ?? basket;
    }

    public async Task<bool> DeleteBasketAsync(string userName)
    {
        var deleted = await _redisDb.KeyDeleteAsync(userName);
        
        if (deleted)
        {
            _logger.LogInformation("Deleted basket for user: {UserName}", userName);
        }
        
        return deleted;
    }
}
