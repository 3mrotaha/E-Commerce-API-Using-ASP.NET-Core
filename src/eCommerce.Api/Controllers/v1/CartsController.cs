using eCommerce.Api.Controllers;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1;

public class CartsController : CustomControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetCart([FromRoute] Guid userId)
    {
        EnsureCanAccessUserScope(userId);

        var cart = await _cartService.GetCartByUserAsync(userId);
        return ToActionResult(cart);
    }

    [HttpPost("user/{userId:guid}/items")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> AddItem([FromRoute] Guid userId, [FromBody] AddCartItemDto addCartItemDto)
    {
        EnsureCanAccessUserScope(userId);

        var cart = await _cartService.AddItemAsync(userId, addCartItemDto);
        return ToActionResult(cart);
    }

    [HttpPut("user/{userId:guid}/items/{cartItemId:int}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> UpdateItem([FromRoute] Guid userId, [FromRoute] int cartItemId, [FromBody] UpdateCartItemDto updateCartItemDto)
    {
        EnsureCanAccessUserScope(userId);

        var cart = await _cartService.UpdateItemAsync(userId, cartItemId, updateCartItemDto);
        return ToActionResult(cart);
    }

    [HttpDelete("user/{userId:guid}/items/{cartItemId:int}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> RemoveItem([FromRoute] Guid userId, [FromRoute] int cartItemId)
    {
        EnsureCanAccessUserScope(userId);

        var cart = await _cartService.RemoveItemAsync(userId, cartItemId);
        return ToActionResult(cart);
    }

    [HttpDelete("user/{userId:guid}/items")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> EmptyCart([FromRoute] Guid userId)
    {
        EnsureCanAccessUserScope(userId);

        var cart = await _cartService.EmptyCartAsync(userId);
        return ToActionResult(cart);
    }

    private void EnsureCanAccessUserScope(Guid userId)
    {
        if (!IsPrivilegedAccount() && !IsCurrentUser(userId))
        {
            throw new UnautherizedException("You are not authorized to access this user's cart.");
        }
    }
}
