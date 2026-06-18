using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.OrderItem;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.DTOs.PaymentRecord;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Payments. Payment records are produced as a side effect of placing an
/// order, so each test arranges an order first and then reads the resulting payment back.
/// </summary>
public class PaymentsControllerTests : IntegrationTestBase
{
    public PaymentsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>Adds a card, places an order, and returns the created order (whose PaymentId is the payment record).</summary>
    private async Task<OrderResponseDto> PlaceOrderAsync(HttpClient client, Guid userId)
    {
        var cardResponse = await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}",
            new AddCardPaymentMethodDto("4111111111111111", DateTime.UtcNow.AddYears(2), "123", "Holder"));
        var card = await ReadAsAsync<CardPaymentMethodResponseDto>(cardResponse);

        var orderResponse = await client.PostAsJsonAsync($"/api/Orders/user/{userId}",
            new CreateOrderDto(card.Id, new[] { new CreateOrderItemDto(SeedIds.YogaMatId, 1) }, false, null));
        return await ReadAsAsync<OrderResponseDto>(orderResponse);
    }

    [Fact]
    public async Task GetPaymentsByUser_ShouldReturnUsersPayments()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        await PlaceOrderAsync(client, userId);

        var response = await client.GetAsync($"/api/Payments/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payments = await ReadAsAsync<List<PaymentRecordResponseDto>>(response);
        payments.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPaymentById_WhenExists_ShouldReturnPayment()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var order = await PlaceOrderAsync(client, userId);

        // The order carries the id of the payment record created alongside it.
        var response = await client.GetAsync($"/api/Payments/{order.PaymentId}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payment = await ReadAsAsync<PaymentRecordResponseDto>(response);
        payment.Id.Should().Be(order.PaymentId);
    }

    [Fact]
    public async Task GetPaymentByOrderId_WhenExists_ShouldReturnPayment()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var order = await PlaceOrderAsync(client, userId);

        var response = await client.GetAsync($"/api/Payments/order/{order.Id}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payment = await ReadAsAsync<PaymentRecordResponseDto>(response);
        payment.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetPaymentById_WhenMissing_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.GetAsync($"/api/Payments/{SeedIds.NonExistentId}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaymentsByUser_ForAnotherUser_ShouldReturnUnauthorized()
    {
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var client = CreateUserClient(intruder);

        var response = await client.GetAsync($"/api/Payments/user/{owner}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPaymentsByUser_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"/api/Payments/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
