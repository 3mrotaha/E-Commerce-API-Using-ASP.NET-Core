using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Checkout. Checkout ties several pieces together: it reads the user's cart,
/// finds a payment method, creates an order, and empties the cart. The endpoint still requires an
/// authenticated caller (the global authorization filter applies).
/// </summary>
public class CheckoutControllerTests : IntegrationTestBase
{
    public CheckoutControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Checkout_WithCartAndPaymentMethod_ShouldCreateOrder()
    {
        // Arrange: a user with a saved card and a populated cart.
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}",
            new AddCardPaymentMethodDto("4111111111111111", DateTime.UtcNow.AddYears(2), "123", "Holder"));
        await client.PostAsJsonAsync($"/api/Carts/user/{userId}/items", new AddCartItemDto(SeedIds.YogaMatId, 1));

        // Act
        var response = await client.PostAsync($"/api/Checkout/user/{userId}", content: null);

        // Assert: an order is created (201) from the cart contents.
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await ReadAsAsync<OrderResponseDto>(response);
        order.Items.Should().ContainSingle(i => i.ProductId == SeedIds.YogaMatId);
    }

    [Fact]
    public async Task Checkout_WithoutCart_ShouldReturnNotFound()
    {
        // Arrange: a user who never created a cart.
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        // Act
        var response = await client.PostAsync($"/api/Checkout/user/{userId}", content: null);

        // Assert: no cart => the cart lookup returns NotFound, surfaced as 404.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Checkout_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = CreateAnonymousClient();

        var response = await client.PostAsync($"/api/Checkout/user/{userId}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
