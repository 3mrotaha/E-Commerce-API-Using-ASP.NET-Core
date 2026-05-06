using System.Text.Json;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Cart;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class CartService : ICartService
{
    private const string CartCacheKeyPrefix = "cart:user:";
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CartService> _logger;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartService> logger, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<CartResponseDto>> GetCartByUserAsync(Guid userId)
    {
        _logger.LogDebug("Getting cart for user {UserId}", userId);

        var cart = await GetCachedCartForUserAsync(userId);
        if (cart == null)
        {
            _logger.LogInformation("Cart for user {UserId} was not found", userId);
            return Result<CartResponseDto>.NotFound("Cart was not found.");
        }

        return Result<CartResponseDto>.Success(cart);
    }

    public async Task<Result<CartResponseDto>> AddItemAsync(Guid userId, AddCartItemDto addCartItemDto)
    {
        _logger.LogDebug("Adding product {ProductId} to cart for user {UserId}", addCartItemDto.ProductId, userId);

        if (addCartItemDto.Quantity <= 0)
        {
            return Result<CartResponseDto>.BadRequest("Cart item quantity must be greater than zero.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(addCartItemDto.ProductId);
        if (product == null)
        {
            return Result<CartResponseDto>.NotFound($"Product with id '{addCartItemDto.ProductId}' was not found.");
        }

        var cart = await GetOrCreateCachedCartForUserAsync(userId);
        if (cart == null)
        {
            _logger.LogError("Failed to create cart for user {UserId}", userId);
            return Result<CartResponseDto>.BadRequest("Failed to create cart.");
        }

        var cachedItems = cart.Items.ToList();
        var existingItem = cachedItems.FirstOrDefault(ci => ci.ProductId == addCartItemDto.ProductId);

        var requestedQuantity = addCartItemDto.Quantity + (existingItem?.Quantity ?? 0);
        if (product.QuantityInStock < requestedQuantity)
        {
            return Result<CartResponseDto>.BadRequest($"Insufficient stock for product '{product.Name}'.");
        }

        if (existingItem == null)
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = addCartItemDto.Quantity,
                UnitPrice = product.UnitPrice,
                UpdatedAt = DateTime.UtcNow
            };

            var createdItem = await _unitOfWork.CartItems.AddAsync(cartItem);
            if (createdItem == null)
            {
                _logger.LogError("Failed to add product {ProductId} to cart {CartId}", product.Id, cart.Id);
                return Result<CartResponseDto>.BadRequest("Failed to add item to cart.");
            }

            cachedItems.Add(ToCartItemResponse(createdItem, product.Name));
        }
        else
        {
            var updatedAt = DateTime.UtcNow;
            var cartItem = new CartItem
            {
                Id = existingItem.Id,
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = requestedQuantity,
                UnitPrice = product.UnitPrice,
                UpdatedAt = updatedAt
            };

            var updatedItem = await _unitOfWork.CartItems.UpdateAsync(cartItem);
            if (updatedItem == null)
            {
                _logger.LogError("Failed to update cart item {CartItemId}", existingItem.Id);
                return Result<CartResponseDto>.BadRequest("Failed to update cart item.");
            }

            var existingItemIndex = cachedItems.FindIndex(ci => ci.Id == existingItem.Id);
            cachedItems[existingItemIndex] = ToCartItemResponse(updatedItem, product.Name);
        }

        cart = await TouchCartAsync(cart, cachedItems);
        _logger.LogInformation("Added product {ProductId} to cart {CartId} for user {UserId}", product.Id, cart.Id, userId);
        return Result<CartResponseDto>.Success(cart);
    }

    public async Task<Result<CartResponseDto>> UpdateItemAsync(Guid userId, int cartItemId, UpdateCartItemDto updateCartItemDto)
    {
        _logger.LogDebug("Updating cart item {CartItemId} for user {UserId}", cartItemId, userId);

        if (updateCartItemDto.Quantity <= 0)
        {
            return Result<CartResponseDto>.BadRequest("Cart item quantity must be greater than zero.");
        }

        var cart = await GetCachedCartForUserAsync(userId);
        if (cart == null)
        {
            return Result<CartResponseDto>.NotFound("Cart was not found.");
        }

        var cachedItems = cart.Items.ToList();
        var cartItem = cachedItems.FirstOrDefault(ci => ci.Id == cartItemId);
        if (cartItem == null)
        {
            return Result<CartResponseDto>.NotFound($"Cart item with id '{cartItemId}' was not found.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
        if (product == null)
        {
            return Result<CartResponseDto>.NotFound($"Product with id '{cartItem.ProductId}' was not found.");
        }

        if (product.QuantityInStock < updateCartItemDto.Quantity)
        {
            return Result<CartResponseDto>.BadRequest($"Insufficient stock for product '{product.Name}'.");
        }

        var updatedAt = DateTime.UtcNow;
        var updatedCartItem = new CartItem
        {
            Id = cartItem.Id,
            CartId = cart.Id,
            ProductId = cartItem.ProductId,
            Quantity = updateCartItemDto.Quantity,
            UnitPrice = product.UnitPrice,
            UpdatedAt = updatedAt
        };

        var updatedItem = await _unitOfWork.CartItems.UpdateAsync(updatedCartItem);
        if (updatedItem == null)
        {
            _logger.LogError("Failed to update cart item {CartItemId}", cartItemId);
            return Result<CartResponseDto>.BadRequest("Failed to update cart item.");
        }

        var existingItemIndex = cachedItems.FindIndex(ci => ci.Id == cartItemId);
        cachedItems[existingItemIndex] = ToCartItemResponse(updatedItem, product.Name);

        cart = await TouchCartAsync(cart, cachedItems);
        _logger.LogInformation("Updated cart item {CartItemId} for user {UserId}", cartItemId, userId);
        return Result<CartResponseDto>.Success(cart);
    }

    public async Task<Result<CartResponseDto>> RemoveItemAsync(Guid userId, int cartItemId)
    {
        _logger.LogDebug("Removing cart item {CartItemId} for user {UserId}", cartItemId, userId);

        var cart = await GetCachedCartForUserAsync(userId);
        if (cart == null)
        {
            return Result<CartResponseDto>.NotFound("Cart was not found.");
        }

        var cachedItems = cart.Items.ToList();
        var cartItem = cachedItems.FirstOrDefault(ci => ci.Id == cartItemId);
        if (cartItem == null)
        {
            return Result<CartResponseDto>.NotFound($"Cart item with id '{cartItemId}' was not found.");
        }

        await _unitOfWork.CartItems.DeleteAsync(ToCartItem(cartItem));
        cachedItems.RemoveAll(ci => ci.Id == cartItemId);
        cart = await TouchCartAsync(cart, cachedItems);

        _logger.LogInformation("Removed cart item {CartItemId} from cart {CartId}", cartItemId, cart.Id);
        return Result<CartResponseDto>.Success(cart);
    }

    public async Task<Result<CartResponseDto>> EmptyCartAsync(Guid userId)
    {
        _logger.LogDebug("Emptying cart for user {UserId}", userId);

        var cart = await GetCachedCartForUserAsync(userId);
        if (cart == null)
        {
            return Result<CartResponseDto>.NotFound("Cart was not found.");
        }

        foreach (var cartItem in cart.Items)
        {
            await _unitOfWork.CartItems.DeleteAsync(ToCartItem(cartItem));
        }

        cart = await TouchCartAsync(cart, []);

        _logger.LogInformation("Emptied cart {CartId} for user {UserId}", cart.Id, userId);
        return Result<CartResponseDto>.Success(cart);
    }

    private async Task<CartResponseDto?> GetCachedCartForUserAsync(Guid userId)
    {
        var cacheKey = GetCartCacheKey(userId);
        var cachedCartJson = await _cache.GetStringAsync(cacheKey);
        if (cachedCartJson != null)
        {
            var cachedCart = JsonSerializer.Deserialize<CartResponseDto>(cachedCartJson);
            if (cachedCart != null)
            {
                return cachedCart;
            }

            _logger.LogWarning("Cached cart for user {UserId} could not be deserialized; refreshing from database", userId);
            await _cache.RemoveAsync(cacheKey);
        }

        var cart = await GetCartForUserAsync(userId);
        if (cart == null)
        {
            return null;
        }

        var cartResponse = await BuildCartResponseAsync(cart);
        await SetCachedCartAsync(cartResponse);
        return cartResponse;
    }

    private async Task<CartResponseDto?> GetOrCreateCachedCartForUserAsync(Guid userId)
    {
        var cachedCart = await GetCachedCartForUserAsync(userId);
        if (cachedCart != null)
        {
            return cachedCart;
        }

        var now = DateTime.UtcNow;
        var createdCart = await _unitOfWork.Carts.AddAsync(new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        });

        if (createdCart == null)
        {
            return null;
        }

        var cartResponse = new CartResponseDto(createdCart.Id, createdCart.UserId, [], createdCart.CreatedAt, createdCart.UpdatedAt);
        await SetCachedCartAsync(cartResponse);
        return cartResponse;
    }

    private async Task<Cart?> GetCartForUserAsync(Guid userId)
    {
        return (await _unitOfWork.Carts.FindAsync(c => c.UserId == userId)).FirstOrDefault();
    }

    private async Task<CartResponseDto> BuildCartResponseAsync(Cart cart)
    {
        var cartItems = (await _unitOfWork.CartItems.FindAsync(ci => ci.CartId == cart.Id)).ToList();

        var productIds = cartItems.Select(ci => ci.ProductId).Distinct().ToList();
        if (productIds.Count > 0)
        {
            var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
            var productLookup = products.ToDictionary(p => p.Id, p => p);

            foreach (var item in cartItems)
            {
                if (productLookup.TryGetValue(item.ProductId, out var product))
                {
                    item.Product = product;
                }
            }
        }

        cart.CartItems = cartItems;
        return _mapper.Map<CartResponseDto>(cart);
    }

    private async Task<CartResponseDto> TouchCartAsync(CartResponseDto cart, IEnumerable<CartItemResponseDto> items)
    {
        var updatedAt = DateTime.UtcNow;
        await _unitOfWork.Carts.UpdateAsync(new Cart
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = updatedAt
        });

        var updatedCart = cart with
        {
            Items = items.ToList(),
            UpdatedAt = updatedAt
        };

        await SetCachedCartAsync(updatedCart);
        return updatedCart;
    }

    private async Task SetCachedCartAsync(CartResponseDto cart)
    {
        await _cache.SetStringAsync(GetCartCacheKey(cart.UserId), JsonSerializer.Serialize(cart));
    }

    private static string GetCartCacheKey(Guid userId)
    {
        return $"{CartCacheKeyPrefix}{userId}";
    }

    private static CartItemResponseDto ToCartItemResponse(CartItem cartItem, string productName)
    {
        return new CartItemResponseDto(
            cartItem.Id,
            cartItem.CartId,
            cartItem.ProductId,
            productName,
            cartItem.Quantity,
            cartItem.UnitPrice,
            cartItem.TotalPrice,
            cartItem.UpdatedAt);
    }

    private static CartItem ToCartItem(CartItemResponseDto cartItem)
    {
        return new CartItem
        {
            Id = cartItem.Id,
            CartId = cartItem.CartId,
            ProductId = cartItem.ProductId,
            Quantity = cartItem.Quantity,
            UnitPrice = cartItem.UnitPrice,
            UpdatedAt = cartItem.UpdatedAt
        };
    }
}
