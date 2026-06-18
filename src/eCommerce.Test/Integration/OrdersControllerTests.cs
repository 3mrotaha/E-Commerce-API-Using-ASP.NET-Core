using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.OrderItem;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Domain.Enums;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Orders. Placing an order needs a payment method the user owns plus a real
/// product, so the happy-path tests arrange those first. State changes and deletes are admin-only.
/// </summary>
public class OrdersControllerTests : IntegrationTestBase
{
    public OrdersControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>Creates a card for the user and returns its id; orders must reference an owned payment method.</summary>
    private static async Task<Guid> AddPaymentMethodAsync(HttpClient client, Guid userId)
    {
        var response = await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}",
            new AddCardPaymentMethodDto("4111111111111111", DateTime.UtcNow.AddYears(2), "123", "Holder"));
        var card = await ReadAsAsync<CardPaymentMethodResponseDto>(response);
        return card.Id;
    }

    /// <summary>Places a valid order for the user and returns the created order.</summary>
    private async Task<OrderResponseDto> PlaceOrderAsync(HttpClient client, Guid userId)
    {
        var paymentMethodId = await AddPaymentMethodAsync(client, userId);
        var request = new CreateOrderDto(
            PaymentMethodId: paymentMethodId,
            Items: new[] { new CreateOrderItemDto(SeedIds.YogaMatId, 1) },
            HasDiscount: false,
            DiscountValue: null);

        var response = await client.PostAsJsonAsync($"/api/Orders/user/{userId}", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsAsync<OrderResponseDto>(response);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturnCreated()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var order = await PlaceOrderAsync(client, userId);

        order.UserId.Should().Be(userId);
        order.OrderState.Should().Be(OrderState.PENDING);
        order.Items.Should().ContainSingle(i => i.ProductId == SeedIds.YogaMatId);
    }

    [Fact]
    public async Task CreateOrder_WithNoItems_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var paymentMethodId = await AddPaymentMethodAsync(client, userId);

        var response = await client.PostAsJsonAsync($"/api/Orders/user/{userId}",
            new CreateOrderDto(paymentMethodId, Array.Empty<CreateOrderItemDto>(), false, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithMissingPaymentMethod_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PostAsJsonAsync($"/api/Orders/user/{userId}",
            new CreateOrderDto(SeedIds.NonExistentId, new[] { new CreateOrderItemDto(SeedIds.YogaMatId, 1) }, false, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrdersByUser_ShouldReturnUsersOrders()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        await PlaceOrderAsync(client, userId);

        var response = await client.GetAsync($"/api/Orders/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await ReadAsAsync<List<OrderResponseDto>>(response);
        orders.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOrderById_WhenExists_ShouldReturnOrder()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var order = await PlaceOrderAsync(client, userId);

        var response = await client.GetAsync($"/api/Orders/{order.Id}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await ReadAsAsync<OrderResponseDto>(response);
        fetched.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetOrderById_WhenMissing_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.GetAsync($"/api/Orders/{SeedIds.NonExistentId}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrdersByUser_ForAnotherUser_ShouldReturnUnauthorized()
    {
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var client = CreateUserClient(intruder);

        var response = await client.GetAsync($"/api/Orders/user/{owner}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOrderState_AsAdmin_ShouldReturnUpdatedOrder()
    {
        // Arrange: a user places an order, then an admin moves it forward.
        var userId = Guid.NewGuid();
        var userClient = CreateUserClient(userId);
        var order = await PlaceOrderAsync(userClient, userId);
        var adminClient = CreateSuperAdminClient();

        // Act
        var response = await adminClient.PutAsJsonAsync($"/api/Orders/{order.Id}/state",
            new UpdateOrderStateDto(OrderState.SHIPPED));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<OrderResponseDto>(response);
        updated.OrderState.Should().Be(OrderState.SHIPPED);
    }

    [Fact]
    public async Task UpdateOrderState_AsUser_ShouldReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PutAsJsonAsync($"/api/Orders/{Guid.NewGuid()}/state",
            new UpdateOrderStateDto(OrderState.SHIPPED));

        // The endpoint requires ADMIN/SUPER_ADMIN; a USER is authenticated but forbidden.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOrderState_WhenMissing_ShouldReturnNotFound()
    {
        var adminClient = CreateSuperAdminClient();

        var response = await adminClient.PutAsJsonAsync($"/api/Orders/{SeedIds.NonExistentId}/state",
            new UpdateOrderStateDto(OrderState.SHIPPED));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOrder_AsAdmin_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var userClient = CreateUserClient(userId);
        var order = await PlaceOrderAsync(userClient, userId);
        var adminClient = CreateSuperAdminClient();

        var response = await adminClient.DeleteAsync($"/api/Orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteOrder_WhenMissing_ShouldReturnNotFound()
    {
        var adminClient = CreateSuperAdminClient();

        var response = await adminClient.DeleteAsync($"/api/Orders/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync($"/api/Orders/user/{userId}",
            new CreateOrderDto(Guid.NewGuid(), new[] { new CreateOrderItemDto(SeedIds.YogaMatId, 1) }, false, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
