using Basket.Api.Entities;

namespace Basket.Api.Repositories.Interfaces;

public interface IBasketRepository
{
    Task<ShoppingCart?> GetBasketAsync(string userName);
    Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket);
    Task<bool> DeleteBasketAsync(string userName);
}
