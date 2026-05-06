using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Cart;
using eCommerce.Application.DTOs.CartItem;

namespace eCommerce.Application.Interfaces;

public interface ICartService
{
    Task<Result<CartResponseDto>> GetCartByUserAsync(Guid userId);
    Task<Result<CartResponseDto>> AddItemAsync(Guid userId, AddCartItemDto addCartItemDto);
    Task<Result<CartResponseDto>> UpdateItemAsync(Guid userId, int cartItemId, UpdateCartItemDto updateCartItemDto);
    Task<Result<CartResponseDto>> RemoveItemAsync(Guid userId, int cartItemId);
    Task<Result<CartResponseDto>> EmptyCartAsync(Guid userId);
}
