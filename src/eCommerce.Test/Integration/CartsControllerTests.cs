using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.Cart;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Carts. Each test uses its own user id so the per-user cart state (which is
/// cached) never leaks between tests. Carts are created lazily when the first item is added.
/// </summary>
public class CartsControllerTests : IntegrationTestBase
{
    public CartsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCart_WhenNoCartYet_ShouldReturnNotFound()
    {
        // Arrange: a brand-new user has no cart until they add something.
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        // Act
        var response = await client.GetAsync($"/api/Carts/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_WithValidProduct_ShouldCreateCartWithItem()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items",
            new AddCartItemDto(SeedIds.MechanicalKeyboardId, 2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await ReadAsAsync<CartResponseDto>(response);
        cart.Items.Should().ContainSingle(i => i.ProductId == SeedIds.MechanicalKeyboardId && i.Quantity == 2);
    }

    [Fact]
    public async Task AddItem_WithMissingProduct_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items",
            new AddCartItemDto(SeedIds.NonExistentId, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_WithNonPositiveQuantity_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items",
            new AddCartItemDto(SeedIds.MechanicalKeyboardId, 0));

        // The service guards against quantity <= 0 with a Validation result => 400.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateItem_ShouldChangeQuantity()
    {
        // Arrange: seed a cart with one line so there is an item id to target.
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var added = await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items",
            new AddCartItemDto(SeedIds.SmartWatchId, 1));
        var cart = await ReadAsAsync<CartResponseDto>(added);
        var itemId = cart.Items.First().Id;

        // Act
        var response = await client.PutAsJsonAsync($"/api/Carts/user/{userId}/items/{itemId}",
            new UpdateCartItemDto(3));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<CartResponseDto>(response);
        updated.Items.Should().ContainSingle(i => i.Id == itemId && i.Quantity == 3);
    }

    [Fact]
    public async Task RemoveItem_ShouldEmptyTheLine()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var added = await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items",
            new AddCartItemDto(SeedIds.YogaMatId, 1));
        var cart = await ReadAsAsync<CartResponseDto>(added);
        var itemId = cart.Items.First().Id;

        var response = await client.DeleteAsync($"/api/Carts/user/{userId}/items/{itemId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<CartResponseDto>(response);
        updated.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task EmptyCart_ShouldRemoveAllItems()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items", new AddCartItemDto(SeedIds.YogaMatId, 1));
        await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items", new AddCartItemDto(SeedIds.SmartWatchId, 1));

        var response = await client.DeleteAsync($"/api/Carts/user/{userId}/items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var emptied = await ReadAsAsync<CartResponseDto>(response);
        emptied.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCart_ForAnotherUser_ShouldReturnUnauthorized()
    {
        // Arrange: sign in as one user but ask for a different user's cart.
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var client = CreateUserClient(intruder);

        // Act
        var response = await client.GetAsync($"/api/Carts/user/{owner}");

        // Assert: the ownership guard throws, mapped to 401 by the exception middleware.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddItem_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items",
            new AddCartItemDto(SeedIds.YogaMatId, 1));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
